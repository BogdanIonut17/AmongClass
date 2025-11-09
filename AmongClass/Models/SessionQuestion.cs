using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class SessionQuestion
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }
        public Session Session { get; set; }

        public Guid QuestionId { get; set; }
        public Question Question { get; set; }

        // Ordinea întrebării în sesiune
        public int Order { get; set; }

        // Timpul alocat pentru întrebare (în secunde)
        public int TimeLimit { get; set; } = 60;

        // Când a fost activată întrebarea
        public DateTime? ActivatedAt { get; set; }

        // Când s-a încheiat întrebarea
        public DateTime? CompletedAt { get; set; }
    }
}