using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class SessionStudent
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Session Session { get; set; }

        public string StudentId { get; set; }
        public IdentityUser Student { get; set; }
    }
}
