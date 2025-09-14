using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StartUp;

public class FindWindowHandle
{
    // Delegate for EnumWindows callback
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern int  GetClassName(IntPtr hWnd, StringBuilder sb, int max);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(
        IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);
    private const int DWMWA_CLOAKED = 14;

    private static bool IsCloaked(IntPtr hWnd)
    {
        if (DwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0)
            return cloaked != 0;
        return false;
    }

    public static IntPtr GetWindowHandle(string procTitle, bool electronApp = false, bool packagedApp = false)
    {
        var candidates = new List<IntPtr>();
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindow(hWnd)) return true;

            var title = new StringBuilder(512);
            GetWindowText(hWnd, title, title.Capacity);

            var cls = new StringBuilder(256);
            GetClassName(hWnd, cls, cls.Capacity);

            var t = title.ToString();
            var c = cls.ToString();
            
            bool titleLooksRight = t.IndexOf(procTitle, StringComparison.OrdinalIgnoreCase) >= 0;

            bool electronClass = c.StartsWith("Chrome_WidgetWin_", StringComparison.Ordinal);
            bool packagedAppClass = c.Equals("ApplicationFrameWindow", StringComparison.Ordinal);


            if (titleLooksRight || electronApp && electronClass || packagedApp && packagedAppClass)
                candidates.Add(hWnd);

            return true;
        }, IntPtr.Zero);

        var chosen = candidates
            .OrderByDescending(h => IsWindowVisible(h) && !IsCloaked(h))
            .ThenByDescending(h => !IsCloaked(h))
            .FirstOrDefault();

        return chosen;
    }
}