using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DiskCleanManager.Models;

namespace DiskCleanManager.Services;

/// <summary>
/// disk_cleanup_summary.md のテーブル行をパースして CacheFolder リストを生成する。
/// パス内のユーザー名は %USERPROFILE% 展開で置換し、ユーザー名非依存にする。
/// </summary>
public static class MarkdownParser
{
    // | No | 判定 | 容量 | パス | 備考 | の行にマッチ
    private static readonly Regex TableRowRegex = new(
        @"^\|\s*(\d+)\s*\|\s*(.+?)\s*\|\s*\*\*(.+?)\*\*\s*\|\s*`(.+?)`\s*\|\s*(.+?)\s*\|",
        RegexOptions.Compiled);

    // コマンド実行型の定義（No → コマンド文字列 & フォルダ名）
    private static readonly Dictionary<int, (string Command, string FolderName)> CommandEntries = new()
    {
        [1]  = ("Remove-Item -Recurse -Force \"$env:USERPROFILE\\.gradle\\caches\"", "Gradle_caches"),
        [3]  = ("pip cache purge", "pip_cache"),
        [9]  = ("uv cache clean", "uv_cache"),
        [10] = ("npm cache clean --force", "npm-cache"),
        [20] = ("dart pub cache clean", "Dart_PubCache"),
    };

    public static List<CacheFolder> Parse(string markdownText)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var result = new List<CacheFolder>();

        foreach (var line in markdownText.Split('\n'))
        {
            var m = TableRowRegex.Match(line.Trim());
            if (!m.Success) continue;

            if (!int.TryParse(m.Groups[1].Value.Trim(), out int no)) continue;

            var safetyRaw = m.Groups[2].Value.Trim();
            var sizeText  = m.Groups[3].Value.Trim();
            var rawPath   = m.Groups[4].Value.Trim();
            var note      = m.Groups[5].Value.Trim();

            var safety = safetyRaw switch
            {
                var s when s.Contains("⛔") => SafetyLevel.Forbidden,
                var s when s.Contains("🟡") => SafetyLevel.Caution,
                _                           => SafetyLevel.Safe,
            };

            // ユーザー名を %USERPROFILE% 展開で置換
            var expandedPath = ExpandPath(rawPath, userProfile);

            ActionType action;
            string? command = null;
            string folderName;

            if (safety == SafetyLevel.Forbidden)
            {
                action = ActionType.Forbidden;
                folderName = $"No{no}";
            }
            else if (CommandEntries.TryGetValue(no, out var cmdEntry))
            {
                action = ActionType.Command;
                command = cmdEntry.Command;
                folderName = cmdEntry.FolderName;
            }
            else
            {
                action = ActionType.Symlink;
                folderName = MakeFolderName(expandedPath, no);
            }

            result.Add(new CacheFolder
            {
                No              = no,
                Safety          = safety,
                OriginalSizeText = sizeText,
                RawPath         = rawPath,
                Path            = expandedPath,
                Note            = note,
                Action          = action,
                CleanCommand    = command,
                AppFolderName   = folderName,
            });
        }

        return result;
    }

    /// <summary>
    /// パス内のユーザー名を実際の %USERPROFILE% で置換。
    /// 例: C:\Users\ryuya\AppData → C:\Users\{actualUser}\AppData
    /// </summary>
    private static string ExpandPath(string rawPath, string userProfile)
    {
        // まず環境変数展開（%USERPROFILE% など既に入っている場合）
        var expanded = Environment.ExpandEnvironmentVariables(rawPath);

        // C:\Users\<任意のユーザー名>\ を実際のユーザープロファイルに置換
        expanded = Regex.Replace(
            expanded,
            @"C:\\Users\\[^\\]+\\",
            userProfile.TrimEnd('\\') + "\\",
            RegexOptions.IgnoreCase);

        return expanded.Replace('/', '\\');
    }

    /// <summary>
    /// シンボリックリンク先に使うフォルダ名をパスから生成する。
    /// 重複を避けるため No を接頭辞に付ける。
    /// </summary>
    private static string MakeFolderName(string path, int no)
    {
        // パスの末尾セグメント（末尾の \ を除去してから取得）
        var trimmed = path.TrimEnd('\\', '/');
        var lastSegment = System.IO.Path.GetFileName(trimmed);
        if (string.IsNullOrEmpty(lastSegment)) lastSegment = $"Cache{no}";

        // 使えない文字を除去
        var clean = Regex.Replace(lastSegment, @"[^\w\-]", "_");
        return $"{no:D2}_{clean}";
    }
}
