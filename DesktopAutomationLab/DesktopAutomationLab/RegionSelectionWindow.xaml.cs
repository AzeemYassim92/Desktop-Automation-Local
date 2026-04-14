using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopAutomationLab.Models;

namespace DesktopAutomationLab;

public partial class RegionSelectionWindow : Window
{
    private Point? _dragStart;

    public RegionDefinition? SelectedRegion { get; private set; }

    public RegionSelectionWindow()
    {
        InitializeComponent();
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(this);
        SelectionRectangle.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionRectangle, _dragStart.Value.X);
        Canvas.SetTop(SelectionRectangle, _dragStart.Value.Y);
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
        Mouse.Capture(RootCanvas);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragStart is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var current = e.GetPosition(this);
        var left = Math.Min(_dragStart.Value.X, current.X);
        var top = Math.Min(_dragStart.Value.Y, current.Y);
        var width = Math.Abs(current.X - _dragStart.Value.X);
        var height = Math.Abs(current.Y - _dragStart.Value.Y);

        Canvas.SetLeft(SelectionRectangle, left);
        Canvas.SetTop(SelectionRectangle, top);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragStart is null)
        {
            return;
        }

        var current = e.GetPosition(this);
        var left = Math.Min(_dragStart.Value.X, current.X);
        var top = Math.Min(_dragStart.Value.Y, current.Y);
        var width = Math.Abs(current.X - _dragStart.Value.X);
        var height = Math.Abs(current.Y - _dragStart.Value.Y);

        Mouse.Capture(null);
        _dragStart = null;

        if (width < 1 || height < 1)
        {
            DialogResult = false;
            Close();
            return;
        }

        SelectedRegion = new RegionDefinition
        {
            X = (int)Math.Round(Left + left),
            Y = (int)Math.Round(Top + top),
            Width = (int)Math.Round(width),
            Height = (int)Math.Round(height)
        };

        DialogResult = true;
        Close();
    }
}
