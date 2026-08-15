using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiskCleanManager.Services;

public static class FolderScanService
{
    /// <summary>
    /// フォルダを再帰的にスキャンして総バイト数を返す。
    /// シンボリックリンクのフォルダ自体はスキャン対象から除外する。
    /// </summary>
    public static async Task<long> ScanAsync(
        string path,
        IProgress<long>? progress = null,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(path)) return 0L;

            // シンボリックリンクは実体がRAMディスク側にあるのでスキップ
            var attrs = File.GetAttributes(path);
            if ((attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                return 0L;

            long total = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        var fi = new FileInfo(file);
                        total += fi.Length;
                        progress?.Report(total);
                    }
                    catch { /* アクセス拒否等は無視 */ }
                }
            }
            catch (OperationCanceledException) { throw; }
            catch { /* ルートアクセス失敗等は無視 */ }

            return total;
        }, ct);
    }

    public static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
            >= 1024L * 1024        => $"{bytes / (1024.0 * 1024):F2} MB",
            >= 1024L               => $"{bytes / 1024.0:F1} KB",
            _                      => $"{bytes} B",
        };
    }
}
