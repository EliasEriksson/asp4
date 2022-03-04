using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quiz.Models
{
    public class QuizResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        [Required]
        public double PercentCompleted { get; set; }

        [Required]
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }


        public QuizResult(double percentCompleted, Guid quizId)
        {
            Id = Guid.NewGuid();
            PercentCompleted = percentCompleted;
            QuizId = quizId;
        }

        public QuizResult(double percentCompleted, string quizId)
        {
            Id = Guid.NewGuid();
            PercentCompleted = percentCompleted;
            QuizId = Guid.Parse(quizId);
        }
    }
}