using AirCinelMaui.Services;
using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class AvailableFlightsPage : ContentPage
{
    public AvailableFlightsPage(ApiService apiService)
    {
        InitializeComponent();
        var viewModel = new AvailableFlightsViewModel(apiService, Navigation);
        BindingContext = viewModel;
    }
}
