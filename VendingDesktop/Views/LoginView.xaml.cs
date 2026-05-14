using System.Windows;
using System.Windows.Controls;
using VendingDesktop.ViewModels;
namespace VendingDesktop.Views;
public partial class LoginView : UserControl
{
    public LoginView() => InitializeComponent();
    private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm) vm.Password = PwdBox.Password;
    }
}
