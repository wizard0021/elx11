// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 PES5 Indie Team <pes5indie@gmail.com>

namespace OpenEleven.Server.Web.Localization;

public interface IWebLocalizer
{
    string this[string key] { get; }
    string this[string key, string fallback] { get; }
    string CurrentCulture { get; }
    LanguageInfo CurrentLanguage { get; }
    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }
}
