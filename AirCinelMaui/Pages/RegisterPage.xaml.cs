using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace AirCinelMaui.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private List<Country> _countries;
    private List<City> _cities;
    private FileResult _selectedImageFile;

    public RegisterPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            //BaseAddress = new Uri("https://k6glbgpq-5001.uks1.devtunnels.ms/")
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };
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
            var response = await _httpClient.GetAsync("api/countries/countries");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _countries = JsonConvert.DeserializeObject<List<Country>>(json);
                CountryPicker.ItemsSource = _countries;
                CountryPicker.ItemDisplayBinding = new Binding("Name");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load countries: {ex.Message}", "OK");
        }
    }

    private async void BtnSignup_Clicked(object sender, EventArgs e)
    {
        // Verifica se todos os campos obrigatórios estão preenchidos
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        if (CountryPicker.SelectedItem is not Country selectedCountry ||
            CityPicker.SelectedItem is not City selectedCity)
        {
            await DisplayAlert("Error", "Please select a country and a city.", "OK");
            return;
        }

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

            // Lê o stream da imagem e cria o conteúdo para o upload
            var stream = await imageFile.OpenReadAsync();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            // Adiciona o arquivo ao conteúdo do form data com o nome "file"
            content.Add(fileContent, "file", imageFile.FileName);

            // Envia a requisição para a API
            var response = await _httpClient.PostAsync("api/account/uploadimage", content);
            var responseContent = await response.Content.ReadAsStringAsync(); // Captura o conteúdo da resposta

            // Verifica se a resposta foi bem-sucedida
            if (response.IsSuccessStatusCode)
            {
                var imageId = JsonConvert.DeserializeObject<Guid>(responseContent);
                return imageId;
            }
            else
            {
                // Exibe a mensagem de erro se o upload falhar
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
        // Navegar para a página de login
        await Navigation.PushAsync(new LoginPage());
    }

    private async void CountryPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (CountryPicker.SelectedItem is Country selectedCountry)
        {
            await LoadCities(selectedCountry.Id);
        }
    }

    private async Task LoadCities(int countryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/countries/cities/{countryId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _cities = JsonConvert.DeserializeObject<List<City>>(json);
                CityPicker.ItemsSource = _cities;
                CityPicker.ItemDisplayBinding = new Binding("Name");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load cities: {ex.Message}", "OK");
        }
    }
}
