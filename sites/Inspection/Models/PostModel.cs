using Microsoft.AspNetCore.Identity;

namespace Inspection.Models
{
    public class PostModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        
        public IdentityUser User { get; set; }
    }
}

