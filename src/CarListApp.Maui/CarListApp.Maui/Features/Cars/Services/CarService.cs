using CarListApp.Maui.Core.Http;
using CarListApp.Maui.Features.Cars.Models;
using System.Net.Http.Json;
using Serilog;

namespace CarListApp.Maui.Features.Cars.Services
{
    public class CarService : ICarService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private List<Car>? _cars;

        public CarService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            Log.Information("CarService initialized");
        }

        public async Task<List<Car>> GetCarsAsync()
        {
            try
            {
                Log.Debug("Getting cars from service");
                if (_cars != null)
                {
                    Log.Debug("Returning cached cars");
                    return _cars;
                }

                Log.Debug("No cars in memory, syncing with API");
                await SyncWithApiAsync();
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
            try
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
                Log.Information("Retrieved {Count} cars", _cars.Count);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when syncing cars");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error syncing cars with API");
                return false;
            }
        }

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            try
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
                Log.Debug("Deleting car with ID: {Id}", id);
                var client = await _httpClientFactory.CreateClient();
                
                var response = await client.DeleteAsync($"/cars/{id}");
                Log.Debug("API response status: {StatusCode}", response.StatusCode);
                
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("API returned unsuccessful status code: {StatusCode}", response.StatusCode);
                    return false;
                }

                _cars = null; // Clear cache to force refresh
                Log.Information("Successfully deleted car with ID: {Id}", id);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when deleting car with ID: {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting car with ID: {Id}", id);
                return false;
            }
        }
    }
} 