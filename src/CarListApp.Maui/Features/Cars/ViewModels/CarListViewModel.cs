using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CarListApp.Maui.Features.Cars.Models;
using CarListApp.Maui.Features.Cars.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Serilog;
using System.Collections.Specialized;

namespace CarListApp.Maui.Features.Cars.ViewModels
{
    public partial class CarListViewModel : ObservableObject, IDisposable
    {
        private readonly ICarService _carService;
        private readonly INavigationService _navigationService;
        private readonly IShellNavigationService _shellNavigationService;
        private readonly ICarDatabaseService _carDatabaseService;
        private readonly Dictionary<string, Func<Car, object>> _sortOptions;
        private readonly int PageSize = 10;
        private int _currentPage = 1;
        private bool _suppressCollectionChangedNotifications;

        [ObservableProperty]
        private int filteredCount;

        [ObservableProperty]
        private ObservableCollection<Car> cars = new();

        public List<string> SortOptions { get; }
        public bool HasMoreItems { get; private set; }

        [ObservableProperty]
        private string selectedSortOption;

        [ObservableProperty]
        private string searchQuery;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isOffline;

        public CarListViewModel(
            ICarService carService, 
            INavigationService navigationService,
            IShellNavigationService shellNavigationService,
            CarDatabaseService carDatabaseService)
        {
            _carService = carService;
            _navigationService = navigationService;
            _shellNavigationService = shellNavigationService;
            _carDatabaseService = carDatabaseService;

            // Initialize the Cars collection and subscribe to changes
            Cars.CollectionChanged += OnCarsCollectionChanged;
            
            // Initialize sort options
            _sortOptions = new Dictionary<string, Func<Car, object>>
            {
                { "Make (A-Z)", c => c?.Make ?? string.Empty },
                { "Make (Z-A)", c => c?.Make ?? string.Empty },
                { "Model (A-Z)", c => c?.Model ?? string.Empty },
                { "Model (Z-A)", c => c?.Model ?? string.Empty },
                { "Year (Newest)", c => c?.Year ?? 0 },
                { "Year (Oldest)", c => c?.Year ?? 0 },
                { "VIN", c => c?.Vin ?? string.Empty }
            };

            SortOptions = _sortOptions.Keys.ToList();
            SelectedSortOption = SortOptions.FirstOrDefault();
            
            // Set initial count
            FilteredCount = Cars.Count;
            Log.Debug("Initial FilteredCount set to {Count}", FilteredCount);
        }

        private IEnumerable<Car> GetFilteredCars()
        {
            var query = Cars.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var search = SearchQuery.Trim().ToLowerInvariant();
                query = query.Where(c => c != null && (
                    (c.Make?.ToLowerInvariant().Contains(search) ?? false) ||
                    (c.Model?.ToLowerInvariant().Contains(search) ?? false) ||
                    (c.Vin?.ToLowerInvariant().Contains(search) ?? false)));
            }
            return query;
        }

