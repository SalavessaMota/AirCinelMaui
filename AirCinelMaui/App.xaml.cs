using AirCinelMaui.Pages;
using AirCinelMaui.Services;

namespace AirCinelMaui
{
    public partial class App : Application
    {
        private readonly ApiService _apiService;

        public App(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            SetMainPage();
        }

        private void SetMainPage()
        {
            var accessToken = Preferences.Get("AuthToken", string.Empty);

            if (string.IsNullOrEmpty(accessToken))
            {
                MainPage = new NavigationPage(new LoginPage(_apiService));
                return;
            }

            MainPage = new AppShell(_apiService);
        }
    }
}
