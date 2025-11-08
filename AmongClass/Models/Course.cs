using Microsoft.AspNetCore.Identity;

namespace AmongClass.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid TeacherId { get; set; }
        public IdentityUser Teacher { get; set; }

    }
}
