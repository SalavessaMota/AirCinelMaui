using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class MyAccountPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public MyAccountPage(ApiService apiService)
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
                await DisplayAlert("Error", "You must be logged in to view your account details.", "OK");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/account/getuser"); // Substituir pela rota correta
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                if (user != null)
                {
                    FirstNameEntry.Text = user.FirstName;
                    LastNameEntry.Text = user.LastName;
                    PhoneNumberEntry.Text = user.PhoneNumber;
                    AddressEntry.Text = user.Address;
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load account details.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        try
        {
            var token = Preferences.Get("AuthToken", string.Empty);

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "You must be logged in to update your account details.", "OK");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updatedUser = new UpdateUserDto
            {
                FirstName = FirstNameEntry.Text,
                LastName = LastNameEntry.Text,
                PhoneNumber = PhoneNumberEntry.Text,
                Address = AddressEntry.Text
            };

            var response = await _httpClient.PutAsJsonAsync("api/account/updateuser", updatedUser); // Substituir pela rota correta
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Your account details have been updated.", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Failed to update your details: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}