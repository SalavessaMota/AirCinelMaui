using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Input;
using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Pages;
using AirCinelMaui.Services;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand RecoverPasswordCommand { get; }

        public LoginViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            LoginCommand = new Command(async () => await ExecuteLoginCommand());
            RegisterCommand = new Command(async () => await NavigateToRegisterPage());
            RecoverPasswordCommand = new Command(async () => await NavigateToRecoverPasswordPage());
        }

        private async Task ExecuteLoginCommand()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter your email and password", "OK");
                return;
            }

            var loginDto = new LoginDto
            {
                Username = Email,
                Password = Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/account/createtokenapi", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        Preferences.Set("AuthToken", loginResponse.Token);
                        Preferences.Set("TokenExpiration", loginResponse.Expiration);
                        Preferences.Set("UserId", loginResponse.UserId);
                        Preferences.Set("UserName", loginResponse.UserName);

                        await Application.Current.MainPage.DisplayAlert("Success", "Login successful! Welcome.", "OK");
                        Application.Current.MainPage = new AppShell(_apiService);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Failed to retrieve token from response.", "OK");
                    }
                }
                catch (JsonException jsonEx)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"JSON Deserialization Error: {jsonEx.Message}", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid email or password", "OK");
            }
        }

        private async Task NavigateToRegisterPage()
        {
            await _navigation.PushAsync(new RegisterPage(_apiService));
        }

        private async Task NavigateToRecoverPasswordPage()
        {
            await _navigation.PushAsync(new RecoverPasswordPage());
        }
    }
}
