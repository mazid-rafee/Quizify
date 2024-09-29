namespace InstaQuiz.Models.ViewModels
{
    public class QuizTakerDashboardViewModel
    {
        public Quiz Quiz { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalParticipants { get; set; }
        public double? AverageScore { get; set; }

        // New property to hold the list of participants
        public List<ParticipantResultViewModel> Participants { get; set; }
    }

    public class ParticipantResultViewModel
    {
        public string ParticipantName { get; set; }
        public int TotalScore { get; set; }
        public List<Mark> Marks { get; set; }  // Marks for each question
    }
}
