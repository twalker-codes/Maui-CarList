using CommunityToolkit.Mvvm.ComponentModel;

namespace CarListApp.Maui.ViewModels;

public partial class BaseViewModel: ObservableObject
{
    [ObservableProperty]
    string title = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    bool isLoading = false;
    public bool IsNotLoading => !IsLoading;
    
    
}