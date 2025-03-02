using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Features.Cars.Models;
using CarListApp.Maui.Features.Cars.Services;
using CarListApp.Maui.Features.Shell.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Serilog;

namespace CarListApp.Maui.Features.Cars.ViewModels
{
    public partial class CarListViewModel : ObservableObject
    {
        private readonly ICarService _carService;
        private readonly INavigationService _navigationService;
        private readonly IShellNavigationService _shellNavigationService;

        public CarListViewModel(
            ICarService carService, 
            INavigationService navigationService,
            IShellNavigationService shellNavigationService)
        {
            _carService = carService;
            _navigationService = navigationService;
            _shellNavigationService = shellNavigationService;
            Cars = new ObservableCollection<Car>();
            Log.Information("CarListViewModel initialized");
        }

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private ObservableCollection<Car> cars = new();

        [RelayCommand]
        private async Task LoadCarsAsync()
        {
            if (Cars is null)
            {
                Cars = new ObservableCollection<Car>();
            }

            try
            {
                IsRefreshing = true;
                var cars = await _carService.GetCarsAsync();
                
                Cars.Clear();
                foreach (var car in cars)
                {
                    if (car is not null)
                    {
                        Cars.Add(car);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when loading cars, redirecting to login");
                await _shellNavigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading cars");
                if (Application.Current?.MainPage is not null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        "Unable to load cars. Please try again.", "OK");
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task AddNewCarAsync()
        {
            try
            {
                Log.Debug("Navigating to add new car page");
                await _navigationService.NavigateToAsync("car/edit");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error navigating to add new car page");
                if (Application.Current?.MainPage is not null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        "Unable to open add car page. Please try again.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task EditCarAsync(Car? car)
        {
            if (car is null)
            {
                Log.Warning("Attempted to edit null car");
                return;
            }

            try
            {
                Log.Debug("Navigating to edit car page for car ID: {Id}", car.Id);
                var parameters = new Dictionary<string, object>
                {
                    { "id", car.Id }
                };
                await _navigationService.GoToAsync("car/edit", parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error navigating to edit car page for car ID: {Id}", car.Id);
                if (Application.Current?.MainPage is not null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        "Unable to open edit car page. Please try again.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task DeleteCarAsync(Car? car)
        {
            if (car is null)
            {
                Log.Warning("Attempted to delete null car");
                return;
            }

            if (Application.Current?.MainPage is null)
            {
                Log.Warning("MainPage is null during delete operation");
                return;
            }

            try
            {
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Delete Car", 
                    $"Are you sure you want to delete {car.Make} {car.Model}?",
                    "Yes", "No");

                if (answer)
                {
                    Log.Debug("Attempting to delete car with ID: {Id}", car.Id);
                    bool success = await _carService.DeleteCarAsync(car.Id);
                    if (success)
                    {
                        Cars.Remove(car);
                        Log.Information("Successfully deleted car with ID: {Id}", car.Id);
                    }
                    else
                    {
                        Log.Warning("Failed to delete car with ID: {Id}", car.Id);
                        await Application.Current.MainPage.DisplayAlert("Error", 
                            "Unable to delete car. Please try again.", "OK");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when deleting car");
                await _shellNavigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during car deletion");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "An error occurred while deleting the car. Please try again.", "OK");
            }
        }
    }
} 