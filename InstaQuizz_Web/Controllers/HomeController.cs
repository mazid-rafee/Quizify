using InstaQuiz.Data;
using InstaQuiz.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaQuiz.Controllers
{
    public class HomeController : Controller
    {

        private readonly InstaQuizContext _context;
        private readonly MLService _mlService;

        public HomeController(InstaQuizContext context, MLService mlService)
        {
            _context = context;
            _mlService = mlService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Dummy users for validation
            var users = new List<(string email, string password, string userName)>
            {
                ("mazid.rafee@instaquiz.com", "abcd", "Rafee"),
                ("zaima.zarnaz@instaquiz.com", "abcd", "Zaima"),
                ("john.doe@instaquiz.com", "abcd", "John")
            };

            // Check if the entered email and password match a dummy user
            var user = users.FirstOrDefault(u => u.email == email && u.password == password);

            if (user != default)
            {
                // Save the user's name and email to the session
                HttpContext.Session.SetString("UserName", user.userName);
                HttpContext.Session.SetString("UserEmail", user.email);

                // Redirect to the dashboard page if login is successful
                return RedirectToAction("Index");
            }

            // If the login fails, show an error message and return to the login page
            ViewBag.ErrorMessage = "Invalid email or password. Please try again.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Optionally clear session or other user-specific data
            HttpContext.Session.Clear();

            // Redirect to the login page or home page after logout
            return RedirectToAction("Login", "Home");
        }

        // GET: /Account/Dashboard
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            // Retrieve the logged-in user's email from the session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userEmail))
            {
                // If no session is found, redirect to the login page
                return RedirectToAction("Login", "Home");
            }

            // Get the quizzes created by the logged-in user
            var generatedQuizzes = _context.Quizes
                .Where(q => q.Owner == userEmail)
                .ToList();

            // Get the quizzes the user has taken (you need to adjust this based on your data model)
            var takenQuizzes = _context.Participants
                .Where(p => p.Name == userName) // Assuming 'Name' field represents user who took the quiz
                .Select(p => p.Quiz)
                .ToList();

            // Pass the data to the view using ViewBag or a ViewModel
            ViewBag.GeneratedQuizzes = generatedQuizzes;
            ViewBag.TakenQuizzes = takenQuizzes;

            return View();
        }
    }
}