        private void UpdateCounts()
        {
            try
            {
                var totalCount = Cars.Count;
                var filteredCount = GetFilteredCars().Count();
                
                Log.Debug("Calculating counts - Total: {Total}, Filtered: {Filtered}", totalCount, filteredCount);
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (FilteredCount != filteredCount)
                    {
                        Log.Debug("Updating FilteredCount from {Old} to {New}", FilteredCount, filteredCount);
                        FilteredCount = filteredCount;
                    }
                    
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(TotalCount));
                    OnPropertyChanged(nameof(FilteredCount));
                    
                    Log.Debug("UI properties updated - IsEmpty: {IsEmpty}, TotalCount: {Total}, FilteredCount: {Filtered}", 
                        IsEmpty, TotalCount, FilteredCount);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating counts");
            }
        }

        private void OnCarsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_suppressCollectionChangedNotifications)
            {
                Log.Debug("Collection change notifications suppressed during bulk operation");
                return;
            }

            try
            {
                Log.Debug("Collection changed. Action: {Action}", e.Action);
                
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Log.Debug("Items added: {Count}", e.NewItems?.Count ?? 0);
                        foreach (Car? item in e.NewItems ?? Array.Empty<object>())
                        {
                            Log.Debug("Collection change - Added: Make={Make}, Model={Model}, ID={Id}", 
                                item?.Make, item?.Model, item?.Id);
                        }
                        break;
                        
                    case NotifyCollectionChangedAction.Remove:
                        Log.Debug("Items removed: {Count}", e.OldItems?.Count ?? 0);
                        foreach (Car? item in e.OldItems ?? Array.Empty<object>())
                        {
                            Log.Debug("Collection change - Removed: Make={Make}, Model={Model}, ID={Id}", 
                                item?.Make, item?.Model, item?.Id);
                        }
                        break;
                        
                    case NotifyCollectionChangedAction.Reset:
                        Log.Debug("Collection reset triggered. Current count: {Count}", Cars.Count);
                        break;
                }

                // Update counts after collection changes
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Log.Debug("Updating UI properties after collection change");
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(TotalCount));
                    
                    var newFilteredCount = GetFilteredCars().Count();
                    if (FilteredCount != newFilteredCount)
                    {
                        Log.Debug("Updating FilteredCount from {Old} to {New}", FilteredCount, newFilteredCount);
                        FilteredCount = newFilteredCount;
                    }
                    
                    Log.Debug("UI properties updated - IsEmpty: {IsEmpty}, TotalCount: {Total}, FilteredCount: {Filtered}", 
                        IsEmpty, TotalCount, FilteredCount);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling collection change");
            }
        }

        private void ApplySort()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || !_sortOptions.ContainsKey(SelectedSortOption))
                return;

            var sortFunc = _sortOptions[SelectedSortOption];
            var isDescending = SelectedSortOption.Contains("Z-A") || SelectedSortOption.Contains("Newest");
            
            var sorted = isDescending 
                ? Cars.OrderByDescending(sortFunc).ToList() 
                : Cars.OrderBy(sortFunc).ToList();

            Cars.Clear();
            foreach (var car in sorted)
            {
                Cars.Add(car);
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            try
            {
                Log.Debug("Search query changed to: {Query}", value);
                UpdateCounts();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling search query change");
            }
        }

        partial void OnSelectedSortOptionChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            ApplySort();
        }

        [RelayCommand]
        private async Task ShowSortOptionsAsync()
        {
            if (Application.Current?.MainPage == null) return;

            try
            {
                var result = await Application.Current.MainPage.DisplayActionSheet(
                    "Sort by",
                    "Cancel",
                    null,
                    SortOptions.ToArray());

                if (!string.IsNullOrEmpty(result) && result != "Cancel")
                {
                    SelectedSortOption = result;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing sort options");
                ErrorMessage = "Unable to show sort options";
            }
        }

        [RelayCommand]
        private async Task LoadCarsAsync()
        {
            if (IsLoading)
            {
                Log.Debug("Loading operation already in progress, skipping");
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                var cars = await _carService.GetCarsAsync();
                Log.Debug("Loaded {Count} cars from service", cars.Count);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        // Create a new collection for the update
                        var newCollection = new ObservableCollection<Car>();
                        
                        // Add cars to the new collection
                        foreach (var car in cars)
                        {
                            newCollection.Add(car);
                            Log.Debug("Added car to new collection: Make={Make}, Model={Model}, ID={Id}", 
                                car.Make, car.Model, car.Id);
                        }
                        
                        Log.Debug("New collection populated. Count: {Count}", newCollection.Count);
                        
                        // Apply sort if needed
                        if (!string.IsNullOrEmpty(SelectedSortOption))
                        {
                            Log.Debug("Applying sort: {SortOption}", SelectedSortOption);
                            var sortFunc = _sortOptions[SelectedSortOption];
                            var isDescending = SelectedSortOption.Contains("Z-A") || SelectedSortOption.Contains("Newest");
                            var sorted = isDescending 
                                ? newCollection.OrderByDescending(sortFunc).ToList() 
                                : newCollection.OrderBy(sortFunc).ToList();
                            
                            newCollection.Clear();
                            foreach (var car in sorted)
                            {
                                newCollection.Add(car);
                            }
                        }

                        // Unsubscribe from old collection
                        if (Cars != null)
                        {
                            Cars.CollectionChanged -= OnCarsCollectionChanged;
                        }

                        // Subscribe to new collection before assignment
                        newCollection.CollectionChanged += OnCarsCollectionChanged;
                        
                        // Update the main collection
                        Log.Debug("Updating main collection");
                        Cars = newCollection;
                        
                        // Manually trigger collection reset and update counts
                        Log.Debug("Triggering collection reset notification and updating counts");
                        OnCarsCollectionChanged(Cars, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        
                        // Update counts
                        var totalCount = Cars.Count;
                        Log.Debug("Collection refresh completed - TotalCount: {Total}, FilteredCount: {Filtered}", 
                            TotalCount, FilteredCount);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error updating collection on main thread");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading cars");
                ErrorMessage = "Unable to load cars";
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task DeleteCarAsync(Car? car)
        {
            if (car == null || IsLoading) return;

            try
            {
                IsLoading = true;
                if (await _carService.DeleteCarAsync(car.Id))
                {
                    Log.Debug("Deleting car with ID {Id}", car.Id);
                    Cars.Remove(car);
                    Log.Debug("Car deleted. New count: {Count}", Cars.Count);
                }
                else
                {
                    ErrorMessage = "Failed to delete car";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting car");
                ErrorMessage = "Unable to delete car";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddNewCarAsync()
        {
            if (IsLoading) return;

            try
            {
                var result = await _navigationService.NavigateToCarEditAsync(null);
                if (result is Car newCar)
                {
                    Log.Debug("Adding new car");
                    Cars.Add(newCar);
                    Log.Debug("New car added. New count: {Count}", Cars.Count);
                    ApplySort();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding new car");
                ErrorMessage = "Unable to add new car";
            }
        }

        public bool IsEmpty => !Cars.Any();
        public int TotalCount => Cars.Count;
        public int FilteredCount
        {
            get => filteredCount;
            private set
            {
                if (SetProperty(ref filteredCount, value))
                {
                    Log.Debug("FilteredCount updated to {Count}", value);
                }
            }
        }
    }

    // Add extension method for ObservableCollection
    public class CollectionChangeNotificationSuspender : IDisposable
    {
        private readonly ObservableCollection<Car> _collection;
        private readonly NotifyCollectionChangedEventHandler? _handler;
        
        public CollectionChangeNotificationSuspender(ObservableCollection<Car> collection)
        {
            _collection = collection;
            // Store and remove the event handler
            _handler = _collection.CollectionChanged?.GetInvocationList().FirstOrDefault() as NotifyCollectionChangedEventHandler;
            if (_handler != null)
            {
                _collection.CollectionChanged -= _handler;
            }
        }
        
        public void Dispose()
        {
            // Restore the event handler and force a reset notification
            if (_handler != null)
            {
                _collection.CollectionChanged += _handler;
                _collection.CollectionChanged?.Invoke(_collection, 
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }

    public static class ObservableCollectionExtensions
    {
        public static IDisposable SuspendCollectionChangeNotification(this ObservableCollection<Car> collection)
        {
            return new CollectionChangeNotificationSuspender(collection);
        }
    }
} 