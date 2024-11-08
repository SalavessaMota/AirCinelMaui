using AirCinelMaui.Pages;
using AirCinelMaui.Services;
using AirCinelMaui.Validations;

namespace AirCinelMaui
{
    public partial class App : Application
    {
        private readonly ApiService _apiService;
        private readonly IValidator _validator;

        public App(ApiService apiService, IValidator validator)
        {
            InitializeComponent();
            _apiService = apiService;
            _validator = validator;

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
