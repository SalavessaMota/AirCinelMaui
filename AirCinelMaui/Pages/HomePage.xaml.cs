using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _apiService;

    public HomePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Title = "Main Menu";

        var token = Preferences.Get("AuthToken", string.Empty);
        PastFlightsButton.IsVisible = !string.IsNullOrEmpty(token);
        YourTicketsButton.IsVisible = !string.IsNullOrEmpty(token);
        LoginButton.IsVisible = string.IsNullOrEmpty(token);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AvailableFlightsPage(_apiService));
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FlightHistoryPage(_apiService));
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserTicketsPage(_apiService));
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutPage());
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_apiService));
    }
}