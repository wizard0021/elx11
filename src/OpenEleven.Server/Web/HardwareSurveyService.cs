// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 OpenEleven Contributors & ELX11 Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenEleven.Server.Web;

public sealed record SurveyItem(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("percentage")] double Percentage,
    [property: JsonPropertyName("change")] double Change
);

public sealed record SurveyCategory(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("icon")] string Icon,
    [property: JsonPropertyName("items")] List<SurveyItem> Items
);

public sealed record SurveyReport(
    [property: JsonPropertyName("period")] string Period,
    [property: JsonPropertyName("sample_count")] int SampleCount,
    [property: JsonPropertyName("last_updated")] DateTimeOffset LastUpdated,
    [property: JsonPropertyName("categories")] List<SurveyCategory> Categories
);

public sealed record SurveySubmission(
    [property: JsonPropertyName("os")] string? Os,
    [property: JsonPropertyName("cpu")] string? Cpu,
    [property: JsonPropertyName("cpu_cores")] int? CpuCores,
    [property: JsonPropertyName("cpu_ghz")] double? CpuGhz,
    [property: JsonPropertyName("gpu")] string? Gpu,
    [property: JsonPropertyName("gpu_vendor")] string? GpuVendor,
    [property: JsonPropertyName("ram_gb")] int? RamGb,
    [property: JsonPropertyName("vram_mb")] int? VramMb,
    [property: JsonPropertyName("resolution")] string? Resolution,
    [property: JsonPropertyName("pcspec_preset")] string? PcSpecPreset,
    [property: JsonPropertyName("controller")] string? Controller,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("client_ip")] string? ClientIp
);

public sealed class HardwareSurveyService
{
    private readonly object _lock = new();
    private readonly string _storagePath;
    private int _sampleCount = 0;
    private DateTimeOffset _lastUpdated = DateTimeOffset.UtcNow;
    private readonly string _currentPeriod = "October 2026";

