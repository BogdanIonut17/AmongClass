using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid TeacherId { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public IdentityUser Teacher { get; set; }

    }
}
