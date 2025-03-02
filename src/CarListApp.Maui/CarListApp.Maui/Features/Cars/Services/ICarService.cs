using CarListApp.Maui.Features.Cars.Models;

namespace CarListApp.Maui.Features.Cars.Services
{
    public interface ICarService
    {
        Task<List<Car>> GetCarsAsync();
        Task<Car?> GetCarByIdAsync(int id);
        Task<bool> AddCarAsync(Car car);
        Task<bool> UpdateCarAsync(int id, Car car);
        Task<bool> DeleteCarAsync(int id);
        Task<bool> SyncWithApiAsync();
    }
} 