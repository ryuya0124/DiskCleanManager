using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DiskCleanManager.Data;
using DiskCleanManager.Models;
using DiskCleanManager.Services;

namespace DiskCleanManager.ViewModels;

public partial class CacheFolderViewModel : ObservableObject
{
    private readonly CacheFolder _model;
    public LocalizationService Loc => LocalizationService.Instance;

    // --- 静的プロパティ ---
    public int No               => _model.No;
    public string Path          => _model.Path;
    public string Note          => _model.Note;
    public SafetyLevel Safety   => _model.Safety;
    public ActionType Action    => _model.Action;
    public string? CleanCommand => _model.CleanCommand;
    public bool PathExists      => _model.PathExists;

    // --- 編集可能: Rドライブのフォルダ名 ---
    [ObservableProperty] public partial string AppFolderName { get; set; }

    // --- Rドライブターゲットパス（AppFolderName 変更で更新） ---
    public string RTargetPath => System.IO.Path.Combine("R:\\", AppFolderName);
    partial void OnAppFolderNameChanged(string value) => OnPropertyChanged(nameof(RTargetPath));

    // --- 安全レベル表示 ---
    public string SafetyEmoji => _model.Safety switch
    {
        SafetyLevel.Safe      => "🟢",
        SafetyLevel.Caution   => "🟡",
        SafetyLevel.Forbidden => "⛔",
        _                     => "？"
    };
    public string SafetyText => _model.Safety switch
    {
        SafetyLevel.Safe      => Loc["Safe"],
        SafetyLevel.Caution   => Loc["Caution"],
        SafetyLevel.Forbidden => Loc["Forbidden"],
        _                     => ""
    };
    public SolidColorBrush SafetyColor => _model.Safety switch
    {
        SafetyLevel.Safe      => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 197, 94)),
        SafetyLevel.Caution   => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 234, 179, 8)),
        SafetyLevel.Forbidden => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)),
        _                     => new SolidColorBrush(Colors.Gray)
    };

    // --- ActionType ベースの Visibility ---
    public Visibility CmdPanelVisible      => Action == ActionType.Command   ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SymPanelVisible      => Action == ActionType.Symlink   ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ForbPanelVisible     => Action == ActionType.Forbidden ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ActionablePanelVisible =>
        Action != ActionType.Forbidden ? Visibility.Visible : Visibility.Collapsed;

    // --- 計算 Visibility ---
    public Visibility CommandOutputVisibility => CommandOutputVisible ? Visibility.Visible : Visibility.Collapsed;
    public Visibility StatusMessageVisibility => string.IsNullOrEmpty(StatusMessage) ? Visibility.Collapsed : Visibility.Visible;

    // --- ボタン有効フラグ ---
    public bool ScanButtonEnabled   => !IsScanning;
    public bool CmdButtonEnabled    => !IsRunningCommand;
    public bool DeleteButtonEnabled => !IsDeleting;

    // --- 可変プロパティ ---
    [ObservableProperty] public partial string ScannedSize          { get; set; }
    [ObservableProperty] public partial bool   IsScanning           { get; set; }
    [ObservableProperty] public partial bool   IsRunningCommand     { get; set; }
    [ObservableProperty] public partial bool   IsDeleting           { get; set; }
    [ObservableProperty] public partial string CommandOutput        { get; set; }
    [ObservableProperty] public partial bool   CommandOutputVisible { get; set; }
    [ObservableProperty] public partial string StatusMessage        { get; set; }

    private bool _isLinked;
    public bool IsLinked
    {
        get => _isLinked;
        set
        {
            if (SetProperty(ref _isLinked, value))
            {
                OnPropertyChanged(nameof(LinkStatusText));
                OnPropertyChanged(nameof(LinkStatusColor));
                OnPropertyChanged(nameof(LinkButtonEnabled));
            }
        }
    }

    public string LinkStatusText => IsLinked ? Loc["Linked"] : Loc["Unlinked"];
    public SolidColorBrush LinkStatusColor => IsLinked
        ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 102, 241))
        : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 114, 128));
    public bool LinkButtonEnabled => !IsLinked;

    // 削除確認フラグ（2クリック方式）
    private bool _deleteConfirmPending;

    public CacheFolderViewModel(CacheFolder model)
    {
        _model               = model;
        AppFolderName        = model.AppFolderName;
        ScannedSize          = "";
        IsScanning           = false;
        IsRunningCommand     = false;
        IsDeleting           = false;
        CommandOutput        = "";
        CommandOutputVisible = false;
        StatusMessage        = "";
        _isLinked            = false;
        RefreshLinkStatus();

        Loc.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SafetyText));
            OnPropertyChanged(nameof(LinkStatusText));
            OnPropertyChanged(nameof(Loc));
        };
    }

    public void RefreshLinkStatus() => IsLinked = SymlinkService.IsSymlink(_model.Path);

    partial void OnIsScanningChanged(bool value)           => OnPropertyChanged(nameof(ScanButtonEnabled));
    partial void OnIsRunningCommandChanged(bool value)     => OnPropertyChanged(nameof(CmdButtonEnabled));
    partial void OnIsDeletingChanged(bool value)           => OnPropertyChanged(nameof(DeleteButtonEnabled));
    partial void OnCommandOutputVisibleChanged(bool value) => OnPropertyChanged(nameof(CommandOutputVisibility));
    partial void OnStatusMessageChanged(string value)      => OnPropertyChanged(nameof(StatusMessageVisibility));

    // ===== スキャン =====
    [RelayCommand]
    public async Task ScanAsync()
    {
        if (!PathExists || IsScanning) return;
        IsScanning = true;
        ScannedSize = Loc["Scanning"];
        try
        {
            var bytes = await FolderScanService.ScanAsync(_model.Path);
            ScannedSize = bytes == 0 && IsLinked ? Loc["ToRamDisk"] : FolderScanService.FormatBytes(bytes);
        }
        catch { ScannedSize = Loc["Error"]; }
        finally { IsScanning = false; }
    }

    // ===== エクスプローラーで開く（ソース） =====
    [RelayCommand]
    public void OpenInExplorer()
    {
        var p = PathExists ? _model.Path : System.IO.Path.GetDirectoryName(_model.Path) ?? _model.Path;
        try { Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{p}\"", UseShellExecute = true }); }
        catch { }
    }

    // ===== エクスプローラーで開く（Rドライブ） =====
    [RelayCommand]
    public void OpenRInExplorer()
    {
        var target = RTargetPath;
        if (!Directory.Exists(target)) { StatusMessage = $"❌ {target} (Not Found)"; return; }
        try { Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{target}\"", UseShellExecute = true }); }
        catch { }
    }

    // ===== コマンド実行 =====
    [RelayCommand]
    public async Task RunCleanCommandAsync()
    {
        if (CleanCommand == null || IsRunningCommand) return;
        IsRunningCommand = true;
        CommandOutput = Loc["Scanning"];
        CommandOutputVisible = true;
        try
        {
            var result = await CommandRunnerService.RunAsync(CleanCommand);
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(result.Output)) sb.AppendLine(result.Output);
            if (!string.IsNullOrEmpty(result.Error))  sb.AppendLine("[stderr] " + result.Error);
            sb.Append($"[Exit Code: {result.ExitCode}]");
            CommandOutput = sb.ToString().Trim();
        }
        catch (Exception ex) { CommandOutput = $"Error: {ex.Message}"; }
        finally
        {
            IsRunningCommand = false;
            await ScanAsync();
        }
    }

    // ===== キャッシュ削除（2クリック確認） =====
    [RelayCommand]
    public async Task DeleteCacheAsync()
    {
        if (IsDeleting) return;

        // 1クリック目：確認メッセージ
        if (!_deleteConfirmPending)
        {
            _deleteConfirmPending = true;
            StatusMessage = $"{Loc["DeleteConfirm"]} ({System.IO.Path.GetFileName(_model.Path)})";
            // 5秒後に確認フラグをリセット
            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                _deleteConfirmPending = false;
                if (StatusMessage.StartsWith("⚠️"))
                    StatusMessage = "";
            }, TaskScheduler.Current);
            return;
        }

        // 2クリック目：実行
        _deleteConfirmPending = false;
        IsDeleting = true;
        StatusMessage = Loc["Scanning"];

        try
        {
            var script = $"Remove-Item -Path '{_model.Path.Replace("'", "''")}' -Recurse -Force";
            var psi = new ProcessStartInfo
            {
                FileName        = "powershell.exe",
                Arguments       = $"-NoProfile -NonInteractive -Command \"{script.Replace("\"", "\\\"")}\"",
                Verb            = "runas",
                UseShellExecute = true,
                CreateNoWindow  = false,
            };
            using var proc = Process.Start(psi)!;
            await proc.WaitForExitAsync();

            if (proc.ExitCode == 0)
            {
                StatusMessage = Loc["Deleted"];
                ScannedSize = "";
                RefreshLinkStatus();
            }
            else
            {
                StatusMessage = $"{Loc["DeleteFailed"]} (Code: {proc.ExitCode})";
            }
        }
        catch (Exception ex) { StatusMessage = $"❌ {ex.Message}"; }
        finally { IsDeleting = false; }
    }

    // ===== シンボリックリンク作成 =====
    [RelayCommand]
    public async Task CreateSymlinkAsync()
    {
        if (IsLinked) return;
        StatusMessage = "UAC...";
        var result = await SymlinkService.CreateSymlinkAsync(_model.Path, AppFolderName);
        StatusMessage = result.Message;
        RefreshLinkStatus();
        if (IsLinked) ScannedSize = Loc["ToRamDisk"];
    }
}

