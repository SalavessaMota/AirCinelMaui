using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class RecoverPasswordPage : ContentPage
{
    public RecoverPasswordPage()
    {
        InitializeComponent();
        BindingContext = new RecoverPasswordViewModel();
    }
}
