using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Features.Cars.Models;
using CarListApp.Maui.Features.Cars.Services;
using CarListApp.Maui.Features.Shell.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Dispatching;
using Serilog;
using Microsoft.Maui.Controls;

namespace CarListApp.Maui.Features.Cars.ViewModels
{
    public partial class CarListViewModel : ObservableObject, IDisposable
    {
        private readonly ICarService _carService;
        private readonly INavigationService _navigationService;
        private readonly IShellNavigationService _shellNavigationService;
        private readonly IDispatcher _dispatcher;
        private readonly CarDatabaseService _carDatabaseService;
        private CancellationTokenSource? _refreshCancellationTokenSource;
        private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
        private const int RefreshDebounceMs = 500;
        private const int PageSize = 20;
        private bool _isDisposed;
        private int _currentPage = 1;
        private readonly Dictionary<string, Func<Car, object>> _sortOptions;
        private CancellationTokenSource? _searchCancellationTokenSource;
        private bool _suppressCollectionChangedNotifications = false;

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
            _dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException("Dispatcher not available");
            
            Cars = new ObservableCollection<Car>();
            Cars.CollectionChanged += OnCarsCollectionChanged;
            
            // Initialize sort options
            _sortOptions = new Dictionary<string, Func<Car, object>>
            {
                { "Make (A-Z)", c => c.Make ?? "" },
                { "Make (Z-A)", c => c.Make ?? "" },
                { "Model (A-Z)", c => c.Model ?? "" },
                { "Model (Z-A)", c => c.Model ?? "" },
                { "VIN", c => c.Vin ?? "" }
            };
            SortOptions = _sortOptions.Keys.ToList();
            
            Log.Information("CarListViewModel initialized");
        }

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string loadingMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Car> cars = new();

        [ObservableProperty]
        private bool isOperationInProgress;

        [ObservableProperty]
        private bool hasMoreItems;

        [ObservableProperty]
        private bool isLoadingMore;

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private string selectedSortOption = string.Empty;

        [ObservableProperty]
        private bool isOffline;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private int filteredCount;

        public List<string> SortOptions { get; }

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

