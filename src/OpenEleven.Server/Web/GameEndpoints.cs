// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 PES5 Indie Team <pes5indie@gmail.com> & ELX11 Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenEleven.Data.Repositories;
using OpenEleven.Server.Configuration;

namespace OpenEleven.Server.Web;

public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        // Account creation for scripts and tools. The browser form lives at GET/POST
        // /register (Pages/Register.cshtml) and converges on the same RegistrationService;
        // the two are on separate paths because one path cannot serve two POST handlers.
        app.MapPost("/api/register", async (
            RegisterAccountRequest request,
            RegistrationService registrations,
            CaptchaVerifier captcha,
            HttpContext http,
            CancellationToken ct) =>
        {
            // Same gate as the form, or the API would be the way around it.
            if (!await captcha.VerifyAsync(request.CaptchaToken, http.Connection.RemoteIpAddress?.ToString(), ct))
                return Results.BadRequest(new { error = "captchaToken is missing or was not accepted." });

            var outcome = await registrations.RegisterAsync(request, ct);

            return outcome.Failure switch
            {
                RegistrationFailure.Invalid => Results.BadRequest(new { error = outcome.Error }),
                RegistrationFailure.Conflict => Results.Conflict(new { error = outcome.Error }),
                _ => Results.Created(
                    $"/accounts/{outcome.Account!.Id}",
                    new RegisterAccountResponse(
                        outcome.Account.Id, outcome.Account.GameId, outcome.Account.RegCode)),
            };
        }).WithMetadata(new SensitiveBodyAttribute()).RequireRateLimiting(HttpRateLimits.Register);

        // Server.Urls entries backed by a file (the DLC package and friends), streamed with range
        // support so an interrupted download can resume.
        app.MapMethods("/files/{name}", ["GET", "HEAD"], (string name, IOptionsMonitor<ServerOptions> options) =>
            DownloadFiles.Find(options.CurrentValue, name) is { } path && File.Exists(path)
                ? Results.File(path, "application/octet-stream", enableRangeProcessing: true)
                : Results.NotFound());

        // The DOWNLOAD url: an update manifest pointing at the package under /files/.
        // The client appends ?ver=..&ps3=0 to this url, so the language rides in the path.
        app.MapGet("/dlc/manifest/{lang?}", (string? lang, HttpContext http, IOptionsMonitor<ServerOptions> options) =>
        {
            var dlc = options.CurrentValue.Urls.FirstOrDefault(u => u is { Type: "DOWNLOAD", File.Length: > 0 });
            if (dlc is null || !File.Exists(dlc.FullPath))
                return Results.NotFound();
            var package = $"{http.Request.Scheme}://{http.Request.Host}/files/{Path.GetFileName(dlc.File)}";
            var description = dlc.Description.Length > 0 ? dlc.Description : ServerText.Get("dlc.description", lang);
            return Results.Text(DownloadFiles.Manifest(dlc, package, description), "text/plain", Encoding.UTF8);
        });

        // The RANK and RANK_WEEK urls. The client posts the ranking query as the body (some
        // paths may put it in the query string) and wraps our text in ranking_data="...".
        app.MapMethods("/ranking", ["GET", "POST"], async (HttpContext http, OpenEleven.Data.GameDbContext db, TimeProvider time,
            IOptionsMonitor<ServerOptions> options) =>
        {
            var data = http.Request.QueryString.Value ?? "";
            if (HttpMethods.IsPost(http.Request.Method))
            {
                if (http.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limit)
                    limit.MaxRequestBodySize = 4096;
                using var reader = new StreamReader(http.Request.Body, Encoding.ASCII);
                var buffer = new char[4096];
                var read = await reader.ReadBlockAsync(buffer, http.RequestAborted);
                var body = new string(buffer, 0, read);
                if (body.Length > 0)
                    data = body;
            }

            var text = await new State.RankingQuery(db, options.CurrentValue.GameProfile).RenderAsync(data, time.GetUtcNow().UtcDateTime, http.RequestAborted);
            return Results.Text(text, "text/plain", Encoding.UTF8);
        }).RequireRateLimiting(HttpRateLimits.Ranking);

        // The terms screen: the client GETs the EULA_PES url and shows the body (read_buffer="...").
        app.MapMethods("/eula_pes.txt", ["GET", "HEAD"], (HttpContext http, IOptionsMonitor<ServerOptions> options) =>
            Results.Text(HttpMethods.IsHead(http.Request.Method) ? "" : EulaText(options.CurrentValue, http.Request.Query["lang"]),
                "text/plain", Encoding.UTF8));

        // ======================================================================
        // Community Hardware & System Survey (Valve / Steam Hardware Survey Style)
        // Anonymous client telemetry from PES 2011 (/rc, pcspec) and ELX11 Launcher
        // ======================================================================
        app.MapMethods("/rc", ["GET", "POST"], async (HttpContext http, HardwareSurveyService survey, ILoggerFactory loggerFactory) =>
        {
            var log = loggerFactory.CreateLogger("OpenEleven.Survey");
            string body = "";
            try
            {
                using var reader = new StreamReader(http.Request.Body, Encoding.Latin1);
                body = await reader.ReadToEndAsync(http.RequestAborted);
            }
            catch { }

            log.LogInformation("Client telemetry /rc from {Ip}: length={Len}", http.Connection.RemoteIpAddress, body.Length);

            string? pcspec = null;
            if (body.Contains("pcspec="))
            {
                var match = System.Text.RegularExpressions.Regex.Match(body, @"pcspec=([A-Za-z0-9_-]+)");
                if (match.Success) pcspec = match.Groups[1].Value;
            }

            survey.RecordTelemetry(new SurveySubmission(
                Os: null,
                Cpu: null,
                CpuCores: null,
                CpuGhz: null,
                Gpu: null,
                GpuVendor: null,
                RamGb: null,
                VramMb: null,
                Resolution: null,
                PcSpecPreset: pcspec,
                Controller: null,
                Source: "GAME_HTTP_RC",
                ClientIp: http.Connection.RemoteIpAddress?.ToString()
            ));

            return Results.Text("result=\"NOERR\"\0", "text/plain", Encoding.ASCII);
        });

        app.MapMethods("/pcspec.bin", ["GET", "POST", "HEAD"], (HttpContext http, HardwareSurveyService survey, ILoggerFactory loggerFactory) =>
        {
            var log = loggerFactory.CreateLogger("OpenEleven.Survey");
            log.LogInformation("Client pcspec.bin accessed from {Ip} (Method: {Method})", http.Connection.RemoteIpAddress, http.Request.Method);

            if (HttpMethods.IsPost(http.Request.Method))
            {
                survey.RecordTelemetry(new SurveySubmission(
                    Os: null, Cpu: null, CpuCores: null, CpuGhz: null, Gpu: null, GpuVendor: null,
                    RamGb: null, VramMb: null, Resolution: null, PcSpecPreset: "HIGH",
                    Controller: null, Source: "GAME_HTTP_PCSPEC", ClientIp: http.Connection.RemoteIpAddress?.ToString()
                ));
            }

            return Results.Bytes(new byte[] { 0x01, 0x00, 0x00, 0x00 }, "application/octet-stream");
        });

        app.MapPost("/api/survey/report", async (HttpContext http, HardwareSurveyService survey) =>
        {
            SurveySubmission? submission = null;
            if (http.Request.HasJsonContentType())
            {
                try
                {
                    submission = await http.Request.ReadFromJsonAsync<SurveySubmission>(http.RequestAborted);
                }
                catch { }
            }
            else
            {
                var form = await http.Request.ReadFormAsync();
                int.TryParse(form["ram_gb"], out var ramGb);
                int.TryParse(form["cpu_cores"], out var cpuCores);
                double.TryParse(form["cpu_ghz"], out var cpuGhz);
                submission = new SurveySubmission(
                    Os: form["os"].ToString(),
                    Cpu: form["cpu"].ToString(),
                    CpuCores: cpuCores > 0 ? cpuCores : null,
                    CpuGhz: cpuGhz > 0 ? cpuGhz : null,
                    Gpu: form["gpu"].ToString(),
                    GpuVendor: form["gpu_vendor"].ToString(),
                    RamGb: ramGb > 0 ? ramGb : null,
                    VramMb: null,
                    Resolution: form["resolution"].ToString(),
                    PcSpecPreset: form["pcspec_preset"].ToString(),
                    Controller: form["controller"].ToString(),
                    Source: form["source"].ToString().Length > 0 ? form["source"].ToString() : "LAUNCHER",
                    ClientIp: http.Connection.RemoteIpAddress?.ToString()
                );
            }

            if (submission is not null)
            {
                var ip = http.Connection.RemoteIpAddress?.ToString();
                submission = submission with { ClientIp = ip };
                survey.RecordTelemetry(submission);
                return Results.Ok(new { success = true, message = "Survey report recorded successfully." });
            }

            return Results.BadRequest(new { error = "Invalid survey payload." });
        });

        app.MapGet("/api/survey/stats", (HttpContext http, HardwareSurveyService survey) =>
        {
            var period = http.Request.Query["period"].ToString();
            return Results.Ok(survey.GetReport(period));
        });

        // Direct download for ELX11 Launcher with caching strictly disabled (ensures players always get v2.0)
        app.MapMethods("/ELX11_Launcher.exe", ["GET", "HEAD"], (HttpContext http) =>
        {
            http.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            http.Response.Headers.Pragma = "no-cache";
            http.Response.Headers.Expires = "0";

            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "ELX11_Launcher.exe"),
                "/home/ubuntu/elx11/src/OpenEleven.Server/wwwroot/ELX11_Launcher.exe",
                "/home/ubuntu/elx11/wwwroot/ELX11_Launcher.exe",
                Path.Combine(AppContext.BaseDirectory, "ELX11_Launcher.exe")
            };

            foreach (var c in candidates)
            {
                if (File.Exists(c))
                    return Results.File(c, "application/vnd.microsoft.portable-executable", "ELX11_Launcher.exe");
            }
            return Results.NotFound();
        });

        app.MapMethods("/ELX11_Launcher_v2.exe", ["GET", "HEAD"], (HttpContext http) =>
        {
            http.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            http.Response.Headers.Pragma = "no-cache";
            http.Response.Headers.Expires = "0";

            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "ELX11_Launcher_v2.exe"),
                "/home/ubuntu/elx11/src/OpenEleven.Server/wwwroot/ELX11_Launcher_v2.exe",
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "ELX11_Launcher.exe"),
                "/home/ubuntu/elx11/src/OpenEleven.Server/wwwroot/ELX11_Launcher.exe"
            };

            foreach (var c in candidates)
            {
                if (File.Exists(c))
                    return Results.File(c, "application/vnd.microsoft.portable-executable", "ELX11_Launcher_v2.exe");
            }
            return Results.NotFound();
        });

        // Static files the client fetches by URL from CMD_GET_URLLIST.
        app.MapMethods("/{file}", ["GET", "HEAD"], (string file, HttpContext http, WebAssets assets) =>
        {
            if (!assets.TryGet("/" + file, out var content))
                return Results.NotFound();

            var body = HttpMethods.IsHead(http.Request.Method) ? Array.Empty<byte>() : content;
            return Results.Bytes(body, "application/octet-stream");
        });

        // The HTTP half of login.
        app.MapPost("/gameid_auth", async (HttpContext http, ILoggerFactory loggerFactory) =>
        {
            var log = loggerFactory.CreateLogger("OpenEleven.GameIdAuth");

            var form = await http.Request.ReadFormAsync();
            var gameId = form["game_id"].ToString();
            if (gameId.Length == 0) gameId = form["konami_id"].ToString();
            var passwordHash = form["game_id_password"].ToString();
            if (passwordHash.Length == 0) passwordHash = form["passwd"].ToString();

            log.LogInformation(
                "GAMEID auth: game_id={GameId} valid_md5={Valid}",
                gameId, GameIdAuthEndpoint.LooksLikeMd5(passwordHash));

            var authenticator = http.RequestServices.GetRequiredService<GameIdAuthService>();
            var result = await authenticator.AuthenticateAsync(
                gameId,
                passwordHash,
                http.Connection.RemoteIpAddress ?? System.Net.IPAddress.None,
                http.RequestAborted);

            if (!result.Success)
                log.LogWarning("GAMEID auth rejected for {GameId}: {Reason}", gameId, result.Reason);

            return Results.Text(result.Body, "text/plain", Encoding.ASCII);
        }).WithMetadata(new SensitiveBodyAttribute());

        // ======================================================================
        // Microsoft Massive In-Game Advertising protocol for stadium hoardings
        // ======================================================================
        app.MapMethods("/adsrv/5/locateService", ["GET", "POST"], (IOptionsMonitor<ServerOptions> options, ILoggerFactory loggerFactory) =>
        {
            var log = loggerFactory.CreateLogger("OpenEleven.MassiveAds");
            var ip = options.CurrentValue.AdvertiseIp;
            if (string.IsNullOrWhiteSpace(ip) || ip == "0.0.0.0") ip = "63.178.232.241";
            var port = options.CurrentValue.Listen.Http;
            log.LogInformation("Massive locateService request from client, advertising {Ip}:{Port}", ip, port);
            var resp = MassiveAdService.BuildLocateResponse(ip, port);
            return Results.Bytes(resp, "application/massive");
        });

        app.MapMethods("/adsrv/5/openSession", ["GET", "POST"], (ILoggerFactory loggerFactory) =>
        {
            var log = loggerFactory.CreateLogger("OpenEleven.MassiveAds");
            log.LogInformation("Massive openSession request from client");
            var resp = MassiveAdService.BuildOpenSessionResponse();
            return Results.Bytes(resp, "application/massive");
        });

        app.MapMethods("/adsrv/5/enterZone", ["GET", "POST"], (IOptionsMonitor<ServerOptions> options, ILoggerFactory loggerFactory) =>
        {
            var log = loggerFactory.CreateLogger("OpenEleven.MassiveAds");
            var ip = options.CurrentValue.AdvertiseIp;
            if (string.IsNullOrWhiteSpace(ip) || ip == "0.0.0.0") ip = "63.178.232.241";
            var port = options.CurrentValue.Listen.Http;
            var adsDir = MassiveAdService.AdsFolder;
            log.LogInformation("Massive enterZone request: streaming hoardings from {Dir}", adsDir);
            var resp = MassiveAdService.BuildEnterZoneResponse(ip, port, adsDir);
            return Results.Bytes(resp, "application/massive");
        });

        app.MapGet("/adsrv/assets/{filename}", (string filename) =>
        {
            var safeName = Path.GetFileName(filename);
            var path = Path.Combine(MassiveAdService.AdsFolder, safeName);
            if (!File.Exists(path)) return Results.NotFound();
            var ext = Path.GetExtension(safeName).ToLowerInvariant();
            var mime = ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
            return Results.File(path, mime);
        });

        // ======================================================================
        // Protected Bot & CLI REST API endpoints (Requires secret API key or Localhost)
        // ======================================================================
        var botApiKey = Environment.GetEnvironmentVariable("ELX11_BOT_API_KEY") ?? "elx11_bot_secret_2026";

        bool IsBotAuthorized(HttpContext http)
        {
            if (http.Connection.RemoteIpAddress is { } ip && System.Net.IPAddress.IsLoopback(ip))
                return true;

            if (http.Request.Headers.TryGetValue("X-Api-Key", out var hVal) && hVal == botApiKey)
                return true;

            if (http.Request.Query.TryGetValue("key", out var qVal) && qVal == botApiKey)
                return true;

            return false;
        }

        app.MapGet("/api/bot/status", async (HttpContext http, OpenEleven.Data.GameDbContext db, OpenEleven.Server.State.Hub hub, CancellationToken ct) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();

            var onlinePlayers = hub.ConnectedPlayerCount();
            var activeMatches = hub.Rooms.Count(r => r.Status == "GAME");
            var activeRooms = hub.Rooms.Count;
            var accountsCount = await db.Accounts.CountAsync(ct);
            var matchesCount = await db.Matches.CountAsync(m => m.Completed, ct);

            return Results.Ok(new
            {
                status = "online",
                online_players = onlinePlayers,
                active_matches = activeMatches,
                active_rooms = activeRooms,
                total_accounts = accountsCount,
                total_matches = matchesCount,
                timestamp = DateTime.UtcNow
            });
        });

        app.MapGet("/api/bot/events", (HttpContext http, MatchEventService events) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();
            long.TryParse(http.Request.Query["since"], out var since);
            return Results.Ok(events.GetSince(since));
        });

        app.MapGet("/api/bot/market/movers", (HttpContext http, MloMarketService market) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();
            var (gainers, droppers, moved) = market.GetTopMovers();
            return Results.Ok(new
            {
                prices_moved = moved,
                gainers = gainers,
                droppers = droppers,
                timestamp = DateTime.UtcNow
            });
        });

        app.MapGet("/api/bot/market/summary", (HttpContext http, MloMarketService market) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();
            return Results.Ok(market.GetMarketSummary());
        });

        app.MapGet("/api/bot/market/player/{query}", (string query, HttpContext http, MloMarketService market) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();
            var p = market.FindPlayer(query);
            if (p is null) return Results.NotFound(new { found = false, error = $"Player '{query}' not found in MLO market." });
            return Results.Ok(new
            {
                found = true,
                id = p.Id,
                name = p.Name,
                position = p.Position,
                club = p.Club,
                nationality = p.Nationality,
                price = p.CurrentPrice,
                last_week_price = p.LastWeekPrice,
                change = p.PriceChange,
                change_percent = p.ChangePercent,
                popularity_rank = p.PopularityRank
            });
        });

        app.MapMethods("/api/bot/market/shift", ["GET", "POST"], (HttpContext http, MloMarketService market, MatchEventService events) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();
            var (gainers, droppers, moved) = market.SimulateMarketShift();
            events.Publish("market_update", new()
            {
                ["prices_moved"] = moved,
                ["gainers"] = gainers.Take(5).Select(g => new Dictionary<string, object?> { ["name"] = g.Name, ["price"] = g.CurrentPrice, ["change"] = g.PriceChange, ["percent"] = g.ChangePercent }).ToArray(),
                ["droppers"] = droppers.Take(5).Select(d => new Dictionary<string, object?> { ["name"] = d.Name, ["price"] = d.CurrentPrice, ["change"] = d.PriceChange, ["percent"] = d.ChangePercent }).ToArray()
            });
            return Results.Ok(new { success = true, prices_moved = moved });
        });

        app.MapGet("/api/bot/survey", (HttpContext http, HardwareSurveyService survey) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();
            return Results.Ok(survey.GetSummary());
        });

        app.MapGet("/api/bot/leaderboard", async (HttpContext http, OpenEleven.Data.GameDbContext db, CancellationToken ct) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();

            var top = await db.Players
                .Include(p => p.Stats)
                .OrderByDescending(p => p.Rating)
                .Take(10)
                .Select(p => new
                {
                    pid = p.Pid,
                    name = p.Name,
                    division = p.Division,
                    rating = p.Rating,
                    points = p.Point,
                    matches = p.Stats != null ? p.Stats.MatchCount : 0,
                    wins = p.Stats != null ? p.Stats.WinCount : 0
                })
                .ToListAsync(ct);

            return Results.Ok(top);
        });

        app.MapGet("/api/bot/player/{query}", async (string query, HttpContext http, OpenEleven.Data.GameDbContext db, OpenEleven.Server.State.Hub hub, CancellationToken ct) =>
        {
            if (!IsBotAuthorized(http)) return Results.Unauthorized();

            var clean = query.Trim().ToLower();
            var onlinePids = SiteData.OnlinePids(hub);

            var player = await db.Players
                .Include(p => p.Stats)
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.NormalizedName.ToLower() == clean || (p.Account != null && p.Account.GameId.ToLower() == clean), ct);

            if (player is not null)
            {
                var s = player.Stats;
                return Results.Ok(new
                {
                    found = true,
                    is_in_game_player = true,
                    pid = player.Pid,
                    game_id = player.Account?.GameId ?? player.Name,
                    player_name = player.Name,
                    division = player.Division,
                    rating = player.Rating,
                    is_online = onlinePids.Contains(player.Pid),
                    matches = s?.MatchCount ?? 0,
                    wins = s?.WinCount ?? 0,
                    draws = s?.DrawCount ?? 0,
                    losses = s?.LoseCount ?? 0,
                    win_rate = s != null && s.MatchCount > 0 ? Math.Round((double)s.WinCount / s.MatchCount * 100, 1) : 0,
                    goals = s?.Goals ?? 0,
                    goals_against = s?.GoalsAgainst ?? 0,
                    last_played = player.LastPlayedAt
                });
            }

            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.GameId.ToLower() == clean, ct);

            if (account is not null)
            {
                return Results.Ok(new
                {
                    found = true,
                    is_in_game_player = false,
                    game_id = account.GameId,
                    player_name = account.GameId,
                    division = "D1",
                    rating = 1000,
                    is_online = false,
                    matches = 0,
                    wins = 0,
                    draws = 0,
                    losses = 0,
                    win_rate = 0.0,
                    goals = 0,
                    goals_against = 0,
                    status = "Registered, waiting for first in-game login"
                });
            }

            return Results.NotFound(new { found = false, error = $"Player '{query}' not found." });
        });

                        // Konami Service Information / Notice endpoints (e.g. /VT031-E1/info/info_sp.txt)
        app.MapGet("/{product}/info/{filename}", (string product, string filename) => Results.NotFound());

        app.MapGet("/info/{filename}", (string filename) => Results.NotFound());

        app.MapFallback((HttpContext http) =>
        {
            if (http.Request.Path.Value?.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Results.Text(@"OK
", "text/plain; charset=utf-8", Encoding.UTF8);
            }
            return Results.NotFound();
        });

        return app;
    }

    public static string EulaText(ServerOptions options, string? lang)
    {
        var eula = options.Protocol.EulaText;
        string text;
        if (eula.Length == 0)
        {
            text = ServerText.Get("eula.text", lang);
        }
        else if (File.Exists(eula))
        {
            if (lang is null || lang.Length is < 2 or > 3 || !lang.All(char.IsAsciiLetter))
                lang = "EN";
            var localized = Path.Combine(Path.GetDirectoryName(eula) ?? "",
                $"{Path.GetFileNameWithoutExtension(eula)}.{lang.ToUpperInvariant()}{Path.GetExtension(eula)}");
            text = File.ReadAllText(File.Exists(localized) ? localized : eula);
        }
        else
        {
            text = eula;
        }

        return text.Replace("\"", "'");
    }
}
