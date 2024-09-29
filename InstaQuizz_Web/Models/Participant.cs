namespace InstaQuiz.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public string Name { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public ICollection<Mark> Marks { get; set; }
    }
}
