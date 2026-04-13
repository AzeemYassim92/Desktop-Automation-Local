using System.Windows;
using System.Windows.Interop;
using DesktopAutomationLab.Interop;
using DesktopAutomationLab.Models;

namespace DesktopAutomationLab.Services;

public class HotkeyService : IDisposable
{
    private readonly Window _window;
    private HwndSource? _source;
    private bool _isRegistered;
    private const int HotkeyId = 9001;

    public event EventHandler? HotkeyPressed;

    public HotkeyService(Window window)
    {
        _window = window;
    }

    public bool IsRegistered => _isRegistered;

    public void Initialize()
    {
        var helper = new WindowInteropHelper(_window);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(WndProc);
    }

    public bool Register(HotkeySettings settings, out string message)
    {
        if (_source is null)
        {
            message = "Hotkey source was not initialized.";
            return false;
        }

        if (!Enum.TryParse(settings.KeyText, true, out System.Windows.Input.Key key))
        {
            message = $"Could not parse key '{settings.KeyText}'.";
            return false;
        }

        var virtualKey = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        uint modifiers = 0;
        if (settings.Ctrl) modifiers |= NativeMethods.MOD_CONTROL;
        if (settings.Alt) modifiers |= NativeMethods.MOD_ALT;
        if (settings.Shift) modifiers |= NativeMethods.MOD_SHIFT;
        if (settings.Win) modifiers |= NativeMethods.MOD_WIN;

        Unregister();

        _isRegistered = NativeMethods.RegisterHotKey(
            _source.Handle,
            HotkeyId,
            modifiers,
            virtualKey);

        message = _isRegistered
            ? $"Registered hotkey: {Describe(settings)}"
            : "Windows rejected the hotkey registration.";

        return _isRegistered;
    }

    public void Unregister()
    {
        if (_source is not null && _isRegistered)
        {
            NativeMethods.UnregisterHotKey(_source.Handle, HotkeyId);
            _isRegistered = false;
        }
    }

    public static string Describe(HotkeySettings settings)
    {
        var parts = new List<string>();
        if (settings.Ctrl) parts.Add("Ctrl");
        if (settings.Alt) parts.Add("Alt");
        if (settings.Shift) parts.Add("Shift");
        if (settings.Win) parts.Add("Win");
        parts.Add(settings.KeyText.ToUpperInvariant());
        return string.Join(" + ", parts);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        if (_source is not null)
        {
            _source.RemoveHook(WndProc);
        }
    }
}
