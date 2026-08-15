using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DiskCleanManager.ViewModels;
using DiskCleanManager.Models;

namespace DiskCleanManager;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = false;

        try { AppWindow.Resize(new Windows.Graphics.SizeInt32(1140, 860)); } catch { }
        try { SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop(); } catch { }

        FolderList.ItemsSource = ViewModel.Items;

        // スキャン進捗の監視
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsScanningAll))
                ScanProgressBar.Visibility = ViewModel.IsScanningAll ? Visibility.Visible : Visibility.Collapsed;

            if (e.PropertyName is nameof(ViewModel.ScanProgress) or nameof(ViewModel.ScanTotal))
            {
                ScanProgressBar.Maximum = ViewModel.ScanTotal;
                ScanProgressBar.Value   = ViewModel.ScanProgress;
            }
        };

        UpdateStats();
    }

    private async void ScanAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsScanningAll)
        {
            await ViewModel.ScanAllAsync(); // キャンセル
            ScanAllButton.Content = "🔍 全件スキャン";
        }
        else
        {
            ScanAllButton.Content = "⏹ 中止";
            await ViewModel.ScanAllAsync();
            ScanAllButton.Content = "🔍 全件スキャン";
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

        StatsText.Text =
            $"全 {total} 件　　🟢 安全 {safe}件　🟡 注意 {caution}件　⛔ 削除不可 {forb}件" +
            $"　　🔗 リンク済み {linked}件　⚡ コマンド型 {cmdCnt}件";
    }
}
