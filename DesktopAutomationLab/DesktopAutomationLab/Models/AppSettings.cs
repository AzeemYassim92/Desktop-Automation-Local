namespace DesktopAutomationLab.Models;

public class AppSettings
{
    public HotkeySettings Hotkey { get; set; } = new();
    public RegionDefinition Region { get; set; } = new();
    public int SampleIntervalMs { get; set; } = 300;
}
