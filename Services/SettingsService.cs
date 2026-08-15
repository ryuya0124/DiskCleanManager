using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DiskCleanManager.Services;

public class AppSettings
{
    public string Theme { get; set; } = "System";      // "System", "Light", "Dark"
    public string Language { get; set; } = "System";   // "System", "ja", "en", etc.
    public Dictionary<string, string> CustomFolderNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public static class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DiskCleanManager");

    private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

    private static AppSettings? _current;

    public static AppSettings Current
    {
        get
        {
            if (_current == null)
            {
                Load();
            }
            return _current!;
        }
    }

    public static string? GetCustomFolderName(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (Current.CustomFolderNames != null && Current.CustomFolderNames.TryGetValue(path, out var customName))
        {
            return customName;
        }
        return null;
    }

    public static void SetCustomFolderName(string path, string customName)
    {
        if (string.IsNullOrEmpty(path)) return;
        Current.CustomFolderNames ??= new(StringComparer.OrdinalIgnoreCase);
        Current.CustomFolderNames[path] = customName;
        Save();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                _current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                _current.CustomFolderNames ??= new(StringComparer.OrdinalIgnoreCase);
                return;
            }
        }
        catch { }

        _current = new AppSettings();
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(_current ?? new AppSettings(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
        catch { }
    }
}
