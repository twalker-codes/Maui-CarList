using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Features.Cars.Models;
using CarListApp.Maui.Features.Cars.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace CarListApp.Maui.Features.Cars.ViewModels
{
    [QueryProperty(nameof(CarId), "id")]
    public partial class CarEditViewModel : ObservableObject
    {
        private readonly ICarService _carService;
        private readonly INavigationService _navigationService;

        public CarEditViewModel(ICarService carService, INavigationService navigationService)
        {
            _carService = carService;
            _navigationService = navigationService;
            Log.Information("CarEditViewModel initialized");
        }

        [ObservableProperty]
        private int carId;

        [ObservableProperty]
        private string make = string.Empty;

        [ObservableProperty]
        private string model = string.Empty;

        [ObservableProperty]
        private string vin = string.Empty;

        [ObservableProperty]
        private bool isNew = true;

        public async Task InitializeAsync()
        {
            try
            {
                if (CarId > 0)
                {
                    IsNew = false;
                    Log.Debug("Loading car details for ID: {Id}", CarId);
                    var car = await _carService.GetCarByIdAsync(CarId);
                    if (car != null)
                    {
                        Make = car.Make;
                        Model = car.Model;
                        Vin = car.Vin;
                        Log.Information("Successfully loaded car details for ID: {Id}", CarId);
                    }
                    else
                    {
                        Log.Warning("Car not found with ID: {Id}", CarId);
                        await ShowError("Car not found. Please try again.");
                        await _navigationService.NavigateBackAsync();
                    }
                }
                else
                {
                    Log.Debug("Initializing new car form");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when loading car details");
                await ShowError("You are not authorized to view this car.");
                await _navigationService.NavigateBackAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading car details");
                await ShowError("An error occurred while loading the car details.");
                await _navigationService.NavigateBackAsync();
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Make) || string.IsNullOrWhiteSpace(Model) || string.IsNullOrWhiteSpace(Vin))
                {
                    Log.Warning("Validation failed: Empty fields detected");
                    await ShowError("Please fill in all fields");
                    return;
                }

                var car = new Car
                {
                    Id = CarId,
                    Make = Make,
                    Model = Model,
                    Vin = Vin
                };

                Log.Debug("Saving car: {Make} {Model}", Make, Model);
                bool success;
                if (IsNew)
                {
                    success = await _carService.AddCarAsync(car);
                }
                else
                {
                    success = await _carService.UpdateCarAsync(CarId, car);
                }

                if (success)
                {
                    Log.Information("Successfully saved car: {Make} {Model}", Make, Model);
                    await _navigationService.NavigateBackAsync();
                }
                else
                {
                    Log.Warning("Failed to save car: {Make} {Model}", Make, Model);
                    await ShowError("Unable to save car. Please try again.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when saving car");
                await ShowError("You are not authorized to save this car.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving car");
                await ShowError("An error occurred while saving the car.");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            try
            {
                Log.Debug("Canceling car edit");
                await _navigationService.NavigateBackAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error navigating back from car edit");
                if (Application.Current?.MainPage is not null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        "Unable to return to the previous page. Please try again.", "OK");
                }
            }
        }

        private async Task ShowError(string message)
        {
            if (Application.Current?.MainPage is not null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
        }
    }
} 