        private void OnCarsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_suppressCollectionChangedNotifications)
            {
                Log.Debug("Collection change notifications suppressed during bulk operation");
                return;
            }

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
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

                        // Update all UI properties
                        OnPropertyChanged(nameof(IsEmpty));
                        OnPropertyChanged(nameof(TotalCount));
                        
                        var newFilteredCount = GetFilteredCars().Count();
                        if (filteredCount != newFilteredCount)
                        {
                            Log.Debug("Updating FilteredCount from {Old} to {New}", filteredCount, newFilteredCount);
                            FilteredCount = newFilteredCount;
                        }
                        
                        Log.Debug("UI properties updated - IsEmpty: {IsEmpty}, TotalCount: {Total}, FilteredCount: {Filtered}", 
                            IsEmpty, TotalCount, FilteredCount);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error updating UI properties on main thread");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling collection change");
            }
        }

        public bool IsEmpty => !Cars.Any();
        public int TotalCount => Cars.Count;

        private IEnumerable<Car> FilteredCars
        {
            get
            {
                if (Cars == null) return Enumerable.Empty<Car>();
                
                var query = Cars.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var search = SearchQuery.Trim().ToLowerInvariant();
                    query = query.Where(c => c != null && (
                        (c.Make?.ToLowerInvariant().Contains(search) ?? false) ||
                        (c.Model?.ToLowerInvariant().Contains(search) ?? false) ||
                        (c.Vin?.ToLowerInvariant().Contains(search) ?? false)));
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(SelectedSortOption) && _sortOptions.TryGetValue(SelectedSortOption, out var sortSelector))
                {
                    query = SelectedSortOption.EndsWith("(Z-A)") 
                        ? query.OrderByDescending(sortSelector)
                        : query.OrderBy(sortSelector);
                }

                return query;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            SearchDebounced().FireAndForget();
        }

        partial void OnSelectedSortOptionChanged(string value)
        {
            ApplyFiltersAndSort();
        }

        private async Task SearchDebounced()
        {
            try
            {
                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();
                var token = _searchCancellationTokenSource.Token;

                await Task.Delay(300, token); // Debounce search for 300ms
                ApplyFiltersAndSort();
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled by a new search
            }
        }

        private void ApplyFiltersAndSort()
        {
            if (_isDisposed) return;

            try
            {
                var filtered = FilteredCars.ToList();
                _dispatcher.DispatchAsync(() =>
                {
                    try
                    {
                        _suppressCollectionChangedNotifications = true;
                        Cars.Clear();
                        foreach (var car in filtered)
                        {
                            Cars.Add(car);
                        }
                    }
                    finally
                    {
                        _suppressCollectionChangedNotifications = false;
                        // Manually trigger a reset notification since we suppressed individual ones
                        OnCarsCollectionChanged(Cars, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying filters and sort");
                ErrorMessage = "Error applying filters";
            }
        }

        [RelayCommand]
        private async Task RetryLastOperationAsync()
        {
            ErrorMessage = string.Empty;
            await LoadCarsAsync();
        }

        [RelayCommand]
        private async Task LoadCarsAsync()
        {
            if (IsOperationInProgress)
            {
                Log.Debug("Operation already in progress, skipping load");
                return;
            }

            _currentPage = 1; // Reset pagination on refresh
            await LoadCarsInternalAsync(true);
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (IsLoadingMore || !HasMoreItems)
            {
                return;
            }

            _currentPage++;
            await LoadCarsInternalAsync(false);
        }

        private async Task LoadCarsInternalAsync(bool isRefresh)
        {
            // Cancel any pending refresh
            _refreshCancellationTokenSource?.Cancel();
            _refreshCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _refreshCancellationTokenSource.Token;

            try
            {
                // Acquire semaphore with timeout
                if (!await _refreshSemaphore.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
                {
                    Log.Warning("Failed to acquire refresh semaphore, operation timed out");
                    return;
                }

                IsOperationInProgress = true;
                IsRefreshing = isRefresh;
                IsLoadingMore = !isRefresh;
                LoadingMessage = isRefresh ? "Loading cars..." : "Loading more cars...";
                ErrorMessage = string.Empty;

                if (isRefresh)
                {
                    await Task.Delay(RefreshDebounceMs, cancellationToken);
                }

                // Ensure Cars collection is initialized
                if (Cars is null)
                {
                    await _dispatcher.DispatchAsync(() => Cars = new ObservableCollection<Car>());
                }

                List<Car> updatedCars;
                try
                {
                    // Try to get cars from API
                    updatedCars = (await _carService.GetCarsAsync()) ?? new List<Car>();
                    IsOffline = false;

                    // Update local cache
                    await Task.Run(() =>
                    {
                        foreach (var car in updatedCars)
                        {
                            _carDatabaseService.AddCar(car);
                        }
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to fetch cars from API, falling back to local cache");
                    // Fall back to local cache
                    updatedCars = _carDatabaseService.GetCars();
                    IsOffline = true;
                }

                var pagedCars = (updatedCars ?? Enumerable.Empty<Car>())
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                HasMoreItems = (updatedCars?.Count ?? 0) > _currentPage * PageSize;
                
                await _dispatcher.DispatchAsync(() =>
                {
                    try
                    {
                        if (isRefresh)
                        {
                            Cars?.Clear();
                        }

                        // Create sets for efficient lookup, with null checks
                        var currentIds = new HashSet<int>(Cars?.Select(c => c.Id) ?? Enumerable.Empty<int>());
                        var updatedIds = new HashSet<int>(pagedCars?.Where(c => c != null).Select(c => c.Id) ?? Enumerable.Empty<int>());

                        // Prepare batch updates with null checks
                        var cars = Cars ?? new ObservableCollection<Car>();
                        var toRemove = cars.Where(c => !updatedIds.Contains(c.Id)).ToList();
                        var toUpdate = new List<(int Index, Car Car)>();
                        var toAdd = new List<Car>();

                        // Identify changes
                        if (pagedCars != null)
                        {
                            foreach (var car in pagedCars.Where(c => c != null))
                            {
                                var existingCar = cars.FirstOrDefault(c => c.Id == car.Id);
                                if (existingCar != null)
                                {
                                    if (!AreCarPropertiesEqual(existingCar, car))
                                    {
                                        var index = cars.IndexOf(existingCar);
                                        toUpdate.Add((index, car));
                                    }
                                }
                                else
                                {
                                    toAdd.Add(car);
                                }
                            }
                        }

                        // Apply batch updates with null checks
                        foreach (var car in toRemove)
                        {
                            cars.Remove(car);
                            Log.Debug("Removed car {Id} from collection", car.Id);
                        }

                        foreach (var (index, car) in toUpdate)
                        {
                            cars[index] = car;
                            Log.Debug("Updated car {Id} in collection", car.Id);
                        }

                        foreach (var car in toAdd.OrderBy(c => c.Id))
                        {
                            cars.Add(car);
                            Log.Debug("Added new car {Id} to collection", car.Id);
                        }

                        // Update the main collection if needed
                        if (Cars != cars)
                        {
                            Cars = cars;
                        }

                        // Apply current filters and sort
                        ApplyFiltersAndSort();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error updating collection");
                        throw;
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Log.Information("Car loading operation was cancelled");
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when loading cars, redirecting to login");
                ErrorMessage = "Your session has expired. Please log in again.";
                await _shellNavigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading cars");
                ErrorMessage = "Unable to load cars. Please try again.";
            }
            finally
            {
                IsRefreshing = false;
                IsOperationInProgress = false;
                IsLoadingMore = false;
                LoadingMessage = string.Empty;
                _refreshSemaphore.Release();
            }
        }

        private bool AreCarPropertiesEqual(Car car1, Car car2)
        {
            return car1.Make == car2.Make &&
                   car1.Model == car2.Model &&
                   car1.Vin == car2.Vin;
        }

        [RelayCommand]
        private async Task AddNewCarAsync()
        {
            if (IsOperationInProgress)
            {
                Log.Debug("Operation already in progress, skipping add");
                return;
            }

            try
            {
                IsOperationInProgress = true;
                LoadingMessage = "Opening add car page...";
                
                Log.Debug("Navigating to add new car page");
                await _navigationService.NavigateToAsync("car/edit");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error navigating to add new car page");
                await ShowErrorAlert("Unable to open add car page. Please try again.");
            }
            finally
            {
                IsOperationInProgress = false;
                LoadingMessage = string.Empty;
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

            if (IsOperationInProgress)
            {
                Log.Debug("Operation already in progress, skipping edit");
                return;
            }

            try
            {
                IsOperationInProgress = true;
                LoadingMessage = "Opening edit car page...";
                
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
                await ShowErrorAlert("Unable to open edit car page. Please try again.");
            }
            finally
            {
                IsOperationInProgress = false;
                LoadingMessage = string.Empty;
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

            if (IsOperationInProgress)
            {
                Log.Debug("Operation already in progress, skipping delete");
                return;
            }

            if (Application.Current?.MainPage is null)
            {
                Log.Warning("Cannot delete car - MainPage is null");
                return;
            }

            try
            {
                IsOperationInProgress = true;
                LoadingMessage = "Confirming deletion...";
                ErrorMessage = string.Empty;

                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Delete Car", 
                    $"Are you sure you want to delete {car.Make} {car.Model}?",
                    "Yes", "No");

                if (!answer) return;

                LoadingMessage = "Deleting car...";
                Log.Debug("Attempting to delete car with ID: {Id}", car.Id);

                bool success = await _carService.DeleteCarAsync(car.Id);
                
                if (success)
                {
                    try
                    {
                        // Remove from local database
                        _carDatabaseService.DeleteCar(car.Id);
                        
                        // Remove from UI collection immediately
                        await _dispatcher.DispatchAsync(() => 
                        {
                            var carToRemove = Cars.FirstOrDefault(c => c.Id == car.Id);
                            if (carToRemove != null)
                            {
                                Cars.Remove(carToRemove);
                                Log.Debug("Removed car {Id} from UI collection", car.Id);
                            }
                        });

                        // Clear any cached data and reload from server
                        await Task.Delay(100); // Small delay to ensure server-side deletion is complete
                        await LoadCarsInternalAsync(true);
                        
                        Log.Information("Successfully deleted car with ID: {Id}", car.Id);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error updating local state after successful server deletion");
                        // Force a complete refresh
                        await LoadCarsInternalAsync(true);
                    }
                }
                else
                {
                    Log.Warning("Failed to delete car with ID: {Id}", car.Id);
                    ErrorMessage = "Unable to delete car. Please try again.";
                    await ShowErrorAlert("Unable to delete car. Please check your connection and try again.");
                    // Refresh to ensure UI is in sync
                    await LoadCarsInternalAsync(true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized access when deleting car");
                ErrorMessage = "Your session has expired. Please log in again.";
                await _shellNavigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during car deletion");
                ErrorMessage = "Unable to delete car. Please try again.";
                await ShowErrorAlert("An error occurred while deleting the car. Please try again.");
                // Refresh to ensure UI is in sync
                await LoadCarsInternalAsync(true);
            }
            finally
            {
                IsOperationInProgress = false;
                LoadingMessage = string.Empty;
            }
        }

        private async Task ShowErrorAlert(string message)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
                }
                else
                {
                    Log.Warning("Cannot show error alert - MainPage is null");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing alert: {Message}", message);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            
            _isDisposed = true;
            _refreshCancellationTokenSource?.Cancel();
            _refreshCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _refreshSemaphore.Dispose();
            
            if (Cars != null)
            {
                Cars.CollectionChanged -= OnCarsCollectionChanged;
            }
            
            GC.SuppressFinalize(this);
        }

        ~CarListViewModel()
        {
            Dispose();
        }
    }

    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(
                t => Log.Error(t.Exception, "Error in FireAndForget task"),
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
} 