    // Category -> (ItemName -> Count)
    private readonly Dictionary<string, Dictionary<string, int>> _categoryCounts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Operating System"] = new(StringComparer.OrdinalIgnoreCase),
        ["Direct3D Video Card Brands"] = new(StringComparer.OrdinalIgnoreCase),
        ["Popular Graphics Cards"] = new(StringComparer.OrdinalIgnoreCase),
        ["System RAM"] = new(StringComparer.OrdinalIgnoreCase),
        ["Physical CPU Cores"] = new(StringComparer.OrdinalIgnoreCase),
        ["Primary Display Resolution"] = new(StringComparer.OrdinalIgnoreCase),
        ["In-Game Graphic Presets"] = new(StringComparer.OrdinalIgnoreCase),
        ["Controller / Input Type"] = new(StringComparer.OrdinalIgnoreCase),
    };

    public HardwareSurveyService()
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        if (!Directory.Exists(dataDir))
        {
            try { Directory.CreateDirectory(dataDir); } catch { }
        }
        _storagePath = Path.Combine(dataDir, "hardware_survey.json");
        LoadFromFile();
    }

    public SurveyReport GetReport(string? period = null)
    {
        lock (_lock)
        {
            var categories = new List<SurveyCategory>();
            foreach (var (catName, counts) in _categoryCounts)
            {
                var total = counts.Values.Sum();
                var items = counts
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => new SurveyItem(
                        Name: kv.Key,
                        Count: kv.Value,
                        Percentage: total > 0 ? Math.Round((double)kv.Value / total * 100.0, 2) : 0.0,
                        Change: 0.0
                    ))
                    .ToList();

                var icon = catName switch
                {
                    "Operating System" => "os",
                    "Direct3D Video Card Brands" => "gpu-brand",
                    "Popular Graphics Cards" => "gpu",
                    "System RAM" => "ram",
                    "Physical CPU Cores" => "cpu",
                    "Primary Display Resolution" => "display",
                    "In-Game Graphic Presets" => "preset",
                    _ => "gamepad"
                };

                categories.Add(new SurveyCategory(catName, icon, items));
            }

            return new SurveyReport(
                Period: _currentPeriod,
                SampleCount: _sampleCount,
                LastUpdated: _lastUpdated,
                Categories: categories
            );
        }
    }

    public object GetSummary()
    {
        lock (_lock)
        {
            var r = GetReport();
            var osCat = r.Categories.FirstOrDefault(c => c.Category == "Operating System");
            var gpuCat = r.Categories.FirstOrDefault(c => c.Category == "Direct3D Video Card Brands");
            var topGpus = r.Categories.FirstOrDefault(c => c.Category == "Popular Graphics Cards");
            var resCat = r.Categories.FirstOrDefault(c => c.Category == "Primary Display Resolution");
            var ramCat = r.Categories.FirstOrDefault(c => c.Category == "System RAM");
            var presetCat = r.Categories.FirstOrDefault(c => c.Category == "In-Game Graphic Presets");

            return new
            {
                period = r.Period,
                sample_count = r.SampleCount,
                last_updated = r.LastUpdated,
                top_os = osCat?.Items.FirstOrDefault()?.Name ?? "None recorded yet",
                top_os_pct = osCat?.Items.FirstOrDefault()?.Percentage ?? 0.0,
                top_gpu_brand = gpuCat?.Items.FirstOrDefault()?.Name ?? "None recorded yet",
                top_gpu_brand_pct = gpuCat?.Items.FirstOrDefault()?.Percentage ?? 0.0,
                top_gpu = topGpus?.Items.FirstOrDefault()?.Name ?? "None recorded yet",
                top_res = resCat?.Items.FirstOrDefault()?.Name ?? "None recorded yet",
                top_res_pct = resCat?.Items.FirstOrDefault()?.Percentage ?? 0.0,
                top_ram = ramCat?.Items.FirstOrDefault()?.Name ?? "None recorded yet",
                top_preset = presetCat?.Items.FirstOrDefault()?.Name ?? "None recorded yet",
                web_url = "https://elx11.wizcake.space/survey"
            };
        }
    }

    public void RecordTelemetry(SurveySubmission sub)
    {
        lock (_lock)
        {
            _sampleCount++;
            _lastUpdated = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(sub.Os))
            {
                Increment("Operating System", NormalizeOs(sub.Os));
            }

            if (!string.IsNullOrWhiteSpace(sub.Gpu))
            {
                var (vendor, model) = NormalizeGpu(sub.Gpu, sub.GpuVendor);
                Increment("Direct3D Video Card Brands", vendor);
                Increment("Popular Graphics Cards", model);
            }

            if (sub.RamGb.HasValue && sub.RamGb.Value > 0)
            {
                Increment("System RAM", NormalizeRam(sub.RamGb.Value));
            }

            if (sub.CpuCores.HasValue && sub.CpuCores.Value > 0)
            {
                Increment("Physical CPU Cores", NormalizeCores(sub.CpuCores.Value));
            }

            if (!string.IsNullOrWhiteSpace(sub.Resolution))
            {
                Increment("Primary Display Resolution", NormalizeResolution(sub.Resolution));
            }

            if (!string.IsNullOrWhiteSpace(sub.PcSpecPreset))
            {
                Increment("In-Game Graphic Presets", NormalizePreset(sub.PcSpecPreset));
            }

            if (!string.IsNullOrWhiteSpace(sub.Controller))
            {
                Increment("Controller / Input Type", NormalizeController(sub.Controller));
            }

            SaveToFile();
        }
    }

    private void Increment(string category, string item)
    {
        if (!_categoryCounts.TryGetValue(category, out var dict))
        {
            dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _categoryCounts[category] = dict;
        }

        dict[item] = dict.GetValueOrDefault(item, 0) + 1;
    }

    private static string NormalizeOs(string os)
    {
        var lower = os.ToLowerInvariant();
        if (lower.Contains("11")) return "Windows 11 64 bit";
        if (lower.Contains("10")) return "Windows 10 64 bit";
        if (lower.Contains("linux") || lower.Contains("wine") || lower.Contains("proton") || lower.Contains("deck")) return "Linux / Steam Deck (Wine / Proton)";
        if (lower.Contains("7")) return "Windows 7 64 bit";
        if (lower.Contains("8")) return "Windows 8.1 64 bit";
        return os;
    }

    private static (string Vendor, string Model) NormalizeGpu(string gpu, string? vendorHint)
    {
        var lower = gpu.ToLowerInvariant();
        var vLower = (vendorHint ?? "").ToLowerInvariant();

        string vendor = "Other";
        if (lower.Contains("nvidia") || lower.Contains("geforce") || lower.Contains("rtx") || lower.Contains("gtx") || vLower.Contains("nvidia"))
            vendor = "NVIDIA";
        else if (lower.Contains("radeon") || lower.Contains("amd") || vLower.Contains("amd"))
            vendor = "AMD";
        else if (lower.Contains("intel") || lower.Contains("iris") || lower.Contains("arc") || lower.Contains("uhd") || vLower.Contains("intel"))
            vendor = "Intel";

        string model = gpu.Trim();
        if (model.Length > 36) model = model[..36];

        return (vendor, model);
    }

    private static string NormalizeRam(int gb)
    {
        if (gb >= 48) return "64 GB or higher";
        if (gb >= 24) return "32 GB";
        if (gb >= 12) return "16 GB";
        if (gb >= 6) return "8 GB";
        return $"{gb} GB or less";
    }

    private static string NormalizeCores(int cores)
    {
        if (cores >= 12) return "12 Cores or more";
        if (cores >= 8) return "8 Cores";
        if (cores >= 6) return "6 Cores";
        if (cores >= 4) return "4 Cores";
        return $"{cores} Cores";
    }

    private static string NormalizeResolution(string res)
    {
        var clean = res.Replace(" ", "").ToLowerInvariant();
        if (clean.Contains("3840x2160") || clean.Contains("4k")) return "3840 x 2160 (4K UHD)";
        if (clean.Contains("2560x1440") || clean.Contains("1440p") || clean.Contains("2k")) return "2560 x 1440 (1440p 2K)";
        if (clean.Contains("1920x1080") || clean.Contains("1080p")) return "1920 x 1080 (1080p FHD)";
        if (clean.Contains("2560x1080")) return "2560 x 1080 (Ultrawide 21:9)";
        if (clean.Contains("1366x768")) return "1366 x 768 (Laptop)";
        if (clean.Contains("1280x720") || clean.Contains("720p")) return "1280 x 720 (720p / Steam Deck)";
        return res;
    }

    private static string NormalizePreset(string preset)
    {
        var lower = preset.ToLowerInvariant();
        if (lower.Contains("high") || lower == "h") return "HIGH (Max Details / 60 FPS)";
        if (lower.Contains("mid") || lower == "m") return "MEDIUM (Standard / 60 FPS)";
        if (lower.Contains("low") || lower == "l") return "LOW (Performance)";
        return preset;
    }

    private static string NormalizeController(string c)
    {
        var lower = c.ToLowerInvariant();
        if (lower.Contains("xbox") || lower.Contains("xinput")) return "Xbox Controller (XInput)";
        if (lower.Contains("sony") || lower.Contains("playstation") || lower.Contains("dualshock") || lower.Contains("dualsense") || lower.Contains("ps4") || lower.Contains("ps5")) return "PlayStation (DualShock 4 / DualSense)";
        if (lower.Contains("key") || lower.Contains("board")) return "Keyboard";
        return "Generic DirectInput USB Gamepad";
    }

    private void SaveToFile()
    {
        try
        {
            var data = new StoredSurveyData
            {
                SampleCount = _sampleCount,
                LastUpdated = _lastUpdated,
                CategoryCounts = _categoryCounts
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_storagePath, json);
        }
        catch { }
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_storagePath)) return;
        try
        {
            var json = File.ReadAllText(_storagePath);
            var data = JsonSerializer.Deserialize<StoredSurveyData>(json);
            if (data is not null)
            {
                _sampleCount = data.SampleCount;
                _lastUpdated = data.LastUpdated;
                foreach (var (cat, items) in data.CategoryCounts)
                {
                    if (_categoryCounts.TryGetValue(cat, out var target))
                    {
                        foreach (var (k, v) in items)
                            target[k] = v;
                    }
                    else
                    {
                        _categoryCounts[cat] = new Dictionary<string, int>(items, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
        }
        catch { }
    }

    private sealed class StoredSurveyData
    {
        public int SampleCount { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public Dictionary<string, Dictionary<string, int>> CategoryCounts { get; set; } = new();
    }
}
