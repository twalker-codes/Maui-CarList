using CarListApp.Maui.Features.Cars.Models;
using SQLite;

namespace CarListApp.Maui.Features.Cars.Services
{
    public class CarDatabaseService
    {
        private SQLiteConnection _connection;

        public CarDatabaseService(string dbPath)
        {
            _connection = new SQLiteConnection(dbPath);
            _connection.CreateTable<Car>();
        }

        public List<Car> GetCars()
        {
            return _connection.Table<Car>().ToList();
        }

        public Car GetCar(int id)
        {
            return _connection.Table<Car>()
                .FirstOrDefault(c => c.Id == id);
        }

        public void AddCar(Car car)
        {
            _connection.Insert(car);
        }

        public void UpdateCar(Car car)
        {
            _connection.Update(car);
        }

        public void DeleteCar(int id)
        {
            _connection.Delete<Car>(id);
        }
    }
} 