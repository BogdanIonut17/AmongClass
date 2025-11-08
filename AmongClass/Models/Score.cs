using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Score
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }
        public Guid StudentId { get; set; }

        public int Points { get; set; }

        public Session Session { get; set; }
        public IdentityUser<Guid> Student { get; set; }
    }

}
