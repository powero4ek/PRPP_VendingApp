using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace VendingDesktop.Services;

public class NotificationService
{
    private readonly Queue<Notification> _queue = new();
    private bool _isShowing;

    public void Show(string message, NotificationType type, int durationMs = 5000)
    {
        _queue.Enqueue(new Notification { Message = message, Type = type, DurationMs = durationMs });
        if (!_isShowing) _ = ProcessQueueAsync();
    }

    private async Task ProcessQueueAsync()
    {
        _isShowing = true;
        while (_queue.Count > 0)
        {
            var n = _queue.Dequeue();
            await ShowToastAsync(n);
        }
        _isShowing = false;
    }

    private async Task ShowToastAsync(Notification n)
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        if (window == null) return;

        var grid = new Grid
        {
            Background = n.Type switch
            {
                NotificationType.Critical => new SolidColorBrush(Colors.DarkRed),
                NotificationType.Warning => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.DodgerBlue)
            },
            Margin = new Thickness(10),
            Width = 350,
            Height = 80
        };

        var sp = new StackPanel { Margin = new Thickness(10) };
        var tb = new TextBlock { Text = n.Message, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap };
        var closeBtn = new Button { Content = "×", HorizontalAlignment = HorizontalAlignment.Right, Background = Brushes.Transparent, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        closeBtn.Click += (s, e) =>
        {
            if (grid.Parent is Panel p) p.Children.Remove(grid);
        };
        sp.Children.Add(closeBtn);
        sp.Children.Add(tb);
        grid.Children.Add(sp);

        var container = window.Content as Panel;
        if (container == null)
        {
            var original = window.Content as UIElement;
            container = new Grid();
            window.Content = container;
            if (original != null) container.Children.Add(original);
        }
        container.Children.Add(grid);
        Panel.SetZIndex(grid, 9999);

        await Task.Delay(n.DurationMs);
        if (grid.Parent is Panel p2) p2.Children.Remove(grid);
    }
}

public class Notification
{
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; }
    public int DurationMs { get; set; }
}

public enum NotificationType { Info, Warning, Critical }
