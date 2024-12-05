using AirCinelMaui.Services;
using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class UserTicketsPage : ContentPage
{
    public UserTicketsPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new UserTicketsViewModel(apiService, Navigation);
    }
}
