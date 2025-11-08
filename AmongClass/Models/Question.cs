using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Question
    {
        [Key]
        public Guid Id { get; set; }
        public string Text { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}
