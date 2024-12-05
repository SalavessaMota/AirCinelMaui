using AirCinelMaui.Services;
using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class MyAccountPage : ContentPage
{
    public MyAccountPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new MyAccountViewModel(apiService, Navigation);
    }
}
