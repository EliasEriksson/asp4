using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /**
     * model for a missing word
     *
     * related to a specific quiz result.
     */
    public class MissingWord
    {
        public int Id { get; set; }
        
        [Required]
        public string? Word { get; set; }
        
        [Required]
        public Guid QuizResultId { get; set; }
        public QuizResult? QuizResult { get; set; }

        public MissingWord(string word, string quizResultId)
        {
            Word = word;
            QuizResultId = Guid.Parse(quizResultId);
        }

        public MissingWord(string word, Guid quizResultId)
        {
            Word = word;
            QuizResultId = quizResultId;
        }
    }
}

