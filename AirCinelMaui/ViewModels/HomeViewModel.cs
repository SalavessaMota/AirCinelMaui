using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AirCinelMaui.Services;
using AirCinelMaui.Pages;
using Microsoft.Maui.Controls;

namespace AirCinelMaui.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        public HomeViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;

            AvailableFlightsCommand = new Command(async () => await NavigateToAvailableFlights());
            FlightHistoryCommand = new Command(async () => await NavigateToFlightHistory());
            UserTicketsCommand = new Command(async () => await NavigateToUserTickets());
            AboutCommand = new Command(async () => await NavigateToAbout());
            LoginCommand = new Command(async () => await NavigateToLogin());

            UpdateButtonVisibility();
        }

        public void OnAppearing()
        {
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            var token = Preferences.Get("AuthToken", string.Empty);
            IsPastFlightsButtonVisible = !string.IsNullOrEmpty(token);
            IsYourTicketsButtonVisible = !string.IsNullOrEmpty(token);
            IsLoginButtonVisible = string.IsNullOrEmpty(token);
        }

        private async Task NavigateToAvailableFlights()
        {
            await _navigation.PushAsync(new AvailableFlightsPage(_apiService));
        }

        private async Task NavigateToFlightHistory()
        {
            await _navigation.PushAsync(new FlightHistoryPage());
        }

        private async Task NavigateToUserTickets()
        {
            await _navigation.PushAsync(new UserTicketsPage(_apiService));
        }

        private async Task NavigateToAbout()
        {
            await _navigation.PushAsync(new AboutPage());
        }

        private async Task NavigateToLogin()
        {
            await _navigation.PushAsync(new LoginPage(_apiService));
        }

        private bool _isPastFlightsButtonVisible;
        public bool IsPastFlightsButtonVisible
        {
            get => _isPastFlightsButtonVisible;
            set => SetProperty(ref _isPastFlightsButtonVisible, value);
        }

        private bool _isYourTicketsButtonVisible;
        public bool IsYourTicketsButtonVisible
        {
            get => _isYourTicketsButtonVisible;
            set => SetProperty(ref _isYourTicketsButtonVisible, value);
        }

        private bool _isLoginButtonVisible;
        public bool IsLoginButtonVisible
        {
            get => _isLoginButtonVisible;
            set => SetProperty(ref _isLoginButtonVisible, value);
        }

        public ICommand AvailableFlightsCommand { get; }
        public ICommand FlightHistoryCommand { get; }
        public ICommand UserTicketsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand LoginCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
