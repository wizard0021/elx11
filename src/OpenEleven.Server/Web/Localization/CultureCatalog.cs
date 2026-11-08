// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 PES5 Indie Team <pes5indie@gmail.com>

namespace OpenEleven.Server.Web.Localization;

public record LanguageInfo(string Code, string NativeName, string EnglishName, string Flag);

public static class CultureCatalog
{
    public const string DefaultCulture = "en";

    public static readonly LanguageInfo[] SupportedLanguages =
    [
        new("en", "English", "English", "🇬🇧"),
        new("tr", "Türkçe", "Turkish", "🇹🇷"),
        new("de", "Deutsch", "German", "🇩🇪"),
        new("fr", "Français", "French", "🇫🇷"),
        new("es", "Español", "Spanish", "🇪🇸"),
        new("it", "Italiano", "Italian", "🇮🇹"),
        new("pt", "Português", "Portuguese", "🇵🇹"),
        new("nl", "Nederlands", "Dutch", "🇳🇱"),
    ];

    public static readonly string[] SupportedCultureCodes = SupportedLanguages.Select(l => l.Code).ToArray();

    public static bool IsSupported(string? code)
        => !string.IsNullOrWhiteSpace(code) && SupportedCultureCodes.Contains(code.Trim().ToLowerInvariant());

    public static LanguageInfo Resolve(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return SupportedLanguages[0];

        var clean = code.Trim().ToLowerInvariant();
        return SupportedLanguages.FirstOrDefault(l => l.Code == clean) ?? SupportedLanguages[0];
    }
}
