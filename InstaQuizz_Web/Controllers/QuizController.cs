using Microsoft.AspNetCore.Mvc;
using InstaQuiz.Data;
using InstaQuiz.Models;
using InstaQuiz.Services;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using InstaQuiz.Models.ViewModels;
using System;

namespace InstaQuiz.Controllers
{
    public class QuizController : Controller
    {
        private readonly InstaQuizContext _context;
        private readonly MLService _mlService;

        public QuizController(InstaQuizContext context, MLService mlService)
        {
            _context = context;
            _mlService = mlService;
        }

        [HttpGet]
        public IActionResult GenerateQuiz()
        {
            return View();  // Render the Generate view where the user can input the prompt
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQuiz(string prompt)
        {
            // Call the ML service to generate quiz questions based on the prompt
            var generatedQuestions = await _mlService.GenerateQuiz(prompt);

            var quiz = new Quiz
            {
                Title = "Generated Quiz",
                Description = prompt,
                Questions = generatedQuestions.Select(q => new Question
                {
                    Text = q.Text,
                    Option1 = q.Option1,
                    Option2 = q.Option2,
                    Option3 = q.Option3,
                    Option4 = q.Option4,
                    CorrectOption = q.CorrectOption
                }).ToList(),
                CreatedAt = DateTime.Now,
                Passcode = GeneratePasscode()
            };

            // Return a view to ask for approval of the generated quiz
            return View("ApproveQuiz", quiz);
        }

        public async Task<IActionResult> ApproveQuiz(Quiz quiz, bool approve)
        {
            if (approve)
            {
                // Generate participant and quiz taker passcodes
                quiz.Passcode = GeneratePasscode();  // For participants
                quiz.QuizTakerPasscode = GeneratePasscode();  // For quiz taker dashboard

                // Save the quiz with both passcodes
                _context.Quizes.Add(quiz);
                await _context.SaveChangesAsync();

                // Redirect to the StartQuiz action
                return RedirectToAction("StartQuiz", new { quizId = quiz.QuizId, passcode = quiz.Passcode, quizTakerPasscode = quiz.QuizTakerPasscode });
            }
            else
            {
                // If not approved, regenerate the quiz using the same prompt
                return await GenerateQuiz(quiz.Description);  // Regenerate the quiz using the same prompt
            }
        }

        public IActionResult StartQuiz(int quizId, string passcode, string quizTakerPasscode)
        {
            var quiz = _context.Quizes.Include(q => q.Questions).FirstOrDefault(q => q.QuizId == quizId);
            if (quiz == null)
            {
                return NotFound();
            }

            // Generate the URLs for participants and quiz taker dashboard
            var quizUrl = Url.Action("TakeQuiz", "Quiz", new { quizId = quiz.QuizId }, Request.Scheme);
            var dashboardUrl = Url.Action("QuizTakerDashboard", "Quiz", new { quizId = quiz.QuizId }, Request.Scheme);

            // Pass the quiz, URLs, and passcodes to the view model
            var viewModel = new StartQuizViewModel
            {
                Quiz = quiz,
                QuizUrl = quizUrl,
                Passcode = passcode,
                DashboardUrl = dashboardUrl,
                QuizTakerPasscode = quizTakerPasscode
            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult TakeQuiz()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                // If the session is not set, redirect to the Login page
                return RedirectToAction("Login", "Home");
            }

            return View("EnterPasscode");  // Render the view to enter passcode
        }

        [HttpPost]
        public IActionResult ValidatePasscode(int quizId, string passcode)
        {
            // Retrieve the quiz from the database
            var quiz = _context.Quizes.Include(q => q.Questions).FirstOrDefault(q => q.QuizId == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            // Validate passcode
            if (quiz.Passcode != passcode)
            {
                // If the passcode is incorrect, set an error message
                ModelState.AddModelError("Passcode", "Incorrect passcode. Please try again.");

                // Show the passcode entry view again with an error message
                ViewBag.QuizId = quizId;
                return View("EnterPasscode");
            }

            // Passcode is correct, show the quiz questions
            var viewModel = new TakeQuizViewModel
            {
                Quiz = quiz
            };

            return View("TakeQuiz", viewModel);  // Render the quiz view
        }



        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, string participantName, Dictionary<int, int> selectedAnswers)
        {
            var quiz = _context.Quizes.Include(q => q.Questions).FirstOrDefault(q => q.QuizId == quizId);
            if (quiz == null)
            {
                return NotFound();
            }

            // Save the participant's answers and calculate the score
            var participant = new Participant
            {
                Name = participantName,
                QuizId = quizId,
                Marks = new List<Mark>()
            };

            foreach (var answer in selectedAnswers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.QuestionId == answer.Key);
                if (question != null)
                {
                    var mark = new Mark
                    {
                        QuestionId = question.QuestionId,
                        Score = question.CorrectOption == answer.Value ? 1 : 0  // Compare the selected answer with the correct one
                    };
                    participant.Marks.Add(mark);
                }
            }

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            // Redirect to the result page
            return RedirectToAction("QuizResult", new { participantId = participant.ParticipantId });
        }


        public IActionResult QuizResult(int participantId)
        {
            var participant = _context.Participants.Include(p => p.Marks).FirstOrDefault(p => p.ParticipantId == participantId);
            if (participant == null)
            {
                return NotFound();
            }

            // Calculate the total score
            int totalScore = participant.Marks.Sum(m => m.Score);

            return View(new QuizResultViewModel
            {
                Participant = participant,
                TotalScore = totalScore
            });
        }


        [HttpPost]
        public IActionResult QuizTakerDashboard(int quizId)
        {
            var quiz = _context.Quizes.Include(q => q.Questions).FirstOrDefault(q => q.QuizId == quizId);

            if (quiz == null)
            {
                return NotFound();
            }
            // Retrieve the list of participants and their results
            var participants = _context.Participants
                .Where(p => p.QuizId == quiz.QuizId)
                .Select(p => new ParticipantResultViewModel
                {
                    ParticipantName = p.Name,
                    TotalScore = p.Marks.Sum(m => m.Score),  // Assuming Score holds the points for the question
                    Marks = p.Marks.ToList()  // List of marks for each question
                }).ToList();

            // Create the dashboard view model with quiz data
            var dashboardViewModel = new QuizTakerDashboardViewModel
            {
                Quiz = quiz,
                TotalQuestions = quiz.Questions.Count,
                IsCompleted = quiz.IsCompleted,
                TotalParticipants = participants.Count,
                AverageScore = participants.Any() ? participants.Average(p => p.TotalScore) : (double?)null,
                Participants = participants
            };

            return View("QuizTakerDashboard", dashboardViewModel);
        }


        private string GeneratePasscode()
        {
            // Generate a random 4-digit passcode
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }
    }
}
