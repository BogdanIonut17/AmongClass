using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class SessionStudent
    {
        [Key]
        public Guid SessionId { get; set; }
        public Session Session { get; set; }

        public Guid StudentId { get; set; }
        public IdentityUser<Guid> Student { get; set; }
    }
}
