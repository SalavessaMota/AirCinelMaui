using AirCinelMaui.Pages;

namespace AirCinelMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new RegisterPage());
        }
    }
}
