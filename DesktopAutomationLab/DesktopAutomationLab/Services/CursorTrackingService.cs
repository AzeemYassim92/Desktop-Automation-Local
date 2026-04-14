using System.Runtime.InteropServices;

namespace DesktopAutomationLab.Services;

public class CursorTrackingService
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    public (int X, int Y) GetCursorPosition()
    {
        GetCursorPos(out POINT point);
        return (point.X, point.Y);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}