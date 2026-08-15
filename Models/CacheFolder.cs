using System.IO;

namespace DiskCleanManager.Models;

public enum SafetyLevel
{
    Safe,       // 🟢 安全
    Caution,    // 🟡 注意
    Forbidden   // ⛔ 消しちゃダメ
}

public enum ActionType
{
    Command,    // CLIコマンドで削除
    Symlink,    // RAMディスクへシンボリックリンク
    Forbidden   // 操作不可
}

public class CacheFolder
{
    public int No { get; init; }
    public SafetyLevel Safety { get; init; }
    public string OriginalSizeText { get; init; } = "";
    public string RawPath { get; init; } = "";
    public string Note { get; init; } = "";

    /// <summary>%USERPROFILE% 等を展開した実パス</summary>
    public string Path { get; init; } = "";

    public ActionType Action { get; init; }

    /// <summary>コマンド実行型のみ。PowerShell に渡すコマンド文字列</summary>
    public string? CleanCommand { get; init; }

    /// <summary>シンボリックリンク先サブフォルダ名（例: "Gradle_caches"）</summary>
    public string AppFolderName { get; init; } = "";

    /// <summary>
    /// シンボリックリンクかどうかを FileAttributes で実際にチェック。
    /// パスが存在しない場合は false。
    /// </summary>
    public bool IsSymlink
    {
        get
        {
            try
            {
                if (!Directory.Exists(Path) && !File.Exists(Path)) return false;
                var attrs = File.GetAttributes(Path);
                return (attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch
            {
                return false;
            }
        }
    }

    public bool PathExists => Directory.Exists(Path) || File.Exists(Path);
}