public partial class MainViewModel : ObservableObject
{
    public LocalizationService Loc => LocalizationService.Instance;

    [ObservableProperty] public partial ObservableCollection<CacheFolderViewModel> Items        { get; set; }
    [ObservableProperty] public partial bool IsScanningAll { get; set; }
    [ObservableProperty] public partial int  ScanProgress  { get; set; }
    [ObservableProperty] public partial int  ScanTotal     { get; set; }

    private CancellationTokenSource? _scanCts;

    public MainViewModel()
    {
        Items         = [];
        IsScanningAll = false;
        ScanProgress  = 0;
        ScanTotal     = 0;

        foreach (var f in BuiltinCacheData.GetAll())
            Items.Add(new CacheFolderViewModel(f));

        Loc.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Loc));
        };
    }

    public async Task ScanAllAsync()
    {
        if (IsScanningAll) { _scanCts?.Cancel(); return; }

        _scanCts = new CancellationTokenSource();
        IsScanningAll = true;
        ScanProgress = 0;
        ScanTotal = Items.Count(x => x.PathExists && x.Action != ActionType.Forbidden);

        try
        {
            foreach (var item in Items)
            {
                if (_scanCts.Token.IsCancellationRequested) break;
                if (!item.PathExists || item.Action == ActionType.Forbidden) continue;
                await item.ScanAsync();
                ScanProgress++;
            }
        }
        finally { IsScanningAll = false; }
    }

    public void RefreshAllLinkStatus()
    {
        foreach (var item in Items)
            item.RefreshLinkStatus();
    }
}
