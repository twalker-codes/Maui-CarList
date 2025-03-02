using System.Threading.Tasks;

namespace CarListApp.Maui.Core.Security
{
    public class SecureStorageCredentialService : ICredentialService
    {
        private const string UsernameKey = "LastUsername";
        private const string PasswordKey = "LastPassword";

        public async Task SaveCredentialsAsync(string username, string password)
        {
            try
            {
                await SecureStorage.Default.SetAsync(UsernameKey, username);
                await SecureStorage.Default.SetAsync(PasswordKey, password);
            }
            catch (Exception)
            {
                // Log error or handle appropriately
                throw;
            }
        }

        public async Task<(string Username, string Password)> GetSavedCredentialsAsync()
        {
            try
            {
                var username = await SecureStorage.Default.GetAsync(UsernameKey) ?? string.Empty;
                var password = await SecureStorage.Default.GetAsync(PasswordKey) ?? string.Empty;
                return (username, password);
            }
            catch (Exception)
            {
                return (string.Empty, string.Empty);
            }
        }

        public Task ClearCredentialsAsync()
        {
            try
            {
                SecureStorage.Default.Remove(UsernameKey);
                SecureStorage.Default.Remove(PasswordKey);
                return Task.CompletedTask;
            }
            catch (Exception)
            {
                // Log error or handle appropriately
                return Task.CompletedTask;
            }
        }
    }
} 