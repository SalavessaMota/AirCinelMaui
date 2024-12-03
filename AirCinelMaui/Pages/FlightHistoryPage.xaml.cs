using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class FlightHistoryPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public FlightHistoryPage(ApiService apiService)
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var token = Preferences.Get("AuthToken", string.Empty);

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "You must be logged in to view flight history.", "OK");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/flights/history");
            if (response.IsSuccessStatusCode)
            {
                var flightHistory = await response.Content.ReadFromJsonAsync<List<FlightDto>>();

                HistoryStackLayout.Children.Clear();
                foreach (var flight in flightHistory)
                {
                    var flightFrame = new Frame
                    {
                        BorderColor = Colors.Gray,
                        CornerRadius = 8,
                        Padding = 10,
                        Margin = new Thickness(5),
                        Content = new StackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label { Text = $"Flight: {flight.FlightNumber}", FontSize = 18, FontAttributes = FontAttributes.Bold },
                                new Label { Text = $"Duration: {flight.DepartureTime:HH:mm dd/MM/yyyy} - {flight.ArrivalTime:HH:mm dd/MM/yyyy}", FontSize = 14 },
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Vertical,
                                    Spacing = 10,
                                    Children =
                                    {
                                        new Label { Text = "From", FontSize = 24, HorizontalOptions = LayoutOptions.Center },
                                        new Image { Source = flight.DepartureAirport.ImageFullPath, WidthRequest = 50, HeightRequest = 50 },
                                        new Label { Text = flight.DepartureAirport.Name },
                                        new Label { Text = "To", FontSize = 24, HorizontalOptions = LayoutOptions.Center },
                                        new Image { Source = flight.ArrivalAirport.ImageFullPath, WidthRequest = 50, HeightRequest = 50 },
                                        new Label { Text = flight.ArrivalAirport.Name }
                                    }
                                },
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    Spacing = 10,
                                    Children =
                                    {
                                        new Image { Source = flight.Airplane.ImageFullPath, WidthRequest = 60, HeightRequest = 40 },
                                        new Label { Text = $"{flight.Airplane.Manufacturer} {flight.Airplane.Model} (Capacity: {flight.Airplane.Capacity})", FontSize = 12 }
                                    }
                                }
                            }
                        }
                    };

                    HistoryStackLayout.Children.Add(flightFrame);
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load flight history.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}
