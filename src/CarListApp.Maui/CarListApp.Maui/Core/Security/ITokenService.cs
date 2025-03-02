using System.Threading.Tasks;

namespace CarListApp.Maui.Core.Security
{
    public interface ITokenService
    {
        Task<string?> GetTokenAsync();
        Task SaveTokenAsync(string token);
        Task RemoveTokenAsync();
        bool IsTokenValid(string token);
    }
} 