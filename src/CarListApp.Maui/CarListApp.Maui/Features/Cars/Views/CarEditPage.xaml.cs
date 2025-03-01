using CarListApp.Maui.Features.Cars.ViewModels;

namespace CarListApp.Maui.Features.Cars.Views;

public partial class CarEditPage : ContentPage
{
    private readonly CarEditViewModel _viewModel;

    public CarEditPage(CarEditViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
} 