using System.Windows.Forms;

namespace DesktopAutomationLab.Services;

public class CursorTrackingService
{
    public (int X, int Y) GetCursorPosition()
    {
        var position = Cursor.Position;
        return (position.X, position.Y);
    }
}
