using Microsoft.AspNetCore.Identity;

namespace AmongClass.Models
{
    public class SessionStudent
    {
        public Guid SessionId { get; set; }
        public Session Session { get; set; }

        public Guid StudentId { get; set; }
        public IdentityUser<Guid> Student { get; set; }
    }
}
