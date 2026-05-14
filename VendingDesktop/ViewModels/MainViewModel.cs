using System.Windows.Input;
using VendingDesktop.Services;
using VendingDesktop.Views;

namespace VendingDesktop.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly NotificationService _notif;
    private object? _currentView;
    private UserViewModel? _currentUser;
    private bool _isLoggedIn;

    public object? CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(); } }
    public UserViewModel? CurrentUser { get => _currentUser; set { _currentUser = value; OnPropertyChanged(); } }
    public bool IsLoggedIn { get => _isLoggedIn; set { _isLoggedIn = value; OnPropertyChanged(); } }

    public ICommand ShowHomeCommand { get; }
    public ICommand ShowMonitorCommand { get; }
    public ICommand ShowAdminCommand { get; }
    public ICommand ShowReportsCommand { get; }
    public ICommand ShowInventoryCommand { get; }
    public ICommand LogoutCommand { get; }

    public MainViewModel(ApiService api, NotificationService notif)
    {
        _api = api; _notif = notif;
        ShowHomeCommand = new RelayCommand(_ => ShowHome(), _ => IsLoggedIn);
        ShowMonitorCommand = new RelayCommand(_ => ShowMonitor(), _ => IsLoggedIn);
        ShowAdminCommand = new RelayCommand(_ => ShowAdmin(), _ => IsLoggedIn);
        ShowReportsCommand = new RelayCommand(_ => {}, _ => IsLoggedIn);
        ShowInventoryCommand = new RelayCommand(_ => {}, _ => IsLoggedIn);
        LogoutCommand = new RelayCommand(_ => Logout());
        CurrentView = new LoginView { DataContext = new LoginViewModel(api, this) };
    }

    public void OnLoginSuccess(AuthResponse auth)
    {
        _api.Token = auth.Token;
        CurrentUser = new UserViewModel { FullName = auth.FullName, Role = auth.Role, PhotoUrl = auth.PhotoUrl };
        IsLoggedIn = true;
        ShowHome();
        _notif.Show($"Добро пожаловать, {auth.FullName}!", NotificationType.Info, 5000);
    }

    private void ShowHome() => CurrentView = new HomeView { DataContext = new HomeViewModel(_api) };
    private void ShowMonitor() => CurrentView = new MonitorView { DataContext = new MonitorViewModel(_api, _notif) };
    private void ShowAdmin() => CurrentView = new AdminView { DataContext = new AdminViewModel(_api, _notif) };
    private void Logout()
    {
        _api.Token = null;
        IsLoggedIn = false;
        CurrentUser = null;
        CurrentView = new LoginView { DataContext = new LoginViewModel(_api, this) };
    }
}

public class UserViewModel : BaseViewModel
{
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? PhotoUrl { get; set; }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    { _execute = execute; _canExecute = canExecute; }
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
}
