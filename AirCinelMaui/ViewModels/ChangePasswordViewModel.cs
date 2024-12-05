using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using AirCinelMaui.Models.Dtos;
using Microsoft.Maui.Controls;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;

        private string _oldPassword;
        public string OldPassword
        {
            get => _oldPassword;
            set => SetProperty(ref _oldPassword, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _confirmNewPassword;
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set => SetProperty(ref _confirmNewPassword, value);
        }

        public ICommand ChangePasswordCommand { get; }

        public ChangePasswordViewModel()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            ChangePasswordCommand = new Command(async () => await OnChangePasswordClicked());
        }

        private async Task OnChangePasswordClicked()
        {
            // Validações
            if (string.IsNullOrWhiteSpace(OldPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "All fields are required.", "OK");
                return;
            }

            if (NewPassword.Length < 8)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "The password must be at least 8 characters long.", "OK");
                return;
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            if (!passwordRegex.IsMatch(NewPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "The password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.", "OK");
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "The password and confirmation password do not match.", "OK");
                return;
            }

            try
            {
                // Verifica o token do usuário
                var token = Preferences.Get("AuthToken", string.Empty);
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to change your password.", "OK");
                    return;
                }

                // Configura o cabeçalho de autenticação
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Cria o objeto DTO
                var changePasswordDto = new ChangePasswordDto
                {
                    OldPassword = OldPassword,
                    NewPassword = NewPassword,
                    Confirm = ConfirmNewPassword
                };

                // Faz a requisição para a API
                var response = await _httpClient.PostAsJsonAsync("api/account/changepassword", changePasswordDto);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Password changed successfully.", "OK");

                    // Limpa os campos
                    OldPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
