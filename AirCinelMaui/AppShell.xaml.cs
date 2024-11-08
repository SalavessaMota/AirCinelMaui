using AirCinelMaui.Pages;
using AirCinelMaui.Services;

namespace AirCinelMaui
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _apiService;

        public AppShell(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;            
            
            ConfigureShell();
        }


        private void ConfigureShell()
        {
            var homePage = new HomePage(_apiService);
            var profilePage = new ProfilePage(_apiService);

            Items.Add(new TabBar
            {
                Items =
            {
                new ShellContent { Title = "Home",Icon = "home",Content = homePage  },
                new ShellContent { Title = "Profile",Icon = "profile",Content = profilePage }
            }
            });
        }
    }
}
