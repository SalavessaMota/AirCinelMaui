using AirCinelMaui.Services;
using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new LoginViewModel(apiService, Navigation);
    }
}
