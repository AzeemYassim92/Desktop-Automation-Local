namespace DesktopAutomationLab.Models;

public class HotkeySettings
{
    public bool Ctrl { get; set; } = true;
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }
    public string KeyText { get; set; } = "F8";
}
