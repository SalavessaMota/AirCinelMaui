using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows.Input;
using AirCinelMaui.Services;
using AirCinelMaui.Pages;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        public ProfileViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
            };

            ChangeProfileImageCommand = new Command(async () => await ChangeProfileImage());
            MyAccountCommand = new Command(async () => await NavigateToMyAccount());
            FaqCommand = new Command(async () => await NavigateToFaq());
            LogoutCommand = new Command(Logout);

            LoadProfileData();
        }

        private string _profileImage;
        public string ProfileImage
        {
            get => _profileImage;
            set => SetProperty(ref _profileImage, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public ICommand ChangeProfileImageCommand { get; }
        public ICommand MyAccountCommand { get; }
        public ICommand FaqCommand { get; }
        public ICommand LogoutCommand { get; }

        private async void LoadProfileData()
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Token not found. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/account/getuserimage");

                if (response.IsSuccessStatusCode)
                {
                    var profileImagePath = await response.Content.ReadAsStringAsync();
                    ProfileImage = profileImagePath;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load profile image.", "OK");
                    ProfileImage = "profile.png";
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }

            Username = Preferences.Get("UserName", string.Empty);
        }

        private async Task ChangeProfileImage()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select Profile Picture"
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    var content = new MultipartFormDataContent();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                    content.Add(fileContent, "file", result.FileName);

                    var token = Preferences.Get("AuthToken", string.Empty);

                    if (string.IsNullOrEmpty(token))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Token not found. Please log in again.", "OK");
                        return;
                    }

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.PostAsync("api/account/uploadImage", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var newImagePath = await response.Content.ReadAsStringAsync();
                        ProfileImage = newImagePath;
                        await Application.Current.MainPage.DisplayAlert("Success", "Profile picture updated successfully.", "OK");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Failed to upload the new profile picture.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task NavigateToMyAccount()
        {
            await _navigation.PushAsync(new MyAccountPage(_apiService));
        }

        private async Task NavigateToFaq()
        {
            await _navigation.PushAsync(new QuestionsPage());
        }

        private void Logout()
        {
            Preferences.Set("AuthToken", string.Empty);
            Preferences.Set("TokenExpiration", string.Empty);
            Preferences.Set("UserId", string.Empty);
            Preferences.Set("UserName", string.Empty);

            Application.Current.MainPage = new NavigationPage(new HomePage(_apiService));
        }
    }
}
