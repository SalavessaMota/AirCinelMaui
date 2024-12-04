using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Services;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;
    private List<City> _cities;
    private FileResult _selectedImageFile;

    public RegisterPage(ApiService apiService)
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCountries();
    }

    private async Task LoadCountries()
    {
        try
        {
            var response = await _apiService.GetCountriesAsync();

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                await DisplayAlert("Error", $"Failed to load countries: {response.ErrorMessage}", "Ok");
            }

            CountryPicker.ItemsSource = response.Data;
            CountryPicker.ItemDisplayBinding = new Binding("Name");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private async Task LoadCities(int countryId)
    {
        try
        {
            var response = await _apiService.GetCitiesAsync(countryId);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                await DisplayAlert("Error", $"Failed to load cities: {response.ErrorMessage}", "Ok");
            }

            CityPicker.ItemsSource = response.Data;
            CityPicker.ItemDisplayBinding = new Binding("Name");
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Failed to load cities", "OK");
        }
    }

    private async void CountryPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (CountryPicker.SelectedItem is Country selectedCountry)
        {
            await LoadCities(selectedCountry.Id);
        }
    }

    private async void BtnSignup_Clicked(object sender, EventArgs e)
    {
        // Validação de campos obrigatórios
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        // Validação de email
        if (!new EmailAddressAttribute().IsValid(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a valid email address.", "OK");
            return;
        }

        // Validação de password
        var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
        if (!passwordRegex.IsMatch(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "The password must be at least 8 characters long, contain one uppercase letter, one lowercase letter, one digit, and one special character.", "OK");
            return;
        }

        // Verificação de correspondência entre senha e confirmação
        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Error", "The password and confirmation password do not match.", "OK");
            return;
        }

        // Validação de país e cidade selecionados
        if (CountryPicker.SelectedItem is not Country selectedCountry ||
            CityPicker.SelectedItem is not City selectedCity)
        {
            await DisplayAlert("Error", "Please select a country and a city.", "OK");
            return;
        }

        // Upload da imagem (se selecionada)
        Guid imageId = Guid.Empty;
        if (_selectedImageFile != null)
        {
            imageId = await UploadImageAsync(_selectedImageFile);

            if (imageId == Guid.Empty)
            {
                await DisplayAlert("Error", "Image upload failed. Please try again.", "OK");
                return;
            }
        }

        // Criação do DTO de registro
        var registerDto = new RegisterDto
        {
            FirstName = FirstNameEntry.Text.Trim(),
            LastName = LastNameEntry.Text.Trim(),
            Address = AddressEntry.Text?.Trim(),
            PhoneNumber = PhoneNumberEntry.Text?.Trim(),
            Username = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text,
            ConfirmPassword = ConfirmPasswordEntry.Text,
            CityId = selectedCity.Id,
            ImageId = imageId
        };

        try
        {
            // Envio do registro para a API
            var response = await _httpClient.PostAsJsonAsync("api/account/register", registerDto);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Registration successful! Please check your email to confirm your account.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Registration failed: {errorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }


    private async void ChooseImage_Clicked(object sender, EventArgs e)
    {
        _selectedImageFile = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select a profile image",
            FileTypes = FilePickerFileType.Images
        });

        if (_selectedImageFile != null)
        {
            var stream = await _selectedImageFile.OpenReadAsync();
            ProfileImage.Source = ImageSource.FromStream(() => stream);
        }
    }

    private async Task<Guid> UploadImageAsync(FileResult imageFile)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var stream = await imageFile.OpenReadAsync();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", imageFile.FileName);
            var response = await _httpClient.PostAsync("api/account/uploadimage", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var imageId = JsonConvert.DeserializeObject<Guid>(responseContent);
                return imageId;
            }
            else
            {
                await DisplayAlert("Error", $"Image upload failed: {response.StatusCode} - {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Image upload failed: {ex.Message}", "OK");
        }

        return Guid.Empty;
    }

    private async void TapLogin_Tapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_apiService));
    }
}
