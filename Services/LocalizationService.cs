using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace DiskCleanManager.Services;

public record LanguageOption(string Code, string DisplayName);

public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLanguageCode = "ja";
    private readonly List<LanguageOption> _availableLanguages = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<LanguageOption> AvailableLanguages => _availableLanguages;

    public string CurrentLanguageCode
    {
        get => _currentLanguageCode;
        private set
        {
            if (_currentLanguageCode != value)
            {
                _currentLanguageCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null)); // notify all
            }
        }
    }

    public string this[string key] => Get(key);

    public string Get(string key, params object[] args)
    {
        if (_strings.TryGetValue(key, out var val))
        {
            if (args != null && args.Length > 0)
            {
                try { return string.Format(val, args); } catch { }
            }
            return val;
        }
        return key;
    }

    private LocalizationService()
    {
        ScanLanguages();
    }

    public void ScanLanguages()
    {
        _availableLanguages.Clear();

        var langDir = FindLanguagesDirectory();
        if (Directory.Exists(langDir))
        {
            foreach (var file in Directory.GetFiles(langDir, "*.json"))
            {
                var code = Path.GetFileNameWithoutExtension(file);
                var displayName = code;
                try
                {
                    var text = File.ReadAllText(file);
                    using var doc = JsonDocument.Parse(text);
                    if (doc.RootElement.TryGetProperty("LanguageName", out var nameProp))
                    {
                        displayName = nameProp.GetString() ?? code;
                    }
                }
                catch { }

                _availableLanguages.Add(new LanguageOption(code, displayName));
            }
        }

        if (_availableLanguages.Count == 0)
        {
            _availableLanguages.Add(new LanguageOption("ja", "日本語"));
            _availableLanguages.Add(new LanguageOption("en", "English"));
        }
    }

    public void SetLanguage(string langSetting)
    {
        string targetCode = langSetting;

        if (string.Equals(langSetting, "System", StringComparison.OrdinalIgnoreCase))
        {
            var systemIso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            targetCode = _availableLanguages.Exists(l => l.Code.Equals(systemIso, StringComparison.OrdinalIgnoreCase))
                ? systemIso
                : "en";
        }

        LoadLanguageFile(targetCode);
        CurrentLanguageCode = targetCode;
    }

    private void LoadLanguageFile(string code)
    {
        _strings.Clear();
        var langDir = FindLanguagesDirectory();
        var filePath = Path.Combine(langDir, $"{code}.json");

        if (!File.Exists(filePath))
        {
            // fallback to ja or en
            filePath = Path.Combine(langDir, "ja.json");
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(langDir, "en.json");
            }
        }

        if (File.Exists(filePath))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        _strings[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch { }
        }
    }

    private static string FindLanguagesDirectory()
    {
        var baseDir = AppContext.BaseDirectory;
        var direct = Path.Combine(baseDir, "Languages");
        if (Directory.Exists(direct)) return direct;

        var parent = Directory.GetParent(baseDir)?.FullName;
        if (parent != null)
        {
            var parentLang = Path.Combine(parent, "Languages");
            if (Directory.Exists(parentLang)) return parentLang;
        }

        var cwdLang = Path.Combine(Directory.GetCurrentDirectory(), "Languages");
        if (Directory.Exists(cwdLang)) return cwdLang;

        return direct;
    }
}
