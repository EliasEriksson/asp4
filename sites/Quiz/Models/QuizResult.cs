using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quiz.Models
{
    /**
     * Quiz result model
     *
     * related a specific quiz.
     * relates to all words missing from the users guesses.
     */
    public class QuizResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public double PercentCompleted { get; set; }

        [Required]
        public int Time { get; set; }
        
        [Required]
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public List<MissingWord>? MissingWords { get; set; }

        public QuizResult(double percentCompleted, Guid quizId, int time)
        {
            Id = Guid.NewGuid();
            PercentCompleted = percentCompleted;
            QuizId = quizId;
            Time = time;
        }

        public QuizResult(double percentCompleted, string quizId, int time)
        {
            Id = Guid.NewGuid();
            PercentCompleted = percentCompleted;
            QuizId = Guid.Parse(quizId);
            Time = time;
        }
    }
}