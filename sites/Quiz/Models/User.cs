using Microsoft.AspNetCore.Identity;

namespace Quiz.Models {
    public class User : IdentityUser
    {
        public List<Quiz>? Quizzes { get; set; }
    }
}

