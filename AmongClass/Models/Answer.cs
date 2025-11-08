gusing Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Answer
    {
        [Key]
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public Guid UserId { get; set; }
        public IdentityUser User { get; set; }
        public int Points { get; set; }
        public ICollection<Vote> Votes { get; set; }
    }
}