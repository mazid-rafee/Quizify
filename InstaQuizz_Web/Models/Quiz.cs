namespace InstaQuiz.Models;

public class Quiz
{
    public int QuizId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Passcode { get; set; }  // For quiz participants
    public string? QuizTakerPasscode { get; set; }  // For quiz taker to access the dashboard
    public DateTime CreatedAt { get; set; }
    public string? Owner { get; set; }  // Add Owner to track who created the quiz
    public ICollection<Question> Questions { get; set; }
    public bool IsCompleted { get; set; }  // Add this flag to track quiz completion
}
