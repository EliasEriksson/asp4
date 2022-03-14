using Microsoft.AspNetCore.Identity;

namespace Quiz.Models
{
    /**
     * Model for the application user.
     *
     * used to create relations between the identity user and other models.
     */
    public class ApplicationUser : IdentityUser
    {
        public List<Quiz>? Quizzes { get; set; }
    }
}