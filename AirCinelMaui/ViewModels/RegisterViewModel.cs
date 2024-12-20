﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Windows.Input;
using AirCinelMaui.Models;
using AirCinelMaui.Models.Dtos;
using AirCinelMaui.Pages;
using AirCinelMaui.Services;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace AirCinelMaui.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;
    private readonly INavigation _navigation;

    public RegisterViewModel(ApiService apiService, INavigation navigation)
    {
        _apiService = apiService;
        _navigation = navigation;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://aircinelmvc.azurewebsites.net/")
        };

        ChooseImageCommand = new Command(async () => await ChooseImageAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
        LoginCommand = new Command(async () => await NavigateToLoginAsync());

        Countries = new ObservableCollection<Country>();
        Cities = new ObservableCollection<City>();

        LoadCountries();
    }

    #region Properties

    private ImageSource _profileImage;
    public ImageSource ProfileImage
    {
        get => _profileImage;
        set => SetProperty(ref _profileImage, value);
    }

    private FileResult _selectedImageFile;

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

    private string _email;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
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

    private ObservableCollection<Country> _countries;
    public ObservableCollection<Country> Countries
    {
        get => _countries;
        set => SetProperty(ref _countries, value);
    }

    private Country _selectedCountry;
    public Country SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            if (SetProperty(ref _selectedCountry, value))
            {
                LoadCities(value.Id);
            }
        }
    }

    private ObservableCollection<City> _cities;
    public ObservableCollection<City> Cities
    {
        get => _cities;
        set => SetProperty(ref _cities, value);
    }

    private City _selectedCity;
    public City SelectedCity
    {
        get => _selectedCity;
        set => SetProperty(ref _selectedCity, value);
    }

    private string _password;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _confirmPassword;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    #endregion

    #region Commands

    public ICommand ChooseImageCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand LoginCommand { get; }

    #endregion

    #region Methods

    private async void LoadCountries()
    {
        try
        {
            var response = await _apiService.GetCountriesAsync();

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load countries: {response.ErrorMessage}", "Ok");
                return;
            }

            Countries = new ObservableCollection<Country>(response.Data);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private async void LoadCities(int countryId)
    {
        try
        {
            var response = await _apiService.GetCitiesAsync(countryId);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load cities: {response.ErrorMessage}", "Ok");
                return;
            }

            Cities = new ObservableCollection<City>(response.Data);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load cities: {ex.Message}", "OK");
        }
    }

    private async Task ChooseImageAsync()
    {
        _selectedImageFile = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select a profile image",
            FileTypes = FilePickerFileType.Images
        });

        if (_selectedImageFile != null)
        {
            var stream = await _selectedImageFile.OpenReadAsync();
            ProfileImage = ImageSource.FromStream(() => stream);
        }
    }

    private async Task RegisterAsync()
    {
        // Validações...
        if (SelectedCountry == null || SelectedCity == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please select a country and a city.", "OK");
            return;
        }

        try
        {
            // Preparar conteúdo multipart/form-data
            using var content = new MultipartFormDataContent();

            // Adicionar dados de texto
            content.Add(new StringContent(FirstName), "FirstName");
            content.Add(new StringContent(LastName), "LastName");
            content.Add(new StringContent(Address ?? ""), "Address");
            content.Add(new StringContent(PhoneNumber ?? ""), "PhoneNumber");
            content.Add(new StringContent(Email), "Username");
            content.Add(new StringContent(Password), "Password");
            content.Add(new StringContent(ConfirmPassword), "ConfirmPassword");
            content.Add(new StringContent(SelectedCity.Id.ToString()), "CityId");

            // Adicionar imagem, se fornecida
            if (_selectedImageFile != null)
            {
                var stream = await _selectedImageFile.OpenReadAsync();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "ImageFile", _selectedImageFile.FileName);
            }

            // Enviar pedido para o backend
            var response = await _httpClient.PostAsync("api/account/registerWithImage", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Registration successful! Please check your email to confirm your account.", "OK");
                await _navigation.PopAsync();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"Registration failed: {errorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
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
                await Application.Current.MainPage.DisplayAlert("Error", $"Image upload failed: {response.StatusCode} - {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Image upload failed: {ex.Message}", "OK");
        }

        return Guid.Empty;
    }

    private async Task NavigateToLoginAsync()
    {
        await _navigation.PushAsync(new LoginPage(_apiService));
    }

    #endregion
}
