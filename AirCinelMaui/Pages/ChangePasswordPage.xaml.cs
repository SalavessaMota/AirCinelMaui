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
        // Captura os valores das entradas do usuário
        string oldPassword = OldPasswordEntry.Text;
        string newPassword = NewPasswordEntry.Text;
        string confirmNewPassword = ConfirmNewPasswordEntry.Text;

        // Validações manuais adicionais (requisitos da DTO)
        if (string.IsNullOrWhiteSpace(oldPassword) ||
            string.IsNullOrWhiteSpace(newPassword) ||
            string.IsNullOrWhiteSpace(confirmNewPassword))
        {
            await DisplayAlert("Error", "All fields are required.", "OK");
            return;
        }

        if (newPassword.Length < 8)
        {
            await DisplayAlert("Error", "The password must be at least 8 characters long.", "OK");
            return;
        }

        var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
        if (!passwordRegex.IsMatch(newPassword))
        {
            await DisplayAlert("Error", "The password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.", "OK");
            return;
        }

        if (newPassword != confirmNewPassword)
        {
            await DisplayAlert("Error", "The password and confirmation password do not match.", "OK");
            return;
        }

        try
        {
            // Verifica o token do usuário
            var token = Preferences.Get("AuthToken", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "You must be logged in to change your password.", "OK");
                return;
            }

            // Configura o cabeçalho da autenticação
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Cria o objeto DTO
            var changePasswordDto = new ChangePasswordDto
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                Confirm = confirmNewPassword
            };

            // Faz a requisição para a API
            var response = await _httpClient.PostAsJsonAsync("api/account/changepassword", changePasswordDto);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Password changed successfully.", "OK");

                // Limpa os campos
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
