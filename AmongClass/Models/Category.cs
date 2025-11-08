using System.ComponentModel.DataAnnotations;

namespace AmongClass.Models
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
