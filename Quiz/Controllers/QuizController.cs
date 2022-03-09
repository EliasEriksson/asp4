using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Quiz.Data;
using Quiz.Models;

namespace Quiz.Controllers
{
    public class QuizController : Controller
    {
        public class QuizWithAvg : Models.Quiz
        {
            public double AvgPercentCompleted { get; set; }

            public QuizWithAvg(Guid id, string? name, string? lyric, int time, double avgPercentCompleted)
            {
                this.Id = id;
                this.Name = name;
                this.Lyric = lyric;
                this.TimeLimitSec = time;
                this.AvgPercentCompleted = avgPercentCompleted;
            }
        }

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
            var intermediateModel = _context.QuizResults.GroupBy(
                r => r.QuizId, p => p.PercentCompleted
            ).Select(g => new
            {
                QuizId = g.Key,
                AvgPercentCompleted = g.Average()
            });
            var completedQuizzes = await _context.Quizzes.Join(
                intermediateModel,
                quiz => quiz.Id,
                intermediate => intermediate.QuizId,
                (quiz, intermediate) => new QuizWithAvg(
                    quiz.Id, quiz.Name, quiz.Lyric, quiz.TimeLimitSec, intermediate.AvgPercentCompleted
                )
            ).ToListAsync();

            var uncompletedQuizzes = await _context.Quizzes
                .Where(quiz => !_context.QuizResults
                    .Select(quizResult => quizResult.QuizId).Contains(quiz.Id))
                .Select(q => new QuizWithAvg(q.Id, q.Name, q.Lyric, q.TimeLimitSec, -1))
                .ToListAsync();

            var data = completedQuizzes.Concat(uncompletedQuizzes).OrderBy(quiz => quiz.Name).ToList();

            return View(data);
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
                return RedirectToAction("Details", "Quiz", new {id = quiz.Id});
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
        [Authorize]
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

                return this.View("Details", quiz);
            }

            return View(quiz);
        }

        // GET: Quiz/Delete/5
        [Authorize]
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
        [Authorize]
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