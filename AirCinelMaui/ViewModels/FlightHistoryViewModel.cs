using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AirCinelMaui.Models.Dtos;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class FlightHistoryViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<FlightHistoryItemViewModel> FlightHistory { get; } = new ObservableCollection<FlightHistoryItemViewModel>();

        public FlightHistoryViewModel()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            LoadFlightHistory();
        }

        private async void LoadFlightHistory()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to view flight history.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/flights/history");
                if (response.IsSuccessStatusCode)
                {
                    var flightHistory = await response.Content.ReadFromJsonAsync<List<FlightDto>>();

                    FlightHistory.Clear();
                    foreach (var flight in flightHistory)
                    {
                        FlightHistory.Add(new FlightHistoryItemViewModel(flight));
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load flight history.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

    public class FlightHistoryItemViewModel
    {
        public FlightDto Flight { get; }

        public FlightHistoryItemViewModel(FlightDto flight)
        {
            Flight = flight;
        }

        public string FlightNumberText => $"Flight: {Flight.FlightNumber}";
        public string DurationText => $"Duration: {Flight.DepartureTime:HH:mm dd/MM/yyyy} - {Flight.ArrivalTime:HH:mm dd/MM/yyyy}";
        public AirportDto DepartureAirport => Flight.DepartureAirport;
        public AirportDto ArrivalAirport => Flight.ArrivalAirport;
        public AirplaneDto Airplane => Flight.Airplane;
        public string AirplaneInfo => $"{Flight.Airplane.Manufacturer} {Flight.Airplane.Model} (Capacity: {Flight.Airplane.Capacity})";
    }
}
