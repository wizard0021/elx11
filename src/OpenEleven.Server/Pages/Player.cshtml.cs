// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 PES5 Indie Team <pes5indie@gmail.com>
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using OpenEleven.Data;
using OpenEleven.Data.Entities;
using OpenEleven.Server.Configuration;
using OpenEleven.Server.State;
using OpenEleven.Server.Web;

namespace OpenEleven.Server.Pages;

/// <summary>
/// One player's card. Shows the in-game name only: the account's Game ID is the login name,
/// and publishing it would hand out half of every credential.
/// </summary>
[OutputCache(Duration = 30)]
public sealed class PlayerModel(GameDbContext db, Hub hub, GameProfile game) : PageModel
{
    public bool Ranked => RankSystem.Applies(game);
    public Player? Profile { get; private set; }
    public Account? Account { get; private set; }
    public string DisplayName => Profile?.Name ?? Account?.GameId ?? "Unknown";
    public DateTime MemberSince { get; private set; }
    public bool IsOnline { get; private set; }
    public string LastSeenText { get; private set; } = "Never";
    public IReadOnlyList<MatchRow> Recent { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var query = id.Trim();

        // 1. Try numeric PID
        if (int.TryParse(query, out var pid))
        {
            Profile = await db.Players.Include(p => p.Stats).Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Pid == pid, ct);
        }

        // 2. Try by player name or account gameId
        if (Profile is null)
        {
            var lower = query.ToLower();
            Profile = await db.Players.Include(p => p.Stats).Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.NormalizedName == lower || p.Name.ToLower() == lower || (p.Account != null && p.Account.GameId.ToLower() == lower), ct);
        }

        // 3. If no player avatar exists yet, check if there is a registered Account
        if (Profile is null)
        {
            var lower = query.ToLower();
            Account = await db.Accounts.FirstOrDefaultAsync(a => a.GameId.ToLower() == lower, ct);
            if (Account is null)
                return NotFound();

            MemberSince = Account.CreatedAt;
            IsOnline = false;
            LastSeenText = Account.LastLoginAt.HasValue ? SiteData.TimeAgo(Account.LastLoginAt.Value) : "Never (Pending first game login)";
            return Page();
        }

        Account = Profile.Account;
        MemberSince = Profile.Account?.CreatedAt ?? default;
        IsOnline = SiteData.OnlinePids(hub).Contains(Profile.Pid);
        LastSeenText = IsOnline ? "Online now" : SiteData.TimeAgo(Profile.LastPlayedAt ?? Profile.Account?.LastLoginAt);
        Recent = await SiteData.NameAsync(db,
            SiteData.Finished(db).Where(m => m.HomePid == Profile.Pid || m.AwayPid == Profile.Pid).Take(15), ct);
        return Page();
    }
}
