# DiskCleanManager 🚀

Windows 向けモダン・高速ディスク容量最適化 ＆ キャッシュクリーンアップツール

**WinUI 3 (Windows App SDK)** と **.NET 9** で構築されたディスク管理ユーティリティです。

---

## 🌟 主な機能

- 🔍 **スマート・キャッシュスキャン**
  - ブラウザ、開発環境 (npm, pip, Docker, NuGet, Gradle, Cargo等)、ゲームランチャー (Steam, Epic, Unity)、メッセージングアプリ (Discord, Slack, LINE等) などの肥大化しやすいキャッシュフォルダを自動検出
- 🧹 **安全なキャッシュクリーンアップ**
  - 対象フォルダの一括スキャン、サイズ計算、選択削除
  - 専用コマンド実行によるキャッシュパージ (例: `npm cache clean --force`, `docker system prune`, `cleanmgr` など)
- 🔗 **シンボリックリンク (ジャンクション) オフロード**
  - Cドライブを圧迫する大容量フォルダを別ドライブ (Dドライブなど) へ移動し、透過的なジャンクションを作成して容量を確保
- 📄 **マークダウン定義ファイルのインポート**
  - 独自のクリーンアップ対象一覧が書かれた Markdown (`disk_cleanup_summary.md` 等) からターゲットフォルダをインポート
- 🌐 **多言語対応 (Multilingual)**
  - 日本語 (Japanese) / 英語 (English) 切り替え対応
- 🎨 **Modern Windows Fluent UI**
  - Mica エフェクトや Windows 11 のデザインガイドラインに準拠した UI

---

## 💻 動作環境

- **OS**: Windows 10 (バージョン 1809 / Build 17763 以降) または Windows 11
- **アーキテクチャ**: x64 (推奨), ARM64, x86
- **ランタイム**:
  - **自己完結版 (Self-Contained / Single File)**: 追加ランタイムのインストール不要（そのまま実行可能）
  - **フレームワーク依存版 (Framework-Dependent)**: [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) および [Windows App SDK Runtime](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads) が必要

---

## 🛠️ ビルド方法

プロジェクトルートに用意されているスクリプトまたは `dotnet` コマンドで簡単にビルド・発行できます。

### 1. スクリプトで簡単ビルド (推奨)

#### バッチファイルから実行 (メニュー選択)
エクスプローラーで `build.bat` をダブルクリックするか、ターミナルで実行してください。
```cmd
build.bat
```

#### PowerShell スクリプトから実行
```powershell
# すべての配布パターンを一括ビルド (dist フォルダに出力)
.\build.ps1 -Type all

# ライブラリ同梱・単一EXE版 (Single File)
.\build.ps1 -Type single-file

# ライブラリ同梱・自己完結フォルダ版 (Self-Contained)
.\build.ps1 -Type self-contained

# ライブラリ非同梱・軽量版 (Framework-Dependent)
.\build.ps1 -Type framework-dependent
```

---

### 2. コマンドライン (`dotnet publish`) からの手動ビルド

#### 📦 A. ライブラリ同梱・単一EXE版 (Single-File / 自己完結型)
> ランタイムや依存 DLL をすべて 1 つの EXE にパッケージングした配布に最適な形式です。

```powershell
dotnet publish DiskCleanManager\DiskCleanManager.csproj -c Release -r win-x64 --self-contained true -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist\DiskCleanManager-x64-SingleFile
```

#### 📂 B. ライブラリ同梱・フォルダ版 (Self-Contained / 自己完結型)
> .NET ランタイムと Windows App SDK を同梱したフォルダ形式です。PC 側に .NET 9 が入っていなくても動作します。

```powershell
dotnet publish DiskCleanManager\DiskCleanManager.csproj -c Release -r win-x64 --self-contained true -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true -p:PublishSingleFile=false -o dist\DiskCleanManager-x64-SelfContained
```

#### 🪶 C. ライブラリ非同梱・軽量版 (Framework-Dependent / フレームワーク依存型)
> ランタイムを含まないためファイルサイズが小さく、ビルドも高速です。実行環境に .NET 9 Desktop Runtime が必要です。

```powershell
dotnet publish DiskCleanManager\DiskCleanManager.csproj -c Release -r win-x64 --self-contained false -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=false -p:PublishTrimmed=false -p:PublishReadyToRun=false -p:PublishSingleFile=false -o dist\DiskCleanManager-x64-FrameworkDependent
```

---

## 📖 使い方

1. **アプリの起動**
   - 生成された `DiskCleanManager.exe` を起動します。
   - ※管理者権限が必要なシステムフォルダを操作する場合は「管理者として実行」してください。
2. **スキャン**
   - 上部の「**一括スキャン**」をクリックすると、検出された対象フォルダの容量が高速計算されます。
3. **クリーンアップ**
   - 削除したい項目のチェックボックスをオンにして「**選択項目をクリーンアップ**」を実行するか、各行のアクションボタンから直接削除・コマンド実行します。
4. **別ドライブへの退避 (ジャンクション)**
   - 各アイテムのメニューから「**別ドライブへ移動**」を選択し、移動先パス（例: `D:\Offload\npm-cache`）を指定すると、自動でフォルダ移動とジャンクション作成が行われます。

---

## 📁 プロジェクト構成

```
diskclean/
├── DiskCleanManager/           # アプリケーション本体プロジェクト
│   ├── Assets/                 # アイコン・ロゴ・画像リソース
│   ├── Data/                   # 組み込みキャッシュ定義データ (BuiltinCacheData.cs)
│   ├── Languages/              # 多言語 JSON リソース (ja.json, en.json)
│   ├── Models/                 # データモデル
│   ├── Services/               # スキャン、コマンド実行、シンボリックリンク処理
│   ├── ViewModels/             # MVVM ViewModel
│   ├── MainWindow.xaml         # メインウィンドウ UI 定義
│   └── DiskCleanManager.csproj # プロジェクトファイル
├── dist/                       # ビルド成果物出力先
├── build.bat                   # 対話型ビルドバッチ
├── build.ps1                   # PowerShell ビルドスクリプト
└── README.md                   # 本ドキュメント
```

---

## 📄 ライセンス

本プロジェクトは MIT License のもとで公開されています。
