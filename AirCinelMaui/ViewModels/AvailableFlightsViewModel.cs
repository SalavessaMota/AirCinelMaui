using System.Collections.ObjectModel;
using System.Windows.Input;
using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using Microsoft.Maui.Controls;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AirCinelMaui.Pages;

namespace AirCinelMaui.ViewModels
{
    public class AvailableFlightsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly HttpClient _httpClient;
        private readonly INavigation _navigation;

        public ObservableCollection<Flight> Flights { get; } = new ObservableCollection<Flight>();
        public ObservableCollection<string> DepartureCities { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ArrivalCities { get; } = new ObservableCollection<string>();

        private string _selectedDepartureCity;
        public string SelectedDepartureCity
        {
            get => _selectedDepartureCity;
            set => SetProperty(ref _selectedDepartureCity, value);
        }

        private string _selectedArrivalCity;
        public string SelectedArrivalCity
        {
            get => _selectedArrivalCity;
            set => SetProperty(ref _selectedArrivalCity, value);
        }

        public ICommand ApplyFiltersCommand { get; }
        public ICommand PurchaseTicketCommand { get; }

        public AvailableFlightsViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            ApplyFiltersCommand = new Command(async () => await ApplyFilters());
            PurchaseTicketCommand = new Command<int>(async (flightId) => await PurchaseTicket(flightId));

            Initialize();
        }

        private async void Initialize()
        {
            await PopulateFilters();
            await LoadFlights();
        }

        private async Task PopulateFilters()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var citiesResponse = await _httpClient.GetAsync("api/flights/cities");
                if (citiesResponse.IsSuccessStatusCode)
                {
                    var cities = await citiesResponse.Content.ReadFromJsonAsync<List<City>>();
                    DepartureCities.Clear();
                    ArrivalCities.Clear();

                    cities = cities.OrderBy(c => c.Name).ToList();

                    foreach (var city in cities)
                    {
                        DepartureCities.Add(city.Name);
                        ArrivalCities.Add(city.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load filters: {ex.Message}", "OK");
            }
        }

        private async Task LoadFlights()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/flights/available");
                if (response.IsSuccessStatusCode)
                {
                    var flights = await response.Content.ReadFromJsonAsync<List<Flight>>();
                    Flights.Clear();
                    foreach (var flight in flights)
                    {
                        Flights.Add(flight);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load flights.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task ApplyFilters()
        {
            await LoadFilteredFlights(SelectedDepartureCity, SelectedArrivalCity);
        }

        private async Task LoadFilteredFlights(string departureCity = null, string arrivalCity = null)
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(departureCity))
                    queryParams.Add($"departureCity={departureCity}");
                if (!string.IsNullOrEmpty(arrivalCity))
                    queryParams.Add($"arrivalCity={arrivalCity}");

                string queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/flights/filter?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var flights = await response.Content.ReadFromJsonAsync<List<Flight>>();
                    Flights.Clear();
                    foreach (var flight in flights)
                    {
                        Flights.Add(flight);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load flights.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task PurchaseTicket(int flightId)
        {
            var token = Preferences.Get("AuthToken", string.Empty);

            if (string.IsNullOrEmpty(token))
            {
                bool goToLogin = await Application.Current.MainPage.DisplayAlert("Error", "You are not logged in. Please log in to purchase a ticket.", "Go to Login", "Cancel");
                if (goToLogin)
                {
                    await _navigation.PushAsync(new LoginPage(_apiService));
                }
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var seatResponse = await _httpClient.GetAsync($"api/flights/{flightId}/availableseats");
            if (!seatResponse.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load available seats.", "OK");
                return;
            }

            var availableSeats = await seatResponse.Content.ReadFromJsonAsync<List<string>>();
            string seatNumber = await Application.Current.MainPage.DisplayActionSheet("Select Seat", "Cancel", null, availableSeats.ToArray());

            if (string.IsNullOrEmpty(seatNumber) || seatNumber == "Cancel")
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Seat selection is required.", "OK");
                return;
            }

            var purchaseModel = new PurchaseTicketDto
            {
                FlightId = flightId,
                SeatNumber = seatNumber
            };

            var response = await _httpClient.PostAsJsonAsync("api/flights/purchase", purchaseModel);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PurchaseTicketResponse>();
                await Application.Current.MainPage.DisplayAlert("Success", $"{result.Message} Ticket ID: {result.TicketId}", "OK");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Purchase Failed", errorMessage, "OK");
            }
        }
    }
}
