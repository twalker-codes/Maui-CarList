using CarListApp.Maui.Helpers;
using CarListApp.Maui.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarListApp.Maui.ViewModels
{
    public partial class LoadingPageViewModel : BaseViewModel
    {
        public LoadingPageViewModel()
        {
            CheckUserLoginDetails();
        }

        private async void CheckUserLoginDetails()
        {
            try
            {
                // Retrieve token from internal storage
                var token = await SecureStorage.Default.GetAsync("Token");
                if (string.IsNullOrEmpty(token))
                {
                    await GoToLoginPage();
                    return;
                }

                var jsonToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    SecureStorage.Default.Remove("Token");
                    await GoToLoginPage();
                    return;
                }

                var role = jsonToken.Claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Role))?.Value;

                App.UserInfo = new UserInfo()
                {
                    Username = jsonToken.Claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Email))?.Value,
                    Role = role
                };
                MenuBuilder.BuildMenu();
                await GoToMainPage();
            }
            catch (PlatformNotSupportedException)
            {
                await GoToLoginPage();
            }
            catch (Exception)
            {
                await GoToLoginPage();
            }
        }

        private async Task GoToLoginPage()
        {
            Application.Current.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task GoToMainPage()
        {
            Application.Current.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}