using System.Net.Http.Json;
using System.Windows.Input;
using AirCinelMaui.Models.Dtos;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class RecoverPasswordViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isStatusMessageVisible;
        public bool IsStatusMessageVisible
        {
            get => _isStatusMessageVisible;
            set => SetProperty(ref _isStatusMessageVisible, value);
        }

        public ICommand RecoverPasswordCommand { get; }

        public RecoverPasswordViewModel()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            RecoverPasswordCommand = new Command(async () => await ExecuteRecoverPasswordCommand());
        }

        private async Task ExecuteRecoverPasswordCommand()
        {
            if (string.IsNullOrEmpty(Email))
            {
                StatusMessage = "Please enter your email.";
                IsStatusMessageVisible = true;
                return;
            }

            var recoverPasswordDto = new RecoverPasswordDto
            {
                Email = Email
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/account/recoverpassword", recoverPasswordDto);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "A recovery email has been sent to your address.", "OK");
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    StatusMessage = $"Error: {errorContent}";
                    IsStatusMessageVisible = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsStatusMessageVisible = true;
            }
        }
    }
}
