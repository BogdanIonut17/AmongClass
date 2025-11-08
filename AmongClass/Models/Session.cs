using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Session
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid TeacherId { get; set; }

        public IdentityUser<Guid> Teacher { get; set; }

        public ICollection<Score> Scores { get; set; } = new List<Score>();
    }

}
