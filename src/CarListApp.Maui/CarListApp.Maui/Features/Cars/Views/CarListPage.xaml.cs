using CarListApp.Maui.Features.Cars.ViewModels;

namespace CarListApp.Maui.Features.Cars.Views;

public partial class CarListPage : ContentPage
{
    private readonly CarListViewModel _viewModel;

    public CarListPage(CarListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCarsCommand.ExecuteAsync(null);
    }
} 