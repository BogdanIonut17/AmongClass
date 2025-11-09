using AmongClass.Models;
using Microsoft.AspNetCore.Identity;
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
        public DbSet<SessionQuestion> SessionQuestions { get; set; }
        public DbSet<SessionStudent> SessionStudents { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Score>()
        .HasOne(s => s.Student)
        .WithMany()
        .HasForeignKey(s => s.StudentId)
        .OnDelete(DeleteBehavior.Restrict);

            // Relația 2: Score <-> Session
            builder.Entity<Score>()
                .HasOne(s => s.Session)
                .WithMany(sess => sess.Scores)
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relația 3: SessionStudent <-> Student (ApplicationUser)
            builder.Entity<SessionStudent>()
                .HasOne(ss => ss.Student)
                .WithMany()
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relația 4: SessionStudent <-> Session (A fost omisă anterior, am adăugat-o)
            builder.Entity<SessionStudent>()
                .HasOne(ss => ss.Session)
                .WithMany(s => s.SessionStudents) // Presupunând că Session are ICollection<SessionStudent> SessionStudents
                .HasForeignKey(ss => ss.SessionId)
                .OnDelete(DeleteBehavior.Cascade);


            // Relația 5: Answer <-> User (ApplicationUser)
            builder.Entity<Answer>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relația 6: SessionQuestion <-> Question
            builder.Entity<SessionQuestion>()
                .HasOne(sq => sq.Question)
                .WithMany(q => q.SessionQuestions)
                .HasForeignKey(sq => sq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Nu ștergem întrebările când ștergem SessionQuestion

            // Relația 7: SessionQuestion <-> Session (A fost omisă anterior, am adăugat-o)
            builder.Entity<SessionQuestion>()
                .HasOne(sq => sq.Session)
                .WithMany(s => s.SessionQuestions) // Presupunând că Session are ICollection<SessionQuestion> SessionQuestions
                .HasForeignKey(sq => sq.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
