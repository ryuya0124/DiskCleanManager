using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DiskCleanManager.Services;

public enum SymlinkResultKind { Success, AlreadyLinked, Error }

public record SymlinkResult(SymlinkResultKind Kind, string Message);

public static class SymlinkService
{
    /// <summary>
    /// path が ReparsePoint（シンボリックリンク）かどうかを FileAttributes で実際に検知する。
    /// </summary>
    public static bool IsSymlink(string path)
    {
        try
        {
            if (!Directory.Exists(path) && !File.Exists(path)) return false;
            var attrs = File.GetAttributes(path);
            return (attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }
        catch { return false; }
    }

    /// <summary>
    /// source を R:\{subFolderName}\ へ移動してシンボリックリンク（junction）を作成する。
    ///
    /// ルール:
    ///   1. Rドライブが存在しない → エラー
    ///   2. R:\{subFolderName} が存在しない → エラー（自動作成しない）
    ///   3. source が既にシンボリックリンク → AlreadyLinked
    ///   4. source が存在しない → エラー
    ///   5. UAC で昇格した PowerShell スクリプトを実行して移動 + junction 作成
    /// </summary>
    public static async Task<SymlinkResult> CreateSymlinkAsync(string sourcePath, string subFolderName)
    {
        // 1. Rドライブ存在確認
        if (!Directory.Exists("R:\\"))
            return new SymlinkResult(SymlinkResultKind.Error,
                "R: ドライブが見つかりません。RAMDiskがマウントされているか確認してください。");

        // 2. R:\{subFolderName} 存在確認（自動作成しない）
        var targetBase = Path.Combine("R:\\", subFolderName);
        if (!Directory.Exists(targetBase))
            return new SymlinkResult(SymlinkResultKind.Error,
                $"フォルダ「{targetBase}」が存在しません。先にRAMDisk上に作成してください。");

        // 3. 既にシンボリックリンク
        if (IsSymlink(sourcePath))
            return new SymlinkResult(SymlinkResultKind.AlreadyLinked,
                $"「{sourcePath}」は既にシンボリックリンクです。");

        // 4. ソース存在確認
        if (!Directory.Exists(sourcePath))
            return new SymlinkResult(SymlinkResultKind.Error,
                $"ソースフォルダが見つかりません: {sourcePath}");

        // 5. 昇格した PowerShell で移動 + junction 作成
        //    ・ファイルを R:\{subFolderName}\ へコピー
        //    ・元フォルダを削除
        //    ・mklink /j でジャンクション（ディレクトリシンボリックリンク）を作成
        //    → mklink は cmd.exe のビルトインなので cmd /c で呼ぶ
        var script = $@"
Copy-Item -Path '{EscapePs(sourcePath)}' -Destination '{EscapePs(targetBase)}' -Recurse -Force
Remove-Item -Path '{EscapePs(sourcePath)}' -Recurse -Force
cmd /c mklink /j '{EscapePs(sourcePath)}' '{EscapePs(targetBase)}'
".Trim();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName        = "powershell.exe",
                Arguments       = $"-NoProfile -NonInteractive -Command \"{EscapeArg(script)}\"",
                Verb            = "runas",          // UAC ダイアログを出して昇格
                UseShellExecute = true,             // runas には ShellExecute が必要
                CreateNoWindow  = false,
            };

            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("プロセスを起動できませんでした。");

            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
                return new SymlinkResult(SymlinkResultKind.Error,
                    $"スクリプトが失敗しました（終了コード: {proc.ExitCode}）。\nRドライブの空き容量を確認してください。");

            // 作成確認
            if (IsSymlink(sourcePath))
                return new SymlinkResult(SymlinkResultKind.Success,
                    $"✓ ジャンクションを作成しました\n{sourcePath}\n  → {targetBase}");
            else
                return new SymlinkResult(SymlinkResultKind.Error,
                    "コマンドは成功しましたがジャンクションが確認できませんでした。");
        }
        catch (Exception ex)
        {
            return new SymlinkResult(SymlinkResultKind.Error,
                $"シンボリックリンク作成に失敗しました: {ex.Message}");
        }
    }

    private static string EscapePs(string s)  => s.Replace("'", "''");
    private static string EscapeArg(string s) => s.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "; ");
}
