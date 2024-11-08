using AirCinelMaui.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public ProfilePage(ApiService apiService)
	{
		InitializeComponent();
        _httpClient = new HttpClient
        {
            //BaseAddress = new Uri("https://k6glbgpq-5001.uks1.devtunnels.ms/")
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Obtém o token da `Preference`
            var token = Preferences.Get("AuthToken", string.Empty);

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "Token not found. Please log in again.", "OK");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/account/getuserimage");

            if (response.IsSuccessStatusCode)
            {
                var profileImagePath = await response.Content.ReadAsStringAsync();
                ImgBtnProfile.Source = ImageSource.FromUri(new Uri(profileImagePath));
            }
            else
            {
                await DisplayAlert("Error", "Failed to load profile image.", "OK");
                ImgBtnProfile.Source = "profile.png";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }

        LblUsername.Text = Preferences.Get("UserName", string.Empty);
    }




    private void MyAccount_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void Faq_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new QuestionsPage());
    }

    private void ImgBtnProfile_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnLogout_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("AuthToken", string.Empty);
        Preferences.Set("TokenExpiration", string.Empty);
        Preferences.Set("UserId", string.Empty);
        Preferences.Set("UserName", string.Empty);

        Application.Current!.MainPage = new NavigationPage(new HomePage(_apiService));
    }
}