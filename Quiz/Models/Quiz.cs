using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quiz.Models
{
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