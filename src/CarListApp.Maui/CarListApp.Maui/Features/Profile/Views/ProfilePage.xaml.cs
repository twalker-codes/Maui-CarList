using CarListApp.Maui.Features.Profile.ViewModels;

namespace CarListApp.Maui.Features.Profile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadUserInfoCommand.Execute(null);
    }
} 