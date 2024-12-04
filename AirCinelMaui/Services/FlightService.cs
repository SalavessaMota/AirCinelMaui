using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using System.Net.Http.Json;

public class FlightService
{
    private readonly HttpClient _httpClient;

    public FlightService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/");
    }

    // Método para obter voos disponíveis
    public async Task<List<Flight>> GetAvailableFlightsAsync()
    {
        var response = await _httpClient.GetAsync("api/flights/available");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Flight>>();
        }
        else
        {
            throw new Exception("Failed to load available flights.");
        }
    }

    // Método para obter cidades (filtros)
    public async Task<List<City>> GetCitiesAsync()
    {
        var response = await _httpClient.GetAsync("api/flights/cities");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<City>>();
        }
        else
        {
            throw new Exception("Failed to load cities.");
        }
    }

    // Método para filtrar voos
    public async Task<List<Flight>> GetFilteredFlightsAsync(string departureCity = null, string arrivalCity = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(departureCity))
            queryParams.Add($"departureCity={Uri.EscapeDataString(departureCity)}");
        if (!string.IsNullOrEmpty(arrivalCity))
            queryParams.Add($"arrivalCity={Uri.EscapeDataString(arrivalCity)}");

        var queryString = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"api/flights/filter?{queryString}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Flight>>();
        }
        else
        {
            throw new Exception("Failed to load filtered flights.");
        }
    }

    // Método para comprar bilhete
    public async Task<PurchaseTicketResponse> PurchaseTicketAsync(int flightId, string seatNumber)
    {
        var purchaseModel = new PurchaseTicketDto
        {
            FlightId = flightId,
            SeatNumber = seatNumber
        };

        var response = await _httpClient.PostAsJsonAsync("api/flights/purchase", purchaseModel);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PurchaseTicketResponse>();
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to purchase ticket: {errorMessage}");
        }
    }

    public async Task<List<string>> GetAvailableSeatsAsync(int flightId)
    {
        var response = await _httpClient.GetAsync($"api/flights/{flightId}/availableseats");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<string>>();
        }
        else
        {
            throw new Exception($"Failed to load available seats for flight {flightId}.");
        }
    }
}
