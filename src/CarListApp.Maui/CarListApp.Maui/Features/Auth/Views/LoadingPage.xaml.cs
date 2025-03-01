using CarListApp.Maui.Features.Auth.ViewModels;

namespace CarListApp.Maui.Features.Auth.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage(LoadingPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 