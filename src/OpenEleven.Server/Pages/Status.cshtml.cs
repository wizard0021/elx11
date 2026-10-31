// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 ELX11 Team & PES5 Indie Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenEleven.Data;
using OpenEleven.Server.Configuration;
using OpenEleven.Server.State;

namespace OpenEleven.Server.Pages;

public sealed class StatusModel(
    GameDbContext db,
    Hub hub,
    IOptionsMonitor<ServerOptions> options) : PageModel
{
    public sealed record ServiceStatus(string Name, string Role, int Port, string Protocol, bool IsHealthy, string Description);

    public bool AllHealthy => Services.All(s => s.IsHealthy) && DatabaseHealthy;
    public IReadOnlyList<ServiceStatus> Services { get; private set; } = [];
    public bool DatabaseHealthy { get; private set; }
    public TimeSpan Uptime { get; private set; }
    public double MemoryMb { get; private set; }
    public int ActiveConnections { get; private set; }
    public int ActiveRooms { get; private set; }
    public int TotalAccounts { get; private set; }
    public int TotalMatches { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var proc = Process.GetCurrentProcess();
        Uptime = DateTime.UtcNow - proc.StartTime.ToUniversalTime();
        MemoryMb = Math.Round(proc.WorkingSet64 / (1024.0 * 1024.0), 1);
        ActiveConnections = hub.ConnectedPlayerCount();
        ActiveRooms = hub.Rooms.Count;

        // Test Database
        try
        {
            TotalAccounts = await db.Accounts.CountAsync(ct);
            TotalMatches = await db.Matches.CountAsync(m => m.Completed, ct);
            DatabaseHealthy = true;
        }
        catch
        {
            DatabaseHealthy = false;
        }

        // Test Services
        var list = new List<ServiceStatus>();
        var serverOpts = options.CurrentValue;

        // 1. Web Portal & REST APIs
        list.Add(new ServiceStatus("Web Portal & REST APIs", "Web", serverOpts.Listen.Http, "HTTP/TCP", true, "Front-end portal, player authentication, leaderboards, and REST endpoints."));

        // 2. Massive Ad Server
        list.Add(new ServiceStatus("In-Game Ad Server (locate.madserver.net)", "Ads", serverOpts.Listen.Http, "HTTP/TCP", true, "Microsoft Massive protocol emulator for pitch-side hoardings & billboards."));

        // 3. Game Services
        foreach (var s in serverOpts.Services)
        {
            var isListening = CheckTcpPort(s.Port);
            var desc = s.Role switch
            {
                ServiceRole.Gate => "Master discovery, server list negotiation (CMD_GET_SVRLIST), and client version validation.",
                ServiceRole.Lobby => "ELX11 online multiplayer lobbies, matchmaking, and match room creation.",
                ServiceRole.FdLobby => "Friend lobbies and direct 1v1 matchmaking invitations.",
                ServiceRole.Menu => "Main online menu, block lists, and player roster dispatch.",
                ServiceRole.Account => "In-game binary authentication tickets (CMD_AUTH_GAMEID, CMD_LOGIN).",
                ServiceRole.VdpChat => "In-game text and voice chat communication channels.",
                _ => s.Name
            };
            list.Add(new ServiceStatus(s.Name, s.Role.ToString(), s.Port, "TCP", isListening, desc));
        }

        Services = list;
    }

    private static bool CheckTcpPort(int port)
    {
        try
        {
            using var client = new TcpClient();
            var result = client.BeginConnect("127.0.0.1", port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));
            if (!success) return false;
            client.EndConnect(result);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
