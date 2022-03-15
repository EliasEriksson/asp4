using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quiz.Models
{
    /**
     * Model representing a quiz
     *
     * contains the entire lyrics for a song.
     * the lyrics stored is assumed to be stripped from punctuation and
     * weird characters except the english contraction character '
     *
     * related to the user that submitted the Quiz results for this quiz.
     */
    public class Quiz
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required] public string? Name { get; set; }

        [Required] public string? Lyric { get; set; }

        [Required]
        [Display(Name = "Time Limit (seconds)")]
        [Range(0, int.MaxValue)]
        public int TimeLimitSec { get; set; }

        [Required] public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public List<QuizResult>? Results { get; set; }
    }
}