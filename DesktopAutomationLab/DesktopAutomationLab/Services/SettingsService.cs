using System.IO;
using System.Text.Json;
using DesktopAutomationLab.Models;

namespace DesktopAutomationLab.Services;

public class SettingsService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public string GetDefaultPath()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopAutomationLab");

        Directory.CreateDirectory(root);
        return Path.Combine(root, "appsettings.json");
    }

    public void Save(AppSettings settings, string path)
    {
        var json = JsonSerializer.Serialize(settings, _options);
        File.WriteAllText(path, json);
    }

    public AppSettings Load(string path)
    {
        if (!File.Exists(path))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json, _options) ?? new AppSettings();
    }
}
