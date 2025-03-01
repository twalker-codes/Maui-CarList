using CarListApp.Maui.Features.Auth.ViewModels;

namespace CarListApp.Maui.Features.Auth.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadUserInfoCommand.Execute(null);
    }

    private async void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        await _viewModel.ToggleDarkModeCommand.ExecuteAsync(null);
    }
} 