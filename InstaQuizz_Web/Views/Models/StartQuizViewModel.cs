namespace InstaQuiz.Models.ViewModels
{
    public class StartQuizViewModel
    {
        public Quiz Quiz { get; set; }
        public string QuizUrl { get; set; }
        public string Passcode { get; set; }
        public string DashboardUrl { get; set; }  // Link to quiz taker's dashboard
        public string QuizTakerPasscode { get; set; }  // Passcode for quiz taker to access the dashboard
    }

}