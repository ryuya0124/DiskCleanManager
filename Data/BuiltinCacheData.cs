using System;
using System.Collections.Generic;
using DiskCleanManager.Models;

namespace DiskCleanManager.Data;

/// <summary>
/// disk_cleanup_summary.md の内容をハードコードしたデータ。
/// MDファイルがなくても使えるようにアプリに埋め込んでいる。
/// %USERPROFILE% はランタイムで展開する。
/// </summary>
public static class BuiltinCacheData
{
    private static readonly string Up =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    // %USERPROFILE% を展開したパスに変換するヘルパー
    private static string U(string rel) => System.IO.Path.Combine(Up, rel);

    public static List<CacheFolder> GetAll()
    {
        return
        [
            new() { No=1,  Safety=SafetyLevel.Safe,
                Path=U(@".gradle\caches"),
                Note="Gradleビルドキャッシュ。削除しても次回ビルド時に再ダウンロードされます。",
                Action=ActionType.Command,
                CleanCommand=@"Remove-Item -Recurse -Force ""$env:USERPROFILE\.gradle\caches""",
                AppFolderName="Gradle_caches" },

            new() { No=2,  Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Maxon\Cinebench2026_win_x86_64_72BF1D96\Redshift\Cache"),
                Note="Cinebench テクスチャキャッシュ。削除可能。",
                Action=ActionType.Symlink, AppFolderName="02_Cache" },

            new() { No=3,  Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\pip\cache"),
                Note="pipダウンロードキャッシュ。pip cache purge で安全に削除可能。",
                Action=ActionType.Command,
                CleanCommand=@"if (Get-Command pip -ErrorAction SilentlyContinue) { pip cache purge } else { Remove-Item -Path (Join-Path $env:LOCALAPPDATA 'pip\cache\*') -Recurse -Force -ErrorAction SilentlyContinue; Write-Host 'pip キャッシュを削除しました。' }",
                AppFolderName="pip_cache" },

            new() { No=4,  Safety=SafetyLevel.Caution,
                Path=U(@"AppData\Local\NVIDIA\DXCache"),
                Note="DirectXシェーダーキャッシュ。消しても壊れませんがゲーム初回起動時にシェーダー再構築でカクつきます。",
                Action=ActionType.Symlink, AppFolderName="04_DXCache" },

            new() { No=5,  Safety=SafetyLevel.Forbidden,
                Path=U(@"flutter\bin\cache"),
                Note="Flutter SDK本体のエンジン・Dart SDKバイナリが含まれます。消すとflutterコマンドが壊れます。",
                Action=ActionType.Forbidden, AppFolderName="05_flutter_cache" },

            new() { No=6,  Safety=SafetyLevel.Caution,
                Path=@"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\Emscripten\emscripten\cache",
                Note="Unity WebGLビルド用キャッシュ。消すと次回WebGLビルドに非常に長い時間がかかります。",
                Action=ActionType.Symlink, AppFolderName="06_Emscripten_cache" },

            new() { No=7,  Safety=SafetyLevel.Safe,
                Path=@"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache",
                Note="Windows Update配信最適化キャッシュ。ディスククリーンアップ等で削除可能。",
                Action=ActionType.Symlink, AppFolderName="07_DeliveryOptimization" },

            new() { No=8,  Safety=SafetyLevel.Forbidden,
                Path=@"C:\ProgramData\Package Cache",
                Note="インストーラーの修復・アンインストール管理データ。消すとアプリの更新やアンインストールができなくなります。",
                Action=ActionType.Forbidden, AppFolderName="08_PackageCache" },

            new() { No=9,  Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\uv\cache"),
                Note="uvパッケージキャッシュ。uv cache clean で安全に削除可能。",
                Action=ActionType.Command,
                CleanCommand=@"if (Get-Command uv -ErrorAction SilentlyContinue) { uv cache clean } elseif (Test-Path (Join-Path $env:USERPROFILE 'AI_lllustration\Comfy Desktop\resources\bootstrap-python\uv.exe')) { & (Join-Path $env:USERPROFILE 'AI_lllustration\Comfy Desktop\resources\bootstrap-python\uv.exe') cache clean } else { Remove-Item -Path (Join-Path $env:LOCALAPPDATA 'uv\cache\*') -Recurse -Force -ErrorAction SilentlyContinue; Write-Host 'uv キャッシュを削除しました。' }",
                AppFolderName="uv_cache" },

            new() { No=10, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\npm-cache"),
                Note="npmキャッシュ。npm cache clean --force で安全に削除可能。",
                Action=ActionType.Command,
                CleanCommand=@"if (Get-Command npm -ErrorAction SilentlyContinue) { npm cache clean --force } else { Remove-Item -Path (Join-Path $env:LOCALAPPDATA 'npm-cache\*') -Recurse -Force -ErrorAction SilentlyContinue; Write-Host 'npm キャッシュを削除しました。' }",
                AppFolderName="npm-cache" },

            new() { No=11, Safety=SafetyLevel.Safe,
                Path=@"C:\ProgramData\LGHUB\cache",
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="11_LGHUB_cache" },

            new() { No=12, Safety=SafetyLevel.Safe,
                Path=U(@".cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="12_dotcache" },

            new() { No=13, Safety=SafetyLevel.Forbidden,
                Path=U(@"AppData\Local\Package Cache"),
                Note="インストーラーの修復・アンインストール管理データ。消すとアプリの更新やアンインストールができなくなります。",
                Action=ActionType.Forbidden, AppFolderName="13_LocalPackageCache" },

            new() { No=14, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Comfy Desktop\Partitions\inst-1783520668700\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="14_ComfyDesktop_Cache" },

            new() { No=15, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Code\CachedExtensionVSIXs"),
                Note="VS Code 拡張機能のインストール残骸。削除可能。",
                Action=ActionType.Symlink, AppFolderName="15_VSCode_VSIX" },

            new() { No=16, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Riot Client\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="16_RiotClient_Cache" },

            new() { No=17, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Cognosphere\HYP\1_0\fedata\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="17_HYP_Cache" },

            new() { No=18, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\CrashDumps"),
                Note="アプリのクラッシュログ・ダンプファイル。削除可能。",
                Action=ActionType.Symlink, AppFolderName="18_CrashDumps" },

            new() { No=19, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\.minecraft\webcache2"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="19_minecraft_webcache2" },

            new() { No=20, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\Pub\Cache"),
                Note="Dart/Pubパッケージキャッシュ。dart pub cache clean で安全に削除可能。",
                Action=ActionType.Command,
                CleanCommand=@"if (Get-Command dart -ErrorAction SilentlyContinue) { dart pub cache clean } else { Remove-Item -Path (Join-Path $env:LOCALAPPDATA 'Pub\Cache\*') -Recurse -Force -ErrorAction SilentlyContinue; Write-Host 'Pub キャッシュを削除しました。' }",
                AppFolderName="Dart_PubCache" },

            new() { No=21, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\aviutl2-catalog\EBWebView\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="21_aviutl2_Cache" },

            new() { No=22, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\EpicGamesLauncher\Saved\webcache_4430"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="22_Epic_webcache" },

            new() { No=23, Safety=SafetyLevel.Safe,
                Path=@"C:\Program Files\Ruby40-x64\msys64\var\cache",
                Note="MSYS2 pacman キャッシュ。削除可能。",
                Action=ActionType.Symlink, AppFolderName="23_Ruby_msys2_cache" },

            new() { No=24, Safety=SafetyLevel.Safe,
                Path=@"C:\msys64\var\cache",
                Note="MSYS2 pacman キャッシュ。削除可能。",
                Action=ActionType.Symlink, AppFolderName="24_msys64_cache" },

            new() { No=25, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Code\CachedData"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="25_VSCode_CachedData" },

            new() { No=26, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\Steam\htmlcache\Default\Code Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="26_Steam_CodeCache" },

            new() { No=27, Safety=SafetyLevel.Caution,
                Path=U(@"AppData\LocalLow\NVIDIA\DXCache"),
                Note="DirectXシェーダーキャッシュ。消しても壊れませんがゲーム初回起動時にシェーダー再構築でカクつきます。",
                Action=ActionType.Symlink, AppFolderName="27_NVIDIA_DXCache_Low" },

            new() { No=28, Safety=SafetyLevel.Safe,
                Path=U(@".gemini\antigravity-browser-profile\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="28_Antigravity_Cache" },

            new() { No=29, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Comfy Desktop\Partitions\inst-1786428874289\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="29_ComfyDesktop2_Cache" },

            new() { No=30, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\KRLauncher\G153\C50004\KRWebViewUserData\EBWebView\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="30_KRLauncher_Cache" },

            new() { No=31, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Antigravity IDE\CachedData"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="31_Antigravity_CachedData" },

            new() { No=32, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\discord\Shared Dictionary\cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="32_Discord_cache" },

            new() { No=33, Safety=SafetyLevel.Safe,
                Path=@"C:\Windows\SystemApps\MicrosoftWindows.Client.CBS_cw5n1h2txyewy\Cortana.UI\cache",
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="33_Cortana_cache" },

            new() { No=34, Safety=SafetyLevel.Forbidden,
                Path=@"C:\Windows\WinSxS\amd64_userexperience-desktop_31bf3856ad364e35_10.0.26100.8972_none_c7e89dbd1a7347a5\CBS\Cortana.UI\cache",
                Note="Windowsコンポーネントストアのシステムファイルです。手動削除不可。",
                Action=ActionType.Forbidden, AppFolderName="34_WinSxS" },

            new() { No=35, Safety=SafetyLevel.Safe,
                Path=U(@".gemini\antigravity-browser-profile\Default\Code Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="35_Antigravity_CodeCache" },

            new() { No=36, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\Unity\Caches"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="36_Unity_Caches" },

            new() { No=37, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\UnityHub\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="37_UnityHub_Cache" },

            new() { No=38, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="38_Claude_Cache" },

            new() { No=39, Safety=SafetyLevel.Safe,
                Path=@"C:\Program Files\HoYoPlay\games\Genshin Impact game\GenshinImpact_Data\webCaches",
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="39_Genshin_webCaches" },

            new() { No=40, Safety=SafetyLevel.Safe,
                Path=U(@"Documents\kurashiTEPCOApp\APITest\chrome-data\Default\Code Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="40_TEPCO_CodeCache" },

            new() { No=41, Safety=SafetyLevel.Forbidden,
                Path=@"C:\Windows\WinSxS\amd64_userexperience-desktop_31bf3856ad364e35_10.0.26100.8655_none_c806c8951a5d0e07\CBS\Cortana.UI\cache",
                Note="Windowsコンポーネントストアのシステムファイルです。手動削除不可。",
                Action=ActionType.Forbidden, AppFolderName="41_WinSxS" },

            new() { No=42, Safety=SafetyLevel.Forbidden,
                Path=@"C:\Windows\WinSxS\amd64_userexperience-desktop_31bf3856ad364e35_10.0.26100.8737_none_c7f9ca851a672a8c\CBS\Cortana.UI\cache",
                Note="Windowsコンポーネントストアのシステムファイルです。手動削除不可。",
                Action=ActionType.Forbidden, AppFolderName="42_WinSxS" },

            new() { No=43, Safety=SafetyLevel.Forbidden,
                Path=@"C:\Windows\WinSxS\amd64_userexperience-desktop_31bf3856ad364e35_10.0.26100.8328_none_c823f3231a47bb12\CBS\Cortana.UI\cache",
                Note="Windowsコンポーネントストアのシステムファイルです。手動削除不可。",
                Action=ActionType.Forbidden, AppFolderName="43_WinSxS" },

            new() { No=44, Safety=SafetyLevel.Forbidden,
                Path=@"C:\Windows\WinSxS\amd64_userexperience-desktop_31bf3856ad364e35_10.0.26100.6725_none_c7f8f3ff1a67e3a5\CBS\Cortana.UI\cache",
                Note="Windowsコンポーネントストアのシステムファイルです。手動削除不可。",
                Action=ActionType.Forbidden, AppFolderName="44_WinSxS" },

            new() { No=45, Safety=SafetyLevel.Forbidden,
                Path=@"C:\Windows\WinSxS\amd64_userexperience-desktop_31bf3856ad364e35_10.0.26100.1591_none_c815e77f1a5104dd\CBS\Cortana.UI\cache",
                Note="Windowsコンポーネントストアのシステムファイルです。手動削除不可。",
                Action=ActionType.Forbidden, AppFolderName="45_WinSxS" },

            new() { No=46, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\iMazing\Caches"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="46_iMazing_Caches" },

            new() { No=47, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\LocalLow\BANDAI NAMCO Entertainment Inc_\gakumas\Vuplex.WebView\chromium-cache\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="47_gakumas_Cache" },

            new() { No=48, Safety=SafetyLevel.Safe,
                Path=U(@"Documents\kurashiTEPCOApp\APITest\chrome-data\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="48_TEPCO_Cache" },

            new() { No=49, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Comfy Desktop\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="49_ComfyDesktop_Cache2" },

            new() { No=50, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\NVIDIA Corporation\NVIDIA App\CefCache\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="50_NVIDIA_CefCache" },

            new() { No=51, Safety=SafetyLevel.Safe,
                Path=@"C:\Program Files\Neverness To Everness\NTEGlobal\UserData\cef_cache_0\Default\Cache",
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="51_NTE_CefCache" },

            new() { No=52, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\LINE\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="52_LINE_Cache" },

            new() { No=53, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\Code Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="53_Claude_CodeCache" },

            new() { No=54, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\Antigravity IDE\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="54_Antigravity_Cache2" },

            new() { No=55, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\dmmgameplayer5\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="55_DMM_Cache" },

            new() { No=56, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Local\Paradox Interactive\launcher-v2\chromium-data\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="56_Paradox_Cache" },

            new() { No=57, Safety=SafetyLevel.Safe,
                Path=@"C:\Program Files (x86)\Steam\steam\cached",
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="57_Steam_cached" },

            new() { No=58, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\G HUB\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="58_GHUB_Cache" },

            new() { No=59, Safety=SafetyLevel.Safe,
                Path=U(@"Documents\Rockstar Games\Social Club\Launcher\Renderer\Default\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="59_Rockstar_Cache" },

            new() { No=60, Safety=SafetyLevel.Safe,
                Path=U(@"AppData\Roaming\voicevox\Cache"),
                Note="不要な一時キャッシュ。削除しても次回必要時に自動再生成されます。",
                Action=ActionType.Symlink, AppFolderName="60_voicevox_Cache" },
        ];
    }
}
