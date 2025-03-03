using CarListApp.Maui.Features.Cars.Models;
using SQLite;
using Serilog;

namespace CarListApp.Maui.Features.Cars.Services
{
    public class CarDatabaseService
    {
        private readonly SQLiteConnection _connection;
        private const int BatchSize = 50;

        public CarDatabaseService(string dbPath)
        {
            _connection = new SQLiteConnection(dbPath);
            _connection.CreateTable<Car>();
            CreateIndexes();
            Log.Information("CarDatabaseService initialized with database at {DbPath}", dbPath);
        }

        private void CreateIndexes()
        {
            try
            {
                // Create indexes for commonly queried fields
                _connection.CreateIndex(nameof(Car), nameof(Car.Make));
                _connection.CreateIndex(nameof(Car), nameof(Car.Model));
                _connection.CreateIndex(nameof(Car), nameof(Car.Vin), unique: true);
                Log.Debug("Database indexes created successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating database indexes");
            }
        }

        public List<Car> GetCars()
        {
            try
            {
                return _connection.Table<Car>().ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting cars from local database");
                return new List<Car>();
            }
        }

        public Car? GetCar(int id)
        {
            try
            {
                return _connection.Table<Car>()
                    .FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting car with ID {Id} from local database", id);
                return null;
            }
        }

        public void UpdateCars(IEnumerable<Car> cars)
        {
            if (cars == null) return;

            try
            {
                _connection.RunInTransaction(() =>
                {
                    var carsList = cars.ToList();
                    for (int i = 0; i < carsList.Count; i += BatchSize)
                    {
                        var batch = carsList.Skip(i).Take(BatchSize);
                        foreach (var car in batch)
                        {
                            var existing = _connection.Find<Car>(car.Id);
                            if (existing != null)
                            {
                                _connection.Update(car);
                            }
                            else
                            {
                                _connection.Insert(car);
                            }
                        }
                    }
                });
                Log.Debug("Batch update completed for {Count} cars", cars.Count());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during batch update of cars");
                throw;
            }
        }

        public void AddCar(Car car)
        {
            try
            {
                _connection.Insert(car);
                Log.Debug("Added car with ID {Id} to local database", car.Id);
            }
            catch (SQLiteException ex) when (ex.Result == SQLite3.Result.Constraint)
            {
                // Handle unique constraint violation
                _connection.Update(car);
                Log.Debug("Updated existing car with ID {Id} in local database", car.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding/updating car with ID {Id} to local database", car.Id);
                throw;
            }
        }

        public void UpdateCar(Car car)
        {
            try
            {
                _connection.Update(car);
                Log.Debug("Updated car with ID {Id} in local database", car.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating car with ID {Id} in local database", car.Id);
                throw;
            }
        }

        public void DeleteCar(int id)
        {
            try
            {
                _connection.Delete<Car>(id);
                Log.Debug("Deleted car with ID {Id} from local database", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting car with ID {Id} from local database", id);
                throw;
            }
        }

        public void DeleteCars(IEnumerable<int> ids)
        {
            if (ids == null) return;

            try
            {
                _connection.RunInTransaction(() =>
                {
                    var idsList = ids.ToList();
                    for (int i = 0; i < idsList.Count; i += BatchSize)
                    {
                        var batch = idsList.Skip(i).Take(BatchSize);
                        foreach (var id in batch)
                        {
                            _connection.Delete<Car>(id);
                        }
                    }
                });
                Log.Debug("Batch delete completed for {Count} cars", ids.Count());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during batch delete of cars");
                throw;
            }
        }

        public void SyncWithServer(IEnumerable<Car> serverCars)
        {
            if (serverCars == null) return;

            try
            {
                _connection.RunInTransaction(() =>
                {
                    var localCars = _connection.Table<Car>().ToList();
                    var serverCarIds = serverCars.Select(c => c.Id).ToHashSet();
                    var localCarIds = localCars.Select(c => c.Id).ToHashSet();

                    // Delete cars that no longer exist on server
                    var carsToDelete = localCarIds.Except(serverCarIds);
                    foreach (var id in carsToDelete)
                    {
                        _connection.Delete<Car>(id);
                    }

                    // Update or insert server cars
                    foreach (var car in serverCars)
                    {
                        var existing = _connection.Find<Car>(car.Id);
                        if (existing != null)
                        {
                            _connection.Update(car);
                        }
                        else
                        {
                            _connection.Insert(car);
                        }
                    }
                });
                Log.Information("Local database synced with server data");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error syncing local database with server");
                throw;
            }
        }
    }
} 