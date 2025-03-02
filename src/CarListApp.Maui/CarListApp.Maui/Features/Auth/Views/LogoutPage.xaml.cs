using CarListApp.Maui.Features.Auth.ViewModels;

namespace CarListApp.Maui.Features.Auth.Views;

public partial class LogoutPage : ContentPage
{
    public LogoutPage(LogoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 