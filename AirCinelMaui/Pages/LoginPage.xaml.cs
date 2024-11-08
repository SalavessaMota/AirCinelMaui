using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AirCinelMaui.Pages;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
	{
		InitializeComponent();
        _httpClient = new HttpClient
        {
            //BaseAddress = new Uri("https://k6glbgpq-5001.uks1.devtunnels.ms/")
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
        _apiService = apiService;
    }

    private async void BtnSignIn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EmailEntry.Text) || string.IsNullOrEmpty(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please enter your email and password", "OK");
            return;
        }

        var loginDto = new LoginDto
        {
            Username = EmailEntry.Text,
            Password = PasswordEntry.Text
        };

        var response = await _httpClient.PostAsJsonAsync("api/account/createtokenapi", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            try
            {
                // Desserializa a resposta JSON para a classe LoginResponse
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    Preferences.Set("AuthToken", loginResponse.Token);
                    Preferences.Set("TokenExpiration", loginResponse.Expiration);
                    Preferences.Set("UserId", loginResponse.UserId);
                    Preferences.Set("UserName", loginResponse.UserName);

                    await DisplayAlert("Success", "Login successful! Welcome.", "OK");
                    await Navigation.PushAsync(new AppShell(_apiService));
                }
                else
                {
                    await DisplayAlert("Error", "Failed to retrieve token from response.", "OK");
                }
            }
            catch (JsonException jsonEx)
            {
                await DisplayAlert("Error", $"JSON Deserialization Error: {jsonEx.Message}", "OK");
            }
        }

    }



    private async void TapRegister_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_apiService));
    }
}