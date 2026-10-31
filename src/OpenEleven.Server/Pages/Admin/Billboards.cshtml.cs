// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 ELX11 Team & PES5 Indie Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenEleven.Server.Web;

namespace OpenEleven.Server.Pages.Admin;

public sealed class BillboardsModel(ILogger<BillboardsModel> log) : PageModel
{
    public sealed record AdboardItem(string Name, string Url, long SizeBytes, int Width, int Height);

    public IReadOnlyList<AdboardItem> Items { get; private set; } = [];
    public string? Message { get; private set; }
    public string? Error { get; private set; }

    public void OnGet()
    {
        LoadItems();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            Error = "Please select a file to upload.";
            LoadItems();
            return Page();
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".png" or ".jpg" or ".jpeg" or ".bmp" or ".dds"))
        {
            Error = "Supported formats are PNG, JPG, BMP, DDS.";
            LoadItems();
            return Page();
        }

        var fileName = Path.GetFileName(file.FileName);
        fileName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
        var targetPath = Path.Combine(MassiveAdService.AdsFolder, fileName);

        await using (var stream = new FileStream(targetPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        log.LogInformation("Admin uploaded new billboard asset: {FileName}", fileName);
        Message = $"Uploaded {fileName} successfully!";
        LoadItems();
        return Page();
    }

    public IActionResult OnPostDelete(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            LoadItems();
            return Page();
        }

        var safeName = Path.GetFileName(fileName);
        var targetPath = Path.Combine(MassiveAdService.AdsFolder, safeName);
        if (System.IO.File.Exists(targetPath))
        {
            System.IO.File.Delete(targetPath);
            log.LogInformation("Admin deleted billboard asset: {FileName}", safeName);
            Message = $"Deleted {safeName} successfully.";
        }

        LoadItems();
        return Page();
    }

    private void LoadItems()
    {
        var dir = MassiveAdService.AdsFolder;
        if (!Directory.Exists(dir))
        {
            Items = [];
            return;
        }

        var list = new List<AdboardItem>();
        foreach (var file in Directory.GetFiles(dir))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".dds")
            {
                var name = Path.GetFileName(file);
                var bytes = System.IO.File.ReadAllBytes(file);
                var (w, h) = MassiveAdService.GetImageDimensions(bytes);
                list.Add(new AdboardItem(name, $"/adsrv/assets/{name}", bytes.Length, w, h));
            }
        }
        Items = list.OrderBy(i => i.Name).ToList();
    }
}
