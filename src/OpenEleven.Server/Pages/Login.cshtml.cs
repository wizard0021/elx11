// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 ELX11 Team & PES5 Indie Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenEleven.Data;
using OpenEleven.Protocol.Crypto;
using OpenEleven.Server.Web;

namespace OpenEleven.Server.Pages;

[SensitiveBody]
public sealed class LoginModel(GameDbContext db, ILogger<LoginModel> log) : PageModel
{
    [BindProperty] public string? GameId { get; set; }
    [BindProperty] public string? Password { get; set; }
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public string? Error { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(GameId) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Please enter both Game ID and Password.";
            return Page();
        }

        var normalizedGameId = GameId.Trim();
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.GameId.ToLower() == normalizedGameId.ToLower());

        if (account is null)
        {
            log.LogWarning("Web player sign-in failed: unknown Game ID '{GameId}'", GameId);
            Error = "Invalid Game ID or password.";
            Password = null;
            return Page();
        }

        if (account.Banned)
        {
            log.LogWarning("Web player sign-in refused: banned account '{GameId}'", account.GameId);
            Error = $"This account is suspended: {account.BanReason ?? "Violation of terms"}";
            Password = null;
            return Page();
        }

        var hash = AuthProof.HashPassword(Password);
        if (!string.Equals(account.PasswordHash, hash, StringComparison.OrdinalIgnoreCase))
        {
            log.LogWarning("Web player sign-in failed: wrong password for '{GameId}'", account.GameId);
            Error = "Invalid Game ID or password.";
            Password = null;
            return Page();
        }

        // Issue Player cookie
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.GameId),
            new(ClaimTypes.Role, "Player")
        };
        var identity = new ClaimsIdentity(claims, "Player");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Player", principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        });

        log.LogInformation("Player '{GameId}' signed in successfully on web portal", account.GameId);

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) && !ReturnUrl.StartsWith("/admin"))
            return LocalRedirect(ReturnUrl);

        return Redirect($"/players/{account.GameId}");
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync("Player");
        return Redirect("/");
    }
}
