using Microsoft.UI.Xaml;
using DiskCleanManager.Converters;

namespace DiskCleanManager;

public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(Window);

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Converters をアプリケーションリソースにコードから登録
        // （WinUI 3 XAML コンパイラは LocalAssembly の型を XAML から解決できないため）
        Resources["SafetyColor"] = new SafetyLevelToColorConverter();
        Resources["SafetyEmoji"] = new SafetyLevelToEmojiConverter();
        Resources["SafetyText"]  = new SafetyLevelToTextConverter();
        Resources["CmdVisible"]  = new ActionTypeToCommandVisibleConverter();
        Resources["SymVisible"]  = new ActionTypeToSymlinkVisibleConverter();
        Resources["ForbVisible"] = new ActionTypeToForbiddenVisibleConverter();
        Resources["BoolVisible"] = new BoolToVisibilityConverter();
        Resources["InvBoolVisible"] = new InverseBoolToVisibilityConverter();
        Resources["LinkColor"]   = new LinkedStatusToColorConverter();
        Resources["LinkText"]    = new LinkedStatusToTextConverter();
        Resources["InvBool"]     = new InverseBoolConverter();

        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }
}
