using AirCinelMaui.Models.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class ChangePasswordPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public ChangePasswordPage()
    {
        InitializeComponent();

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        // Validate user input
        string oldPassword = OldPasswordEntry.Text;
        string newPassword = NewPasswordEntry.Text;
        string confirmNewPassword = ConfirmNewPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(oldPassword) ||
            string.IsNullOrWhiteSpace(newPassword) ||
            string.IsNullOrWhiteSpace(confirmNewPassword))
        {
            await DisplayAlert("Error", "All fields are required.", "OK");
            return;
        }

        if (newPassword != confirmNewPassword)
        {
            await DisplayAlert("Error", "New password and confirmation do not match.", "OK");
            return;
        }

        try
        {
            var token = Preferences.Get("AuthToken", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "You must be logged in to change your password.", "OK");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var changePasswordDto = new ChangePasswordDto
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                Confirm = confirmNewPassword
            };

            var response = await _httpClient.PostAsJsonAsync("api/account/changepassword", changePasswordDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                await DisplayAlert("Success", "Password changed successfully.", "OK");

                // Navigate back or clear entries
                OldPasswordEntry.Text = string.Empty;
                NewPasswordEntry.Text = string.Empty;
                ConfirmNewPasswordEntry.Text = string.Empty;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}
