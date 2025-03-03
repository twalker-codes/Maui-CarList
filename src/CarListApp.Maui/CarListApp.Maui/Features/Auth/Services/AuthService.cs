using CarListApp.Maui.Core.Http;
using CarListApp.Maui.Core.Security;
using CarListApp.Maui.Features.Auth.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using CarListApp.Maui.Features.Profile.Models;
using Serilog;

namespace CarListApp.Maui.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private UserInfo? _currentUser;

        public AuthService(IHttpClientFactory httpClientFactory, ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            Log.Information("AuthService initialized with {HttpClientFactory} and {TokenService}", 
                httpClientFactory.GetType().Name, tokenService.GetType().Name);
        }

        public async Task<AuthResponse> LoginAsync(LoginModel loginModel)
        {
            Log.Information("Login attempt initiated for user: {Username}", loginModel.Username);
            
            try
            {
                Log.Debug("Creating HTTP client for login request");
                var client = await _httpClientFactory.CreateClient(requiresAuth: false);
                var loginEndpoint = "/login";
                Log.Debug("Sending login request to: {BaseAddress}{Endpoint} with username: {Username}", 
                    client.BaseAddress, loginEndpoint, loginModel.Username);
                
                // Log request details
                var requestJson = System.Text.Json.JsonSerializer.Serialize(loginModel);
                Log.Debug("Request payload: {RequestJson}", requestJson);
                
                HttpResponseMessage? response = null;
                try
                {
                    response = await client.PostAsJsonAsync(loginEndpoint, loginModel);
                    Log.Debug("Login HTTP request completed - StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                }
                catch (HttpRequestException httpEx)
                {
                    Log.Error(httpEx, "HTTP request failed during login - Status: {Status}, Message: {Message}", 
                        httpEx.StatusCode, httpEx.Message);
                    throw new UnauthorizedAccessException($"Login failed: Unable to connect to server - {httpEx.Message}", httpEx);
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Log.Debug("Raw response content: {Content}", responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Login failed - Status: {StatusCode}, Reason: {Reason}, Content: {Content}, Headers: {@Headers}", 
                        response.StatusCode, response.ReasonPhrase, responseContent, response.Headers);
                    throw new UnauthorizedAccessException($"Login failed: {response.ReasonPhrase} - {responseContent}");
                }

                Log.Debug("Deserializing successful login response");
                AuthResponse? authResponse = null;
                try
                {
                    authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    Log.Debug("Response deserialization completed - HasToken: {HasToken}", 
                        authResponse?.Token != null);
                }
                catch (JsonException jsonEx)
                {
                    Log.Error(jsonEx, "Failed to deserialize login response - Content: {Content}", responseContent);
                    throw new UnauthorizedAccessException("Login failed: Invalid response format from server", jsonEx);
                }
                
                if (authResponse?.Token == null)
                {
                    Log.Warning("No token received in auth response");
                    throw new UnauthorizedAccessException("No authentication token received");
                }

                Log.Debug("Token received from API - Length: {TokenLength}", authResponse.Token.Length);
                
                try
                {
                    Log.Debug("Attempting to save token to secure storage");
                    await _tokenService.SaveTokenAsync(authResponse.Token);
                    
                    // Verify token was saved
                    var savedToken = await _tokenService.GetTokenAsync();
                    if (string.IsNullOrEmpty(savedToken))
                    {
                        Log.Error("Token verification failed - could not retrieve saved token");
                        throw new UnauthorizedAccessException("Failed to save authentication token");
                    }

                    if (savedToken != authResponse.Token)
                    {
                        Log.Error("Token verification failed - saved token does not match received token");
                        throw new UnauthorizedAccessException("Token verification failed");
                    }
                    
                    Log.Debug("Token successfully saved and verified");
                    _currentUser = ExtractUserInfoFromToken(authResponse.Token);
                    Log.Information("Login successful for user: {Username}, Role: {Role}", 
                        _currentUser?.Username, _currentUser?.Role);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error saving or verifying token");
                    throw new UnauthorizedAccessException("Failed to save or verify authentication token", ex);
                }

                return authResponse;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login process failed for user: {Username} - Type: {ExceptionType}, Message: {Message}", 
                    loginModel.Username, ex.GetType().Name, ex.Message);
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception during login - Type: {ExceptionType}, Message: {Message}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                throw new UnauthorizedAccessException("Login failed due to an unexpected error", ex);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                Log.Information("Starting logout process for user: {Username}", _currentUser?.Username);
                await _tokenService.RemoveTokenAsync();
                _currentUser = null;
                Log.Information("Logout completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during logout for user: {Username}", _currentUser?.Username);
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception during logout");
                }
                throw;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                Log.Debug("Checking authentication status");
                var token = await _tokenService.GetTokenAsync();
                
                if (string.IsNullOrEmpty(token))
                {
                    Log.Debug("No token found during authentication check");
                    return false;
                }

                var isValid = _tokenService.IsTokenValid(token);
                Log.Debug("Authentication check result: IsAuthenticated={IsAuthenticated}, TokenLength={TokenLength}", 
                    isValid, token.Length);
                return isValid;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking authentication status");
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception during authentication check");
                }
                return false;
            }
        }

        public async Task<UserInfo> GetCurrentUserAsync()
        {
            try
            {
                Log.Debug("Getting current user info");
                if (_currentUser != null)
                {
                    Log.Debug("Returning cached user info: {Username}, Role: {Role}", 
                        _currentUser.Username, _currentUser.Role);
                    return _currentUser;
                }

                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Log.Error("No token found when getting current user");
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                if (!_tokenService.IsTokenValid(token))
                {
                    Log.Error("Invalid or expired token when getting current user");
                    await _tokenService.RemoveTokenAsync();
                    throw new UnauthorizedAccessException("Authentication token is invalid or expired");
                }

                _currentUser = ExtractUserInfoFromToken(token);
                if (_currentUser == null || string.IsNullOrEmpty(_currentUser.Username))
                {
                    Log.Error("Failed to extract valid user info from token");
                    throw new UnauthorizedAccessException("Invalid user information in token");
                }

                Log.Debug("Extracted user info from token: Username={Username}, Role: {Role}", 
                    _currentUser.Username, _currentUser.Role);
                return _currentUser;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting current user info");
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception while getting user info");
                }
                throw new UnauthorizedAccessException("Failed to get user information", ex);
            }
        }

        private UserInfo ExtractUserInfoFromToken(string token)
        {
            try
            {
                Log.Debug("Extracting user info from token: Length={TokenLength}", token.Length);
                var handler = new JwtSecurityTokenHandler();
                
                if (!handler.CanReadToken(token))
                {
                    Log.Warning("Token is not in a valid JWT format: Length={TokenLength}", token.Length);
                    return new UserInfo { Role = "User" };
                }

                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                if (jsonToken == null)
                {
                    Log.Warning("Could not parse token as JWT: Length={TokenLength}", token.Length);
                    return new UserInfo { Role = "User" };
                }

                var claims = jsonToken.Claims?.ToList() ?? new List<Claim>();
                Log.Debug("Token claims found: Count={ClaimCount}", claims.Count);
                
                foreach (var claim in claims)
                {
                    Log.Debug("Claim found - Type: {Type}, Value: {Value}", claim.Type, claim.Value);
                }

                var userInfo = new UserInfo
                {
                    UserId = claims.FirstOrDefault(q => q.Type.Equals(JwtRegisteredClaimNames.Sub))?.Value ?? string.Empty,
                    Username = claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Email))?.Value ?? string.Empty,
                    Role = claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Role))?.Value ?? "User"
                };

                Log.Debug("User info extracted - Username: {Username}, Role: {Role}", 
                    userInfo.Username, userInfo.Role);
                return userInfo;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error extracting user info from token: Length={TokenLength}", token.Length);
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception while extracting user info");
                }
                return new UserInfo { Role = "User" };
            }
        }
    }
} 