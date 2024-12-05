using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows.Input;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Pages;
using AirCinelMaui.Services;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class MyAccountViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        public MyAccountViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            SaveChangesCommand = new Command(async () => await OnSaveChanges());
            ChangePasswordCommand = new Command(async () => await NavigateToChangePasswordPage());

            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to view your account details.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/account/getuser");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDto>();
                    if (user != null)
                    {
                        FirstName = user.FirstName;
                        LastName = user.LastName;
                        PhoneNumber = user.PhoneNumber;
                        Address = user.Address;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load account details.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task OnSaveChanges()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to update your account details.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var updatedUser = new UpdateUserDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber,
                    Address = Address
                };

                var response = await _httpClient.PutAsJsonAsync("api/account/updateuser", updatedUser);
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Your account details have been updated.", "OK");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update your details: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task NavigateToChangePasswordPage()
        {
            await _navigation.PushAsync(new ChangePasswordPage());
        }

        public ICommand SaveChangesCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
    }
}
