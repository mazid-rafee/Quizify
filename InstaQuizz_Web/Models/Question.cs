namespace InstaQuiz.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } // The question itself
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        // Four possible answers
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }

        // Index or value indicating the correct option
        public int CorrectOption { get; set; } // 1, 2, 3, or 4 to indicate which option is correct
    }
}
