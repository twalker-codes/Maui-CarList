using CarListApp.Maui.Features.Auth.ViewModels;

namespace CarListApp.Maui.Features.Auth.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 