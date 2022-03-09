#nullable disable
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz.Data;

namespace Quiz.Controllers
{
    public class QuizController : Controller
    {
        private static string LyricFormatter(string lyric)
        {
            lyric = Regex.Replace(lyric!, @"(?:\([^)]+\))|\.|!|\?|,|-|;", "");
            lyric = Regex.Replace(lyric!, @"[\s\n]+", " ");
            return lyric;
        }

        private readonly QuizContext _context;

        public QuizController(QuizContext context)
        {
            _context = context;
        }

        // GET: Quiz
        public async Task<IActionResult> Index()
        {
            return View(await _context.Quizzes.ToListAsync());
        }

        // GET: Quiz/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        public async Task<IActionResult> Results(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quizResult = await this._context.QuizResults.Include(
                r => r.Quiz
            ).Include(
                r => r.MissingWords
            ).FirstOrDefaultAsync(
                r => r.Id == id
            );
            if (quizResult == null)
            {
                return NotFound();
            }

            return this.View(quizResult);
        }

        // GET: Quiz/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quiz/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,Lyric,TimeLimitSec")] Models.Quiz quiz)
        {
            this.ModelState.Clear();
            quiz.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (this.TryValidateModel(quiz))
            {
                quiz.Id = Guid.NewGuid();
                quiz.Lyric = LyricFormatter(quiz.Lyric);
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(quiz);
        }

        // GET: Quiz/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quiz/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Lyric,TimeLimitSec,UserId")] Models.Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            ModelState.Clear();
            quiz.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (this.TryValidateModel(quiz))
            {
                quiz.Lyric = LyricFormatter(quiz.Lyric);
                try
                {
                    _context.Update(quiz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(quiz);
        }

        // GET: Quiz/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quiz/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(Guid id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}