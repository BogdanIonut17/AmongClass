using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Session
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string TeacherId { get; set; }

        // Cod unic pentru join (6 caractere)
        public string JoinCode { get; set; }

        // Status sesiune: Active, Inactive, Completed
        public SessionStatus Status { get; set; } = SessionStatus.Inactive;

        // ID-ul întrebării curente active
        public Guid? CurrentQuestionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public IdentityUser Teacher { get; set; }
        public ICollection<SessionStudent> SessionStudents { get; set; }
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<SessionQuestion> SessionQuestions { get; set; }
    }

    public enum SessionStatus
    {
        Inactive,
        Active,
        Completed
    }
}