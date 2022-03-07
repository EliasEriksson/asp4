using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Quiz.Data;

namespace Quiz.Controllers
{
    public class RootController : Controller
    {
        private readonly QuizContext _context;

        public RootController(QuizContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            Console.WriteLine(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return this.View();
        }
    }
}