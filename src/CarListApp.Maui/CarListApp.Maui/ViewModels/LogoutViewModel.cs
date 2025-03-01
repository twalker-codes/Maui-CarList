using CommunityToolkit.Mvvm.Input;

namespace CarListApp.Maui.ViewModels
{
    public partial class LogoutViewModel : BaseViewModel
    {
        public LogoutViewModel()
        {
            Logout();
        }


        [RelayCommand]
        async void Logout()
        {
            try
            {
                SecureStorage.Default.Remove("Token");
                App.UserInfo = null;
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception)
            {
                // Even if secure storage fails, we still want to log out
                App.UserInfo = null;
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}