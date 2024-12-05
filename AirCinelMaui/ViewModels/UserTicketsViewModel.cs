using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class UserTicketsViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        public ObservableCollection<TicketViewModel> Tickets { get; } = new ObservableCollection<TicketViewModel>();

        public UserTicketsViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            LoadTickets();
        }

        private async void LoadTickets()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to view your tickets.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/flights/tickets");
                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<BoughtTicketDto>>();

                    Tickets.Clear();
                    foreach (var ticket in tickets)
                    {
                        Tickets.Add(new TicketViewModel(ticket));
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await Application.Current.MainPage.DisplayAlert("No Tickets", "You don't have any tickets.", "OK");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load tickets: {errorMessage}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

    public class TicketViewModel
    {
        public BoughtTicketDto Ticket { get; }

        public TicketViewModel(BoughtTicketDto ticket)
        {
            Ticket = ticket;
        }

        public string TicketIdText => $"Ticket ID: {Ticket.Id}";
        public string SeatNumberText => $"Seat: {Ticket.SeatNumber}";
        public string FlightNumberText => $"Flight: {Ticket.Flight.FlightNumber}";
        public string DepartureText => $"From: {Ticket.Flight.DepartureAirport.Name}";
        public string ArrivalText => $"To: {Ticket.Flight.ArrivalAirport.Name}";
        public string DepartureTimeText => $"Departure: {Ticket.Flight.DepartureTime:HH:mm dd/MM/yyyy}";
        public string ArrivalTimeText => $"Arrival: {Ticket.Flight.ArrivalTime:HH:mm dd/MM/yyyy}";
        public string AirplaneImage => Ticket.Flight.Airplane.ImageFullPath;
    }
}
