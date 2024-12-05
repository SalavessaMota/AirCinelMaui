using System.Collections.ObjectModel;

namespace AirCinelMaui.ViewModels
{
    public class QuestionsViewModel : BaseViewModel
    {
        public string Title { get; set; } = "Frequently Asked Questions";

        public ObservableCollection<QuestionAnswer> Questions { get; set; }

        public QuestionsViewModel()
        {
            Questions = new ObservableCollection<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    Question = "1. How can I book a flight?",
                    Answer = "To book a flight, navigate to the 'Available Flights' tab, choose your desired flight from the list, select an available seat, and proceed with the booking."
                },
                new QuestionAnswer
                {
                    Question = "2. Can I change my seat after booking?",
                    Answer = "Once you've booked a seat, you will need to contact customer support to change it. Seat changes are subject to availability."
                },
                new QuestionAnswer
                {
                    Question = "3. How do I view my flight history?",
                    Answer = "You can view your flight history in the 'My Flights History' tab. Here, you will see your past flights."
                },
                new QuestionAnswer
                {
                    Question = "4. What should I do if I forgot my password?",
                    Answer = "If you forgot your password, go to the login screen and tap on 'Forgot Password?' to initiate the password recovery process."
                },
                new QuestionAnswer
                {
                    Question = "5. Can I cancel my booking?",
                    Answer = "Cancellations are available for eligible bookings. Please contact support for more details on your specific booking."
                },
                new QuestionAnswer
                {
                    Question = "6. How can I update my profile information?",
                    Answer = "You can update your profile information in the 'My Account' tab under 'Profile'. Here, you can change your contact details, address, and other personal information."
                }
            };
        }
    }

    public class QuestionAnswer
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
