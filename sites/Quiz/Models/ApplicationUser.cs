using Microsoft.AspNetCore.Identity;

namespace Quiz.Models {
    public class ApplicationUser : IdentityUser
    {
        public List<Quiz>? Quizzes { get; set; }
    }
}

