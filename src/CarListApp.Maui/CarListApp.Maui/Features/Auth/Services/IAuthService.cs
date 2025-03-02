using CarListApp.Maui.Features.Auth.Models;
using System.Threading.Tasks;

namespace CarListApp.Maui.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginModel loginModel);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<UserInfo> GetCurrentUserAsync();
    }
} 