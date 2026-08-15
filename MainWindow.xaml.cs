using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DiskCleanManager.Models;
using DiskCleanManager.Services;
using DiskCleanManager.ViewModels;
using WinRT.Interop;

namespace DiskCleanManager;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();
    private bool _isInitializing = true;

    // 最小ウィンドウサイズ定数 (DPI 100%基準)
    private const int MinWindowWidthPx  = 860;
    private const int MinWindowHeightPx = 540;

    // Subclass Proc (GC回収を防ぐためフィールド保持)
    private readonly SUBCLASSPROC? _subclassProc;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    private const int WM_GETMINMAXINFO = 0x0024;

    private delegate nint SUBCLASSPROC(nint hWnd, uint uMsg, nuint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(nint hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass, nuint dwRefData);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(nint hWnd, uint uMsg, nuint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hWnd);

    public MainWindow()
    {
        InitializeComponent();

        // 最小ウィンドウサイズ制限 (WM_GETMINMAXINFO サブクラス化)
        try
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            _subclassProc = new SUBCLASSPROC(WindowSubclassHandler);
            SetWindowSubclass(hwnd, _subclassProc, 1, 0);
        }
        catch { }

        // カスタムタイトルバー設定 (ヘッダー全体をドラッグ可能に)
        try
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(HeaderGrid);
            UpdateTitleBarInsets();
        }
        catch { }

        try
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1180, 880));
        }
        catch { }

        FolderList.ItemsSource = ViewModel.Items;

        // 進捗バインド
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsScanningAll))
            {
                ScanProgressBar.Visibility = ViewModel.IsScanningAll ? Visibility.Visible : Visibility.Collapsed;
                ScanAllButton.Content = ViewModel.IsScanningAll ? LocalizationService.Instance["StopScan"] : LocalizationService.Instance["ScanAll"];
            }

            if (e.PropertyName is nameof(ViewModel.ScanProgress) or nameof(ViewModel.ScanTotal))
            {
                ScanProgressBar.Maximum = ViewModel.ScanTotal;
                ScanProgressBar.Value   = ViewModel.ScanProgress;
            }
        };

        // 言語とテーマの初期化
        InitializeSettings();

        // 言語変更時のUI更新
        LocalizationService.Instance.PropertyChanged += (_, _) =>
        {
            UpdateLocalizedUi();
            UpdateStats();
        };

        UpdateLocalizedUi();
        UpdateStats();

        // 起動時に自動でバックグラウンドスキャンを開始
        _ = Task.Run(async () =>
        {
            await Task.Delay(600);
            DispatcherQueue.TryEnqueue(async () =>
            {
                await ViewModel.ScanAllAsync();
                UpdateStats();
            });
        });
    }

    private nint WindowSubclassHandler(nint hWnd, uint uMsg, nuint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (uMsg == WM_GETMINMAXINFO)
        {
            try
            {
                var dpi = GetDpiForWindow(hWnd);
                var scaling = dpi > 0 ? (float)dpi / 96.0f : 1.0f;

                var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                minMaxInfo.ptMinTrackSize.x = (int)(MinWindowWidthPx * scaling);
                minMaxInfo.ptMinTrackSize.y = (int)(MinWindowHeightPx * scaling);
                Marshal.StructureToPtr(minMaxInfo, lParam, false);
                return 0;
            }
            catch { }
        }
        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private void UpdateTitleBarInsets()
    {
        try
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = AppWindow.TitleBar;
                var rightInset = titleBar.RightInset;
                if (rightInset > 0)
                {
                    HeaderButtonPanel.Margin = new Thickness(0, 0, rightInset + 8, 0);
                }
            }
        }
        catch { }
    }

    private void InitializeSettings()
    {
        _isInitializing = true;

        // テーマの復元と適用
        var theme = SettingsService.Current.Theme;
        ApplyTheme(theme);

        foreach (ComboBoxItem item in ThemeComboBox.Items)
        {
            if ((string)item.Tag == theme)
            {
                ThemeComboBox.SelectedItem = item;
                break;
            }
        }
        if (ThemeComboBox.SelectedItem == null)
        {
            ThemeComboBox.SelectedIndex = 0;
        }

        // 言語一覧のスキャン & コンボボックス構築
        LocalizationService.Instance.ScanLanguages();
        LanguageComboBox.Items.Clear();

        var systemItem = new ComboBoxItem
        {
            Content = LocalizationService.Instance["LanguageSystem"],
            Tag = "System"
        };
        LanguageComboBox.Items.Add(systemItem);

        foreach (var lang in LocalizationService.Instance.AvailableLanguages)
        {
            LanguageComboBox.Items.Add(new ComboBoxItem
            {
                Content = $"{lang.DisplayName} ({lang.Code})",
                Tag = lang.Code
            });
        }

        var savedLang = SettingsService.Current.Language;
        LocalizationService.Instance.SetLanguage(savedLang);

        ComboBoxItem? targetItem = null;
        foreach (ComboBoxItem item in LanguageComboBox.Items)
        {
            if ((string)item.Tag == savedLang)
            {
                targetItem = item;
                break;
            }
        }
        LanguageComboBox.SelectedItem = targetItem ?? systemItem;

        _isInitializing = false;
    }

    private void ApplyTheme(string theme)
    {
        if (RootGrid == null) return;

        var elTheme = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark"  => ElementTheme.Dark,
            _       => ElementTheme.Default
        };

        RootGrid.RequestedTheme = elTheme;

        // タイトルバーボタンの見た目をテーマに合わせる
        try
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = AppWindow.TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                if (elTheme == ElementTheme.Dark || (elTheme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Dark))
                {
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonHoverForegroundColor = Colors.White;
                    titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(50, 255, 255, 255);
                }
                else
                {
                    titleBar.ButtonForegroundColor = Colors.Black;
                    titleBar.ButtonHoverForegroundColor = Colors.Black;
                    titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(30, 0, 0, 0);
                }
            }
        }
        catch { }
    }

    private void UpdateLocalizedUi()
    {
        var loc = LocalizationService.Instance;
        TitleText.Text          = loc["AppTitle"];
        SubtitleText.Text       = loc["AppSubtitle"];
        ScanAllButton.Content   = ViewModel.IsScanningAll ? loc["StopScan"] : loc["ScanAll"];
        RefreshLinksButton.Content = loc["RefreshLinks"];
        SettingsHeader.Text     = loc["Settings"];
        ThemeLabel.Text         = loc["Theme"];
        LanguageLabel.Text      = loc["Language"];

        if (LanguageComboBox.Items.Count > 0 && LanguageComboBox.Items[0] is ComboBoxItem firstItem)
        {
            firstItem.Content = loc["LanguageSystem"];
        }
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (ThemeComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is string themeTag)
        {
            SettingsService.Current.Theme = themeTag;
            SettingsService.Save();
            ApplyTheme(themeTag);
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (LanguageComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is string langTag)
        {
            SettingsService.Current.Language = langTag;
            SettingsService.Save();
            LocalizationService.Instance.SetLanguage(langTag);
            UpdateStats();
        }
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/ryuya0124/DiskCleanManager",
                UseShellExecute = true
            });
        }
        catch { }
    }

    private async void ScanAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsScanningAll)
        {
            await ViewModel.ScanAllAsync(); // キャンセル
        }
        else
        {
            await ViewModel.ScanAllAsync();
            UpdateStats();
        }
    }

    private void RefreshLinksButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshAllLinkStatus();
        UpdateStats();
    }

    private void UpdateStats()
    {
        var items   = ViewModel.Items;
        var total   = items.Count;
        var safe    = items.Count(x => x.Safety == SafetyLevel.Safe);
        var caution = items.Count(x => x.Safety == SafetyLevel.Caution);
        var forb    = items.Count(x => x.Safety == SafetyLevel.Forbidden);
        var linked  = items.Count(x => x.Action == ActionType.Symlink && x.IsLinked);
        var cmdCnt  = items.Count(x => x.Action == ActionType.Command);

        StatsText.Text = LocalizationService.Instance.Get("StatsSummary", total, safe, caution, forb, linked, cmdCnt);
    }
}
