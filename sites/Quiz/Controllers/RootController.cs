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
            return this.View();
        }
    }
}

