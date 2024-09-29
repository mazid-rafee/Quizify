using Microsoft.EntityFrameworkCore;
using InstaQuiz.Models;

namespace InstaQuiz.Data
{
    public class InstaQuizContext : DbContext
    {
        public InstaQuizContext(DbContextOptions<InstaQuizContext> options) : base(options) { }

        public DbSet<Quiz> Quizes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Mark> Marks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the relationship between Marks and Questions to not use cascade delete
            modelBuilder.Entity<Mark>()
                .HasOne(m => m.Question)
                .WithMany()
                .HasForeignKey(m => m.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete here

            // Add other entity configurations as needed
        }
    }
}
