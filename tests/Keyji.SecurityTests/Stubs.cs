using System.Text;

namespace Keyji
{
    public static class App { public const string TrayArg = "--tray"; }
}

namespace Keyji.Localization
{
    public static class Loc { public const string SystemPreference = "System"; }
}

namespace Keyji.Theming
{
    public enum ThemePreference { System, Light, Dark }
}

namespace Keyji.Diagnostics
{
    public static class Trace { public static void Line(string message) { } }
}

namespace Keyji.Native
{
    internal static class NativeMethods
    {
        internal const uint ILOT_UNINSTALL = 1;
        internal const uint WM_INPUTLANGCHANGEREQUEST = 0x50;
        internal const int INPUTLANGCHANGE_SYSTEMCHARSET = 1;
        internal const uint WM_IME_CONTROL = 0x283;
        internal const int IMC_SETOPENSTATUS = 6;
        internal const int IMC_SETCONVERSIONMODE = 2;
        internal const int IMC_GETCONVERSIONMODE = 1;
        internal const int IMC_GETOPENSTATUS = 5;
        internal const int IME_CMODE_HIRAGANA = 0x19;

        internal static int Installs;
        internal static int ImeSets;
        internal static bool InstallLayoutOrTip(string value, uint flags) { Installs++; return true; }
        internal static int GetKeyboardLayoutList(int count, IntPtr[]? list)
        {
            if (list is not null) list[0] = new IntPtr(0x0411);
            return 1;
        }
        internal static uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr ignored) => 1;
        internal static IntPtr GetKeyboardLayout(uint thread) => new(0x0411);
        internal static IntPtr GetForegroundWindow() => new(1);
        internal static IntPtr PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam) => new(1);
        internal static IntPtr ImmGetDefaultIMEWnd(IntPtr hwnd) => new(2);
        internal static IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (wParam.ToInt32() is IMC_SETOPENSTATUS or IMC_SETCONVERSIONMODE) ImeSets++;
            return IntPtr.Zero;
        }
        internal static bool GetGUIThreadInfo(uint thread, ref GUITHREADINFO info)
        {
            info.hwndFocus = new IntPtr(3);
            return true;
        }
        internal static int GetClassName(IntPtr hwnd, StringBuilder buffer, int count) => 0;
        internal static int GetWindowText(IntPtr hwnd, StringBuilder buffer, int count) => 0;
        internal struct GUITHREADINFO { public int cbSize; public IntPtr hwndFocus; }
    }
}
