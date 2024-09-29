namespace InstaQuiz.Models
{
    public class Mark
    {
        public int MarkId { get; set; }
        public int Score { get; set; }

        // Add a relationship to the Question
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int ParticipantId { get; set; }
        public Participant Participant { get; set; }
    }
}
