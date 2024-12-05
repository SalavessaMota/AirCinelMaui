using AirCinelMaui.Services;
using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new RegisterViewModel(apiService, Navigation);
    }
}
