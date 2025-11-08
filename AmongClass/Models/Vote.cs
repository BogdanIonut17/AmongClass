using Microsoft.AspNetCore.Identity;

namespace AmongClass.Models
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Guid VoterId { get; set; }
        public required IdentityUser Voter { get; set; }
        public Guid AnswerId { get; set; }
        public Answer Answer { get; set; }
        public bool IsAI { get; set; }

    }
}
