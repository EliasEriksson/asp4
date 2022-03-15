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
        /**
         * extends the quiz object with an average percent completed field.
         *
         * required since the query in the Index action calculates the average.
         */
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

        /**
         * helper method to format the posted lyrics
         */
        private static string LyricFormatter(string lyric)
        {
            lyric = Regex.Replace(lyric!, @"(?:\([^)]+\))|\.|!|\?|,|-|;|(?:\[[^)]+\])|", "");
            lyric = Regex.Replace(lyric!, @"[\s\n]+", " ");
            return lyric;
        }

        private readonly QuizContext _context;

        public QuizController(QuizContext context)
        {
            _context = context;
        }

        /**
         * serves the list of quizzes
         *
         * if search is set a search is preformed over the quiz name
         */
        public async Task<IActionResult> Index(string? search)
        {
            // calculates the avg. % completed for each quiz as a intermediate query
            // not run against the database yet
            var intermediateModel = _context.QuizResults.GroupBy(
                r => r.QuizId, p => p.PercentCompleted
            ).Select(g => new
            {
                QuizId = g.Key,
                AvgPercentCompleted = g.Average()
            });

            // variable to hold the query
            IQueryable<Models.Quiz> query;
            if (!string.IsNullOrEmpty(search))
            {
                // initialize the query variable with a search
                query = _context.Quizzes.Where(quiz => quiz.Name!.ToLower().Contains(search.ToLower()));
                ViewBag.Search = true;
            }
            else
            {
                // initialize a query with no search
                query = _context.Quizzes;
                ViewBag.Search = false;
            }
            // apply remaining part of query
            // join the intermediate query with the quiz table
            var completedQuizzes = query.Join(
                intermediateModel,
                quiz => quiz.Id,
                intermediate => intermediate.QuizId,
                (quiz, intermediate) => new QuizWithAvg(
                    quiz.Id, quiz.Name, quiz.Lyric, quiz.TimeLimitSec, intermediate.AvgPercentCompleted
                )
            );
            
            // query the quiz result table for all rows that does not relate to anything in the intermediate query
            // this will be all quizzes that are not completed yet.
            // have to do it this way to avoid divide by 0 and who knows how to do a left join in this rando query language?
            // its not paged anyway so it works
            var uncompletedQuizzes = query
                .Where(quiz => !_context.QuizResults
                    .Select(quizResult => quizResult.QuizId).Contains(quiz.Id))
                .Select(q => new QuizWithAvg(q.Id, q.Name, q.Lyric, q.TimeLimitSec, -1));
            
            // combine completed and uncompleted quizzes and sort by name
            var data = (await completedQuizzes.ToListAsync()).Concat(await uncompletedQuizzes.ToListAsync()).OrderBy(quiz => quiz.Name).ToList();

            return View(data);
        }

        /**
         * serves the details page of a specific quiz if it exists
         */
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

        /**
         * serves the result page of a specific attempt if it exists
         */
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

        /**
         * creates a new quiz and adds it to the database
         */
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

        /**
         * serves the edit page of a quiz if it exists
         */
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

        /**
         * edits the quiz if it exists and data is submitted correctly
         */
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

        /**
         * serves the delete page if it exists
         */
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

        /**
         * deletes a specific quiz if it exists
         */
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