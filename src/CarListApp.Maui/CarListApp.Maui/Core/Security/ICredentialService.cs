using System.Threading.Tasks;

namespace CarListApp.Maui.Core.Security
{
    public interface ICredentialService
    {
        Task SaveCredentialsAsync(string username, string password);
        Task<(string Username, string Password)> GetSavedCredentialsAsync();
        Task ClearCredentialsAsync();
    }
} 