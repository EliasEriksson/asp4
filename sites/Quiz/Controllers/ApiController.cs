using Microsoft.AspNetCore.Mvc;
using Quiz.Data;
using Quiz.Models;

namespace Quiz.Controllers
{
    [Route("/api/quiz/")]
    public class ApiController : Controller
    {
        private readonly QuizContext _context;

        public ApiController(QuizContext context)
        {
            _context = context;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Quiz>> GetQuiz(string id)
        {
            var quiz = await this._context.Quizzes.FindAsync(Guid.Parse(id));
            return quiz == null ? NotFound() : quiz;
        }

        [HttpPost("result/")]
        public async Task<ActionResult<Models.QuizResult>> PostResult(double percentCompleted, string quizId)
        {
            var quizResult = new QuizResult(percentCompleted, quizId);
            this._context.QuizResults.Add(quizResult);
            await this._context.SaveChangesAsync();
            return CreatedAtAction("PostResult", quizResult);
        }
    }
}

