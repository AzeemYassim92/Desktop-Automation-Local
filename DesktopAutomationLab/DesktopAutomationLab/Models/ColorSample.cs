namespace DesktopAutomationLab.Models;

public class ColorSample
{
    public int X { get; set; }
    public int Y { get; set; }
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";
    public string GreenBlueText => $"{Green} / {Blue}";
    public override string ToString()
        => $"{DateTime.Now:HH:mm:ss} - ({X}, {Y})  RGB({Red}, {Green}, {Blue})  {Hex}";
}
