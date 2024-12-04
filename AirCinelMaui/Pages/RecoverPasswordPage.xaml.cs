using AirCinelMaui.Models.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class RecoverPasswordPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public RecoverPasswordPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
    }

    private async void BtnRecoverPassword_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EmailEntry.Text))
        {
            StatusLabel.Text = "Please enter your email.";
            StatusLabel.IsVisible = true;
            return;
        }

        var recoverPasswordDto = new RecoverPasswordDto
        {
            Email = EmailEntry.Text
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/recoverpassword", recoverPasswordDto);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "A recovery email has been sent to your address.", "OK");
                await Navigation.PopAsync(); // Voltar para a página anterior
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                StatusLabel.Text = $"Error: {errorContent}";
                StatusLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            StatusLabel.IsVisible = true;
        }
    }
}
