using Microsoft.AspNetCore.Mvc;

namespace InstaQuiz.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
