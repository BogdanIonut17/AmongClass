using AmongClass.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AmongClass.Models.Category> Categories { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<AIResponse> AIResponses { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<SessionStudent> SessionStudents { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Score>()
        .HasOne(s => s.Student)
        .WithMany()
        .HasForeignKey(s => s.StudentId)
        .OnDelete(DeleteBehavior.Restrict); // sau DeleteBehavior.NoAction

            builder.Entity<Score>()
                .HasOne(s => s.Session)
                .WithMany(sess => sess.Scores)
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
