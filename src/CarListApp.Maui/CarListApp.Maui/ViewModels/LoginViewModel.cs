using CarListApp.Maui.Helpers;
using CarListApp.Maui.Models;
using CarListApp.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarListApp.Maui.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(CarApiService carApiService)
        {
            this.carApiService = carApiService;
        }

        [ObservableProperty]
        string username;

        [ObservableProperty]
        string password;
        private CarApiService carApiService;

        [RelayCommand]
        async Task Login()
        {
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayLoginMessage("Invalid Login Attempt");
            }
            else
            {
                // Call API to attempt a login
                var loginModel = new LoginModel(username, password);

                var response = await carApiService.Login(loginModel);

                // display message
                await DisplayLoginMessage(carApiService.StatusMessage);

                if (!string.IsNullOrEmpty(response.Token))
                {
                    // Store token in secure storage 
                    try
                    {
                        await SecureStorage.Default.SetAsync("Token", response.Token);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        // Handle platforms where secure storage is not supported
                        await DisplayLoginMessage("Secure storage is not supported on this device");
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Handle or log the exception
                        await DisplayLoginMessage($"Error storing token: {ex.Message}");
                        return;
                    }

                    // build a menu on the fly...based on the user role
                    var jsonToken = new JwtSecurityTokenHandler().ReadToken(response.Token) as JwtSecurityToken;

                    var role = jsonToken.Claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Role))?.Value;

                    App.UserInfo = new UserInfo() 
                    {
                        Username = Username,
                        Role = role
                    };

                    // navigate to app's main page
                    MenuBuilder.BuildMenu();
                    Application.Current.MainPage = new AppShell();
                    await Shell.Current.GoToAsync("//MainPage");

                }
                else
                {
                    await DisplayLoginMessage("Invalid Login Attempt");
                }
            }
        }

        async Task DisplayLoginMessage(string message)
        {
            await Shell.Current.DisplayAlert("Login Attempt Result", message, "OK");
            Password = string.Empty;
        }
    }
}
