using AirCinelMaui.ViewModels;

namespace AirCinelMaui.Pages;

public partial class QuestionsPage : ContentPage
{
    public QuestionsPage()
    {
        InitializeComponent();
        BindingContext = new QuestionsViewModel();
    }
}
