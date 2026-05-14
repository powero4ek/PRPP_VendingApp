using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VendingDesktop.Services;
using VendingDesktop.ViewModels;
using VendingDesktop.Views;

namespace VendingDesktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var collection = new ServiceCollection();
        collection.AddHttpClient<ApiService>(c => c.BaseAddress = new Uri("http://localhost:5000/api/"));
        collection.AddSingleton<NotificationService>();
        collection.AddTransient<MainViewModel>();
        collection.AddTransient<LoginViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<MonitorViewModel>();
        collection.AddTransient<AdminViewModel>();
        Services = collection.BuildServiceProvider();

        var main = new MainWindow { DataContext = Services.GetRequiredService<MainViewModel>() };
        main.Show();
    }
}
