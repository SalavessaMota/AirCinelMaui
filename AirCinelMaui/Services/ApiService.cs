using AirCinelMaui.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AirCinelMaui.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://aircinelmvc.azurewebsites.net/";
    private readonly ILogger<ApiService> _logger;

    JsonSerializerOptions _serializerOptions;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }


    public async Task<ApiResponse<bool>> RegisterUser(string name, string email,
                                                          string phone, string password)
    {
        try
        {
            var register = new Register()
            {
                Name = name,
                Email = email,
                Phone = phone,
                Password = password
            };

            var json = JsonSerializer.Serialize(register, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/Users/Register", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error sending HTTP request: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Error sending HTTP request: {response.StatusCode}"
                };
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error registering user: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }


    public async Task<ApiResponse<bool>> Login(string email, string password)
    {
        try
        {
            var login = new Login()
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(login, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/Users/CreateTokenAPI", content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error sending HTTP request: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Error sending HTTP request: {response.StatusCode}"
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Token>(jsonResult, _serializerOptions);

            Preferences.Set("AccessToken", result!.AccessToken);
            Preferences.Set("UserId", (int)result.UserId!);
            Preferences.Set("UserName", result.UserName);

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    private async Task<HttpResponseMessage> PostRequest(string uri, HttpContent content)
    {
        var url = _baseUrl + uri;
        try
        {
            var result = await _httpClient.PostAsync(url, content);
            return result;
        }
        catch (Exception ex)
        {
            // Log o erro ou trate conforme necessário
            _logger.LogError($"Error sending POST requesto to {uri}: {ex.Message}");
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }

    public async Task<ApiResponse<List<Country>>> GetCountriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_baseUrl + "api/countries/countries");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error sending HTTP request: {response.StatusCode}");
                return new ApiResponse<List<Country>>
                {
                    ErrorMessage = $"Error sending HTTP request: {response.StatusCode}"
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var countries = JsonSerializer.Deserialize<List<Country>>(json, _serializerOptions);

            return new ApiResponse<List<Country>> { Data = countries };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting countries: {ex.Message}");
            return new ApiResponse<List<Country>> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<List<City>>> GetCitiesAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"api/countries/cities/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error sending HTTP request: {response.StatusCode}");
                return new ApiResponse<List<City>>
                {
                    ErrorMessage = $"Error sending HTTP request: {response.StatusCode}"
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var cities = JsonSerializer.Deserialize<List<City>>(json, _serializerOptions);

            return new ApiResponse<List<City>> { Data = cities };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cities: {ex.Message}");
            return new ApiResponse<List<City>> { ErrorMessage = ex.Message };
        }

    }


}
