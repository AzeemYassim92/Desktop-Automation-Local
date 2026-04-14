using System.Drawing;
using DesktopAutomationLab.Models;

namespace DesktopAutomationLab.Services;

public class ColorSamplerService
{
    public ColorSample SampleRegionCenter(RegionDefinition region)
    {
        var width = Math.Max(region.Width, 1);
        var height = Math.Max(region.Height, 1);

        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(region.X, region.Y, 0, 0, new Size(width, height));

        var centerX = width / 2;
        var centerY = height / 2;

        var pixel = bitmap.GetPixel(centerX, centerY);

        return new ColorSample
        {
            X = region.X + centerX,
            Y = region.Y + centerY,
            Red = pixel.R,
            Green = pixel.G,
            Blue = pixel.B
        };
    }
}