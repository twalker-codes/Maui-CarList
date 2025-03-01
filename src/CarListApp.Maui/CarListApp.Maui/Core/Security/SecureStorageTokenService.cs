using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CarListApp.Maui.Core.Security
{
    public class SecureStorageTokenService : ITokenService
    {
        private const string TokenKey = "auth_token";
        private string? _cachedToken;
        private readonly ILogger<SecureStorageTokenService> _logger;

        public SecureStorageTokenService(ILogger<SecureStorageTokenService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Log.Information("SecureStorageTokenService initialized");
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cachedToken))
                {
                    Log.Debug("Returning cached token");
                    return _cachedToken;
                }

                Log.Debug("Retrieving token from secure storage");
                _cachedToken = await SecureStorage.Default.GetAsync(TokenKey);

                if (string.IsNullOrEmpty(_cachedToken))
                {
                    Log.Information("No token found in secure storage");
                    return null;
                }

                if (!IsTokenValid(_cachedToken))
                {
                    Log.Warning("Retrieved token is invalid or expired");
                    await RemoveTokenAsync();
                    return null;
                }

                Log.Debug("Token retrieved successfully");
                return _cachedToken;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving token");
                return null;
            }
        }

        public async Task SaveTokenAsync(string token)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(token);

                Log.Debug("Saving token to secure storage");
                await SecureStorage.Default.SetAsync(TokenKey, token);
                _cachedToken = token;
                LogTokenDetails(token);
                Log.Information("Token saved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving token");
                throw;
            }
        }

        public async Task RemoveTokenAsync()
        {
            try
            {
                Log.Debug("Removing token from secure storage");
                SecureStorage.Default.Remove(TokenKey);
                _cachedToken = null;
                Log.Information("Token removed successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error removing token");
                throw;
            }
        }

        public bool IsTokenValid(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    Log.Debug("Token is null or empty");
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken == null)
                {
                    Log.Warning("Could not parse JWT token");
                    return false;
                }

                var expiration = jwtToken.ValidTo;
                var isValid = expiration > DateTime.UtcNow;

                Log.Debug("Token validity checked. Valid: {IsValid}, Expires: {Expiration}", 
                    isValid, expiration);

                return isValid;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating token");
                return false;
            }
        }

        private void LogTokenDetails(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                Log.Debug("Token details - Expires: {Expiration}, Issuer: {Issuer}", 
                    jwtToken.ValidTo, jwtToken.Issuer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error logging token details");
            }
        }
    }
} 