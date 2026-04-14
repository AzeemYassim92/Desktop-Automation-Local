using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DesktopAutomationLab.Models;
using DesktopAutomationLab.Services;

namespace DesktopAutomationLab.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly Window _window;
    private readonly SettingsService _settingsService;
    private readonly CursorTrackingService _cursorTrackingService;
    private readonly ColorSamplerService _colorSamplerService;
    private readonly HotkeyService _hotkeyService;
    private readonly DispatcherTimer _samplerTimer;

    private int _regionStartX;
    private int _regionStartY;
    private bool _hasRegionStart;
    private bool _isRunning;

    public MainViewModel(Window window)
    {
        _window = window;
        _settingsService = new SettingsService();
        _cursorTrackingService = new CursorTrackingService();
        _colorSamplerService = new ColorSamplerService();
        _hotkeyService = new HotkeyService(window);
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        _samplerTimer = new DispatcherTimer();
        _samplerTimer.Tick += OnSamplerTick;

        Settings = new AppSettings();
        CurrentSample = new ColorSample();

        RegisterHotkeyCommand = new RelayCommand(RegisterHotkey);
        UnregisterHotkeyCommand = new RelayCommand(UnregisterHotkey);
        CaptureCursorCommand = new RelayCommand(CaptureCursor);
        SetRegionStartCommand = new RelayCommand(SetRegionStart);
        SetRegionEndCommand = new RelayCommand(SetRegionEnd);
        CenterRegionOnCursorCommand = new RelayCommand(CenterRegionOnCursor);
        SaveRegionScreenshotCommand = new RelayCommand(SaveRegionScreenshot);
        SelectRegionAndCaptureCommand = new RelayCommand(SelectRegionAndCapture);
        StartSamplingCommand = new RelayCommand(StartSampling);
        StopSamplingCommand = new RelayCommand(StopSampling);
        ExportSamplesCommand = new RelayCommand(ExportSamples);
        SaveSettingsCommand = new RelayCommand(SaveSettings);
        LoadSettingsCommand = new RelayCommand(LoadSettings);

        SettingsFilePath = _settingsService.GetDefaultPath();
        HotkeyStatus = "Not registered.";
        SamplerStatus = "Idle.";
        CursorText = "Cursor: not captured yet.";
        RefreshComputedState();
    }

    public AppSettings Settings { get; private set; }
    public ColorSample CurrentSample { get; private set; }
    public Brush CurrentSampleBrush => new SolidColorBrush(Color.FromRgb(CurrentSample.Red, CurrentSample.Green, CurrentSample.Blue));
    public string CurrentSampleHex => CurrentSample.Hex;
    public string HotkeyStatus { get; private set; }
    public string SamplerStatus { get; private set; }
    public string CursorText { get; private set; }
    public string RegionText { get; private set; } = string.Empty;
    public string SettingsFilePath { get; }
    public string RunningStateText => _isRunning ? "Running" : "Stopped";

    public string SettingsSummary =>
        $"Hotkey: {HotkeyService.Describe(Settings.Hotkey)} | Region: X={Settings.Region.X}, Y={Settings.Region.Y}, W={Settings.Region.Width}, H={Settings.Region.Height} | Interval: {Settings.SampleIntervalMs}ms | Output: {Settings.ScreenshotOutputFolder}";

    public ObservableCollection<string> LogEntries { get; } = new();
    public ObservableCollection<string> RecentSamples { get; } = new();

    public RelayCommand RegisterHotkeyCommand { get; }
    public RelayCommand UnregisterHotkeyCommand { get; }
    public RelayCommand CaptureCursorCommand { get; }
    public RelayCommand SetRegionStartCommand { get; }
    public RelayCommand SetRegionEndCommand { get; }
    public RelayCommand CenterRegionOnCursorCommand { get; }
    public RelayCommand SaveRegionScreenshotCommand { get; }
    public RelayCommand SelectRegionAndCaptureCommand { get; }
    public RelayCommand StartSamplingCommand { get; }
    public RelayCommand StopSamplingCommand { get; }
    public RelayCommand ExportSamplesCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand LoadSettingsCommand { get; }

    public void Initialize()
    {
        _hotkeyService.Initialize();
        LoadSettings();
        AddLog("Application initialized.");
    }

    public void Dispose()
    {
        StopSampling();
        _hotkeyService.Dispose();
    }

    private void RegisterHotkey()
    {
        if (_hotkeyService.Register(Settings.Hotkey, out var message))
        {
            HotkeyStatus = message;
            AddLog(message);
        }
        else
        {
            HotkeyStatus = message;
            AddLog($"Hotkey error: {message}");
        }

        RaiseStateProperties();
    }

    private void UnregisterHotkey()
    {
        _hotkeyService.Unregister();
        HotkeyStatus = "Hotkey unregistered.";
        AddLog(HotkeyStatus);
        RaiseStateProperties();
    }

    private void CaptureCursor()
        => _ = CaptureCursorDelayedAsync();

    private async Task CaptureCursorDelayedAsync()
    {
        AddLog("Capture cursor requested. You have 3 seconds.");
        await MinimizeAndDelayAsync();
        var (x, y) = _cursorTrackingService.GetCursorPosition();
        CursorText = $"Cursor: X={x}, Y={y}";
        AddLog($"Captured cursor at ({x}, {y}).");
        RaiseStateProperties();
    }

    private void SetRegionStart()
        => _ = SetRegionStartDelayedAsync();

    private async Task SetRegionStartDelayedAsync()
    {
        AddLog("Set region start requested. Move your cursor to the start point (3 seconds).");
        await MinimizeAndDelayAsync();
        var (x, y) = _cursorTrackingService.GetCursorPosition();
        _regionStartX = x;
        _regionStartY = y;
        _hasRegionStart = true;
        AddLog($"Region start set to ({x}, {y}).");
    }

    private void SetRegionEnd()
        => _ = SetRegionEndDelayedAsync();

    private async Task SetRegionEndDelayedAsync()
    {
        if (!_hasRegionStart)
        {
            AddLog("Cannot set region end before region start.");
            return;
        }

        AddLog("Set region end requested. Move your cursor to the end point (3 seconds).");
        await MinimizeAndDelayAsync();
        var (x, y) = _cursorTrackingService.GetCursorPosition();
        Settings.Region.X = Math.Min(_regionStartX, x);
        Settings.Region.Y = Math.Min(_regionStartY, y);
        Settings.Region.Width = Math.Abs(x - _regionStartX);
        Settings.Region.Height = Math.Abs(y - _regionStartY);

        AddLog($"Region updated to X={Settings.Region.X}, Y={Settings.Region.Y}, W={Settings.Region.Width}, H={Settings.Region.Height}.");
        RefreshComputedState();
    }

    private void CenterRegionOnCursor()
    {
        var (x, y) = _cursorTrackingService.GetCursorPosition();
        Settings.Region.X = x - (Settings.Region.Width / 2);
        Settings.Region.Y = y - (Settings.Region.Height / 2);
        AddLog($"Region centered on cursor at ({x}, {y}).");
        RefreshComputedState();
    }

    private void SaveRegionScreenshot()
    {
        if (!HasValidRegion("Screenshot"))
        {
            return;
        }

        var outputFolder = GetOutputFolder();
        var filePath = Path.Combine(
            outputFolder,
            $"region_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        _colorSamplerService.SaveScreenshot(Settings.Region, filePath);
        AddLog($"Screenshot saved: {filePath}");
    }

    private void SelectRegionAndCapture()
        => _ = SelectRegionAndCaptureAsync();

    private async Task SelectRegionAndCaptureAsync()
    {
        AddLog("Drag to select a region on screen.");
        _window.WindowState = WindowState.Minimized;
        await Task.Delay(150);

        var selector = new RegionSelectionWindow();
        var result = selector.ShowDialog();

        _window.WindowState = WindowState.Normal;
        _window.Activate();

        if (result != true || selector.SelectedRegion is null)
        {
            AddLog("Region selection was cancelled.");
            return;
        }

        Settings.Region = selector.SelectedRegion;
        RefreshComputedState();
        RaisePropertyChanged(nameof(Settings));
        AddLog($"Selected region: X={Settings.Region.X}, Y={Settings.Region.Y}, W={Settings.Region.Width}, H={Settings.Region.Height}.");

        SaveRegionScreenshot();
    }

    private void StartSampling()
    {
        if (!HasValidRegion("Sampling"))
        {
            return;
        }

        var interval = Math.Max(Settings.SampleIntervalMs, 50);
        if (Settings.SampleIntervalMs != interval)
        {
            Settings.SampleIntervalMs = interval;
            AddLog("Sample interval was below 50ms and has been clamped.");
        }

        _samplerTimer.Interval = TimeSpan.FromMilliseconds(interval);
        _samplerTimer.Start();
        SamplerStatus = $"Sampling every {interval} ms.";
        AddLog(SamplerStatus);
        RaiseStateProperties();
    }

    private void StopSampling()
    {
        _samplerTimer.Stop();
        SamplerStatus = "Idle.";
        RaiseStateProperties();
    }

    private void SaveSettings()
    {
        _settingsService.Save(Settings, SettingsFilePath);
        AddLog($"Settings saved to {SettingsFilePath}.");
        RaiseStateProperties();
    }

    private void ExportSamples()
    {
        if (RecentSamples.Count == 0)
        {
            AddLog("No samples to export yet.");
            return;
        }

        var outputFolder = GetOutputFolder();
        var filePath = Path.Combine(
            outputFolder,
            $"sample_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        File.WriteAllLines(filePath, RecentSamples.Reverse());
        AddLog($"Sample list exported: {filePath}");
    }

    private void LoadSettings()
    {
        Settings = _settingsService.Load(SettingsFilePath);
        ValidateSettings();
        AddLog($"Settings loaded from {SettingsFilePath}.");
        RefreshComputedState();
        RaisePropertyChanged(nameof(Settings));
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        _isRunning = !_isRunning;
        AddLog($"Hotkey pressed. Running state changed to {RunningStateText}.");
        RaiseStateProperties();
    }

    private void OnSamplerTick(object? sender, EventArgs e)
    {
        if (!HasValidRegion("Sampling"))
        {
            StopSampling();
            return;
        }

        var sample = _colorSamplerService.SampleRegionCenter(Settings.Region);
        CurrentSample = sample;

        RecentSamples.Insert(0, sample.ToString());
        while (RecentSamples.Count > 25)
        {
            RecentSamples.RemoveAt(RecentSamples.Count - 1);
        }

        RaisePropertyChanged(nameof(CurrentSample));
        RaisePropertyChanged(nameof(CurrentSampleBrush));
        RaisePropertyChanged(nameof(CurrentSampleHex));
    }

    private void RefreshComputedState()
    {
        RegionText = $"Region: X={Settings.Region.X}, Y={Settings.Region.Y}, W={Settings.Region.Width}, H={Settings.Region.Height}";
        RaiseStateProperties();
    }

    private async Task MinimizeAndDelayAsync()
    {
        var restoreState = _window.WindowState == WindowState.Minimized
            ? WindowState.Normal
            : _window.WindowState;

        _window.WindowState = WindowState.Minimized;
        try
        {
            await Task.Delay(3000);
        }
        finally
        {
            _window.WindowState = restoreState;
            _window.Activate();
        }
    }

    private bool HasValidRegion(string actionName)
    {
        if (Settings.Region.Width > 0 && Settings.Region.Height > 0)
        {
            return true;
        }

        AddLog($"{actionName} requires a valid region (width and height must be greater than 0).");
        return false;
    }

    private string GetOutputFolder()
    {
        var configured = Settings.ScreenshotOutputFolder?.Trim();
        var outputFolder = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "DesktopAutomationLabCaptures")
            : configured;

        Directory.CreateDirectory(outputFolder);
        Settings.ScreenshotOutputFolder = outputFolder;
        return outputFolder;
    }

    private void RaiseStateProperties()
    {
        RaisePropertyChanged(nameof(HotkeyStatus));
        RaisePropertyChanged(nameof(SamplerStatus));
        RaisePropertyChanged(nameof(CursorText));
        RaisePropertyChanged(nameof(RegionText));
        RaisePropertyChanged(nameof(SettingsSummary));
        RaisePropertyChanged(nameof(RunningStateText));
        RaisePropertyChanged(nameof(CurrentSampleBrush));
        RaisePropertyChanged(nameof(CurrentSampleHex));
    }

    private void AddLog(string message)
    {
        LogEntries.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
        while (LogEntries.Count > 200)
        {
            LogEntries.RemoveAt(LogEntries.Count - 1);
        }

        RaisePropertyChanged(nameof(LogEntries));
    }
}
