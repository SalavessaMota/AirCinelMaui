using AirCinelMaui.Services;
using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel(apiService, Navigation);
    }
}
