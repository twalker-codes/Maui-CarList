using CarListApp.Maui.Core.Http;
using CarListApp.Maui.Features.Cars.Models;
using System.Net.Http.Json;
using Serilog;
using System.Net;

namespace CarListApp.Maui.Features.Cars.Services
{
    public class CarService : ICarService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private List<Car>? _cars;
        private DateTime _lastCacheUpdate;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5); // 5 minute cache expiration
        private const int MaxRetries = 3;

        public CarService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            Log.Information("CarService initialized with {CacheExpiration} minute cache expiration and {MaxRetries} retries", 
                _cacheExpiration.TotalMinutes, MaxRetries);
        }

        private async Task<T> RetryWithExponentialBackoff<T>(Func<Task<T>> operation, string operationName)
        {
            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    if (i > 0)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, i - 1)); // 1, 2, 4 seconds
                        Log.Debug("Retry attempt {Attempt} for {Operation}, waiting {Delay} seconds", 
                            i + 1, operationName, delay.TotalSeconds);
                        await Task.Delay(delay);
                    }

                    return await operation();
                }
                catch (UnauthorizedAccessException)
                {
                    Log.Warning("Unauthorized access during {Operation} on attempt {Attempt}", operationName, i + 1);
                    throw; // Don't retry auth failures
                }
                catch (Exception ex) when (i < MaxRetries - 1)
                {
                    Log.Warning(ex, "Attempt {Attempt} failed for {Operation}", i + 1, operationName);
                }
            }

            throw new Exception($"Operation {operationName} failed after {MaxRetries} attempts");
        }

        public async Task<List<Car>> GetCarsAsync()
        {
            try
            {
                Log.Debug("Getting cars from service");
                if (_cars != null && DateTime.UtcNow - _lastCacheUpdate < _cacheExpiration)
                {
                    Log.Debug("Returning cached cars, cache age: {CacheAge} minutes", (DateTime.UtcNow - _lastCacheUpdate).TotalMinutes);
                    return _cars;
                }

                Log.Debug("Cache expired or empty, syncing with API");
                await RetryWithExponentialBackoff(SyncWithApiAsync, "SyncWithApi");
                return _cars ?? new List<Car>();
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when getting cars");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting cars");
                return new List<Car>();
            }
        }

        public async Task<bool> SyncWithApiAsync()
        {
            Log.Debug("Syncing cars with API");
            var client = await _httpClientFactory.CreateClient();
            
            var response = await client.GetAsync("/cars");
            Log.Debug("API response status: {StatusCode}", response.StatusCode);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Log.Warning("Unauthorized access when syncing cars");
                throw new UnauthorizedAccessException("Not authorized to access cars");
            }

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("API returned unsuccessful status code: {StatusCode}", response.StatusCode);
                return false;
            }

            _cars = await response.Content.ReadFromJsonAsync<List<Car>>() ?? new List<Car>();
            _lastCacheUpdate = DateTime.UtcNow;
            Log.Information("Retrieved {Count} cars, cache updated at {LastUpdate}", _cars.Count, _lastCacheUpdate);
            return true;
        }

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            try
            {
                return await RetryWithExponentialBackoff(async () =>
                {
                    Log.Debug("Getting car with ID: {Id}", id);
                    var client = await _httpClientFactory.CreateClient();
                    
                    var response = await client.GetAsync($"/cars/{id}");
                    Log.Debug("API response status: {StatusCode}", response.StatusCode);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Warning("API returned unsuccessful status code: {StatusCode}", response.StatusCode);
                        return null;
                    }

                    var car = await response.Content.ReadFromJsonAsync<Car>();
                    Log.Information("Retrieved car with ID: {Id}", id);
                    return car;
                }, $"GetCarById_{id}");
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when getting car with ID: {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting car with ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> AddCarAsync(Car car)
        {
            try
            {
                return await RetryWithExponentialBackoff(async () =>
                {
                    Log.Debug("Adding new car");
                    var client = await _httpClientFactory.CreateClient();
                    
                    var response = await client.PostAsJsonAsync("/cars", car);
                    Log.Debug("API response status: {StatusCode}", response.StatusCode);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Warning("API returned unsuccessful status code: {StatusCode}", response.StatusCode);
                        return false;
                    }

                    _cars = null; // Clear cache to force refresh
                    Log.Information("Successfully added new car");
                    return true;
                }, "AddCar");
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when adding car");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding car");
                return false;
            }
        }

        public async Task<bool> UpdateCarAsync(int id, Car car)
        {
            try
            {
                return await RetryWithExponentialBackoff(async () =>
                {
                    Log.Debug("Updating car with ID: {Id}", id);
                    var client = await _httpClientFactory.CreateClient();
                    
                    var response = await client.PutAsJsonAsync($"/cars/{id}", car);
                    Log.Debug("API response status: {StatusCode}", response.StatusCode);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Warning("API returned unsuccessful status code: {StatusCode}", response.StatusCode);
                        return false;
                    }

                    _cars = null; // Clear cache to force refresh
                    Log.Information("Successfully updated car with ID: {Id}", id);
                    return true;
                }, $"UpdateCar_{id}");
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when updating car with ID: {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating car with ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            try
            {
                using var client = await _httpClientFactory.CreateClient();
                var response = await client.DeleteAsync($"/cars/{id}");

                // If car not found (404), consider it a success since the end goal is achieved
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Information("Car with ID {Id} not found on server (already deleted)", id);
                    _cars = null; // Clear cache to force refresh
                    _lastCacheUpdate = DateTime.MinValue; // Reset cache timestamp
                    return true;
                }

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("Car with ID {Id} successfully deleted from server", id);
                    _cars = null; // Clear cache to force refresh
                    _lastCacheUpdate = DateTime.MinValue; // Reset cache timestamp
                    return true;
                }

                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Failed to delete car with ID {Id}. Status code: {StatusCode}, Content: {Content}", 
                    id, response.StatusCode, content);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting car with ID {Id}", id);
                return false;
            }
        }
    }
} 