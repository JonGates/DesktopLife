using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLife.Diagnostics;

/// <summary>A clean main-screen background for recording the separately running application.</summary>
internal static class RecordingBackdrop
{
    public static void Run()
    {
        var app = new Application();
        var canvas = new Grid
        {
            Background = new LinearGradientBrush(Color.FromRgb(242, 247, 239), Color.FromRgb(209, 229, 229), 25)
        };
        var heading = new StackPanel { Margin = new Thickness(90, 75, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        heading.Children.Add(new TextBlock { Text = "DesktopLife", FontSize = 58, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(27, 64, 64)) });
        heading.Children.Add(new TextBlock { Text = "让桌面，生动一点。", FontSize = 23, Margin = new Thickness(2, 12, 0, 0), Foreground = Brushes.DarkSlateGray });
        canvas.Children.Add(heading);
        canvas.Children.Add(new TextBlock { Text = "鼠标旁的苍蝇 · 自由爬行的昆虫", FontSize = 18, Foreground = Brushes.DarkSlateGray, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(92, 0, 0, 72) });
        var window = new Window
        {
            Title = "DesktopLife Recording Background", WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize, Left = 0, Top = 0,
            Width = SystemParameters.PrimaryScreenWidth, Height = SystemParameters.PrimaryScreenHeight,
            Content = canvas, ShowInTaskbar = true
        };
        window.KeyDown += (_, e) => { if (e.Key == Key.Escape) window.Close(); };
        app.Run(window);
    }
}
