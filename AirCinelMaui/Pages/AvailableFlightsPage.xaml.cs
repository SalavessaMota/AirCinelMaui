using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class AvailableFlightsPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public AvailableFlightsPage(ApiService apiService)
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

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/flights/available");
            if (response.IsSuccessStatusCode)
            {
                var flights = await response.Content.ReadFromJsonAsync<List<Flight>>();

                FlightsStackLayout.Children.Clear();
                foreach (var flight in flights)
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
                                new Label { Text = flight.FlightNumber, FontSize = 18, FontAttributes = FontAttributes.Bold },
                                new Label { Text = $"Duration: {flight.DepartureTime:HH:mm} - {flight.ArrivalTime:HH:mm}", FontSize = 14 },

                                // Departure and Arrival Airports
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Vertical,
                                    Spacing = 10,
                                    Children =
                                    {
                                        new Label { Text = "From", FontSize = 24, HorizontalOptions = LayoutOptions.Center },
                                        new Image { Source = flight.DepartureAirport.ImageFullPath, WidthRequest = 50, HeightRequest = 50 },
                                        new Label { Text = $"{flight.DepartureAirport.Name} at {flight.DepartureTime:HH:mm dd/MM/yyyy}" },
                                        new Label { Text = "To", FontSize = 24, HorizontalOptions = LayoutOptions.Center },
                                        new Image { Source = flight.ArrivalAirport.ImageFullPath, WidthRequest = 50, HeightRequest = 50 },
                                        new Label { Text = $"{flight.ArrivalAirport.Name} at {flight.ArrivalTime:HH:mm dd/MM/yyyy}" }
                                    }
                                },

                                // Airplane Information
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    Spacing = 10,
                                    Children =
                                    {
                                        new Image { Source = flight.Airplane.ImageFullPath, WidthRequest = 60, HeightRequest = 40 },
                                        new Label { Text = $"{flight.Airplane.Manufacturer} {flight.Airplane.Model} (Capacity: {flight.Airplane.Capacity})", FontSize = 12 }
                                    }
                                },

                                // Purchase Button
                                new Button
                                {
                                    Text = "Buy Ticket",
                                    BackgroundColor = Colors.Blue,
                                    TextColor = Colors.White,
                                    Command = new Command(async () => await PurchaseTicket(flight.Id))
                                }
                            }
                        }
                    };

                    FlightsStackLayout.Children.Add(flightFrame);
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load flights.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async Task PurchaseTicket(int flightId)
    {
        var token = Preferences.Get("AuthToken", string.Empty);

        if (string.IsNullOrEmpty(token))
        {
            bool goToLogin = await DisplayAlert("Error", "You are not logged in. Please log in to purchase a ticket.", "Go to Login", "Cancel");
            if (goToLogin)
            {
                await Navigation.PushAsync(new LoginPage(_apiService));
            }
            return;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Obter a lista de lugares disponíveis para o voo
        var seatResponse = await _httpClient.GetAsync($"api/flights/{flightId}/availableseats");
        if (!seatResponse.IsSuccessStatusCode)
        {
            await DisplayAlert("Error", "Failed to load available seats.", "OK");
            return;
        }

        var availableSeats = await seatResponse.Content.ReadFromJsonAsync<List<string>>();
        string seatNumber = await DisplayActionSheet("Select Seat", "Cancel", null, availableSeats.ToArray());

        if (string.IsNullOrEmpty(seatNumber) || seatNumber == "Cancel")
        {
            await DisplayAlert("Error", "Seat selection is required.", "OK");
            return;
        }

        // Preparar dados para a compra
        var purchaseModel = new PurchaseTicketDto
        {
            FlightId = flightId,
            SeatNumber = seatNumber
        };

        var response = await _httpClient.PostAsJsonAsync("api/flights/purchase", purchaseModel);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PurchaseTicketResponse>();
            await DisplayAlert("Success", $"{result.Message} Ticket ID: {result.TicketId}", "OK");
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Purchase Failed", errorMessage, "OK");
        }
    }
}