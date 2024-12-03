using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class UserTicketsPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public UserTicketsPage(ApiService apiService)
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
                await DisplayAlert("Error", "You must be logged in to view your tickets.", "OK");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/flights/tickets");
            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadFromJsonAsync<List<BoughtTicketDto>>();

                TicketsStackLayout.Children.Clear();
                foreach (var ticket in tickets)
                {
                    var ticketFrame = new Frame
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
                                new Label { Text = $"Ticket ID: {ticket.Id}", FontSize = 18, FontAttributes = FontAttributes.Bold },
                                new Label { Text = $"Seat: {ticket.SeatNumber}", FontSize = 14 },
                                new Label { Text = $"Flight: {ticket.Flight.FlightNumber}", FontSize = 14 },
                                new Label { Text = $"From: {ticket.Flight.DepartureAirport.Name}", FontSize = 14 },
                                new Label { Text = $"To: {ticket.Flight.ArrivalAirport.Name}", FontSize = 14 },
                                new Label { Text = $"Departure: {ticket.Flight.DepartureTime:HH:mm dd/MM/yyyy}", FontSize = 14 },
                                new Label { Text = $"Arrival: {ticket.Flight.ArrivalTime:HH:mm dd/MM/yyyy}", FontSize = 14 },
                                new Image { Source = ticket.Flight.Airplane.ImageFullPath, WidthRequest = 100, HeightRequest = 60 }
                            }
                        }
                    };

                    TicketsStackLayout.Children.Add(ticketFrame);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await DisplayAlert("No Tickets", "You don't have any tickets.", "OK");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Failed to load tickets: {errorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}
