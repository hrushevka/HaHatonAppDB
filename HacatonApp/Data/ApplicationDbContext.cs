using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HacatonApp.Models;

namespace HacatonApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
             : base(options) { }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<JuryZaiavka> JuryZaiavkas { get; set; }
        public DbSet<Criteria> Criterias { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Настройка ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.LastName)
                    .HasMaxLength(50);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                // Связь: User -> Team (внешний ключ TeamID)
                entity.HasOne<Team>()
                    .WithMany()
                    .HasForeignKey(u => u.TeamID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Настройка Team
            builder.Entity<Team>(entity =>
            {
                entity.ToTable("Teams");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(t => t.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                // Связь: Team -> Project (один к одному)
                entity.HasOne<Project>()
                    .WithOne()
                    .HasForeignKey<Team>(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка Project
            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Description)
                    .IsRequired();

                entity.Property(p => p.Criteriars)
                    .HasMaxLength(500);

                entity.Property(p => p.Score)
                    .HasDefaultValue(0);
            });

            // Настройка JuryZaiavka
            builder.Entity<JuryZaiavka>(entity =>
            {
                entity.ToTable("JuryZaiavkas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(j => j.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(j => j.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(j => j.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(j => j.Motivation)
                    .HasMaxLength(1000);

                entity.Property(j => j.AdminComment)
                    .HasMaxLength(1000);

                entity.Property(j => j.Status)
                    .HasDefaultValue(JuryZaiavkaStatus.Wait);

                entity.Property(j => j.SubmitedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Связь с пользователем
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(j => j.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Criteria
            builder.Entity<Criteria>(entity =>
            {
                entity.ToTable("Criterias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.Weight)
                    .HasPrecision(5, 2);
            });
        }
    }
}