// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 OpenEleven Contributors & ELX11 Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using OpenEleven.Server.Web;

namespace OpenEleven.Server.Pages;

[OutputCache(Duration = 30)]
public sealed class SurveyModel(HardwareSurveyService surveyService) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "period")]
    public string? Period { get; set; }

    public SurveyReport Report { get; private set; } = null!;

    public void OnGet()
    {
        Report = surveyService.GetReport(Period);
    }
}
