using CarListApp.Maui.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace CarListApp.Maui.Services
{
    public class CarApiService
    {
        HttpClient _httpClient;
        public static string BaseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5001" : "http://localhost:5001";
        public string StatusMessage;

        public CarApiService()
        {
            _httpClient = new() { BaseAddress = new Uri(BaseAddress) };
        }

        public async Task <List<Car>> GetCars()
        {
            try
            {
                await SetAuthToken();
                var response = await _httpClient.GetStringAsync("/cars");
                return JsonConvert.DeserializeObject<List<Car>>(response);
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to retrieve data.";
            }

            return null;
        }

        public async Task<Car> GetCar(int id)
        {
            try
            {
                var response = await _httpClient.GetStringAsync("/cars/" + id);
                return JsonConvert.DeserializeObject<Car>(response);
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to retrieve data.";
            }

            return null;
        }

        public async Task AddCar(Car car)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/cars/", car);
                response.EnsureSuccessStatusCode();
                StatusMessage = "Insert Successful";
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to add data.";
            }
        }

        public async Task DeleteCar(int id)
        {
            try
            {

                var response = await _httpClient.DeleteAsync("/cars/" + id);
                response.EnsureSuccessStatusCode();
                StatusMessage = "Delete Successful";
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to delete data.";
            }
        }

        public async Task UpdateCar(int id, Car car)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("/cars/" + id, car);
                response.EnsureSuccessStatusCode();
                StatusMessage = "Update Successful";
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to update data.";
            }
        }

        public async Task<AuthResponse> Login(LoginModel loginModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/login", loginModel);
                response.EnsureSuccessStatusCode();
                StatusMessage = "Login Successful";

                return JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to login successfully.";
                return new AuthResponse();
            }
        }

        public async Task SetAuthToken()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception)
            {
                // If we can't get the token, we'll just make unauthenticated requests
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
