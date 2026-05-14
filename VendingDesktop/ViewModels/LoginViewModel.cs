using System.Windows.Input;
using VendingDesktop.Services;

namespace VendingDesktop.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly MainViewModel _main;
    private string _email = "";
    private string _password = "";
    private string _error = "";

    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    public ICommand LoginCommand { get; }

    public LoginViewModel(ApiService api, MainViewModel main)
    {
        _api = api; _main = main;
        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password));
    }

    private async Task LoginAsync()
    {
        var auth = await _api.LoginAsync(Email, Password);
        if (auth != null) _main.OnLoginSuccess(auth);
        else Error = "Неверный логин или пароль";
    }
}
