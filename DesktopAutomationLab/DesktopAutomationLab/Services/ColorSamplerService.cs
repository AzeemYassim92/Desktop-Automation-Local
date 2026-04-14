using System.Drawing;
using DesktopAutomationLab.Models;

namespace DesktopAutomationLab.Services;

public class ColorSamplerService
{
    public ColorSample SampleAt(int x, int y)
    {
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
        var color = bitmap.GetPixel(0, 0);

        return new ColorSample
        {
            X = x,
            Y = y,
            Red = color.R,
            Green = color.G,
            Blue = color.B
        };
    }

    public ColorSample SampleRegionCenter(RegionDefinition region)
    {
        var centerX = region.X + Math.Max(region.Width / 2, 0);
        var centerY = region.Y + Math.Max(region.Height / 2, 0);
        return SampleAt(centerX, centerY);
    }

    public void SaveScreenshot(RegionDefinition region, string filePath)
    {
        var width = Math.Max(region.Width, 1);
        var height = Math.Max(region.Height, 1);

        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(region.X, region.Y, 0, 0, new Size(width, height));

        bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
    }
}
