namespace AirCinelMaui.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Title = "About AirCinelMaui";
        LoadAppInfo();
    }

    private void LoadAppInfo()
    {
        VersionLabel.Text = AppInfo.VersionString;
        DateLabel.Text = "01-12-2024";
    }
}
