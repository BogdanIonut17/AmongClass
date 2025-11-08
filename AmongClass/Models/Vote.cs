using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Vote
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AnswerId { get; set; }
        public Answer Answer { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public DateTime VotedAt { get; set; }
    }
}