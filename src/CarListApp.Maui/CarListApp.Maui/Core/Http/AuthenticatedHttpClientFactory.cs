using CarListApp.Maui.Core.Security;
using Serilog;

namespace CarListApp.Maui.Core.Http
{
    public class AuthenticatedHttpClientFactory : IHttpClientFactory
    {
        private readonly ITokenService _tokenService;
        private readonly string _baseAddress;

        public AuthenticatedHttpClientFactory(ITokenService tokenService)
        {
            _tokenService = tokenService;
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? 
                "http://10.0.2.2:5001" : "http://localhost:5001";
            Log.Information("AuthenticatedHttpClientFactory initialized with base address: {BaseAddress}", _baseAddress);
        }

        public async Task<HttpClient> CreateClient(bool requiresAuth = true)
        {
            try
            {
                Log.Debug("Creating HTTP client, requiresAuth: {RequiresAuth}", requiresAuth);
                var client = new HttpClient { BaseAddress = new Uri(_baseAddress) };
                
                if (requiresAuth)
                {
                    Log.Debug("Retrieving token from secure storage");
                    var token = await _tokenService.GetTokenAsync();
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        Log.Warning("No authentication token found when creating HTTP client");
                        throw new UnauthorizedAccessException("No valid authentication token found");
                    }

                    if (!_tokenService.IsTokenValid(token))
                    {
                        Log.Warning("Invalid or expired token found");
                        await _tokenService.RemoveTokenAsync();
                        throw new UnauthorizedAccessException("Authentication token is invalid or expired");
                    }

                    Log.Debug("Adding authentication token to HTTP client");
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                return client;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating HTTP client");
                throw;
            }
        }
    }
} 