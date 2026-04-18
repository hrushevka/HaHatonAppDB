using HacatonApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HacatonApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<JuryZaiavka> JuryZaiavkas { get; set; }
        public DbSet<Criteria> Criterias { get; set; }
        public DbSet<TeamZaiavka> TeamZaiavkas { get; set; }
        public DbSet<ProjectReview> ProjectReviews { get; set; }
        public DbSet<ProjectCriterionScore> ProjectCriterionScores { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(user => user.LastName)
                    .HasMaxLength(50);

                entity.Property(user => user.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne<Team>()
                    .WithMany()
                    .HasForeignKey(user => user.TeamID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Team>(entity =>
            {
                entity.ToTable("Teams");
                entity.HasKey(team => team.Id);
                entity.Property(team => team.Id).ValueGeneratedOnAdd();

                entity.Property(team => team.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(team => team.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne<Project>()
                    .WithOne()
                    .HasForeignKey<Team>(team => team.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(project => project.Id);
                entity.Property(project => project.Id).ValueGeneratedOnAdd();

                entity.Property(project => project.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(project => project.Description)
                    .IsRequired();

                entity.Property(project => project.Criteriars)
                    .HasMaxLength(500);

                entity.Property(project => project.Score)
                    .HasDefaultValue(0f);
            });

            builder.Entity<JuryZaiavka>(entity =>
            {
                entity.ToTable("JuryZaiavkas");
                entity.HasKey(zaiavka => zaiavka.Id);
                entity.Property(zaiavka => zaiavka.Id).ValueGeneratedOnAdd();

                entity.Property(zaiavka => zaiavka.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(zaiavka => zaiavka.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(zaiavka => zaiavka.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(zaiavka => zaiavka.Motivation)
                    .HasMaxLength(1000);

                entity.Property(zaiavka => zaiavka.AdminComment)
                    .HasMaxLength(1000);

                entity.Property(zaiavka => zaiavka.Status)
                    .HasDefaultValue("Wait");

                entity.Property(zaiavka => zaiavka.SubmitedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(zaiavka => zaiavka.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Criteria>(entity =>
            {
                entity.ToTable("Criterias");
                entity.HasKey(criteria => criteria.Id);
                entity.Property(criteria => criteria.Id).ValueGeneratedOnAdd();

                entity.Property(criteria => criteria.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(criteria => criteria.Weight)
                    .HasDefaultValue(1f);

                entity.Property(criteria => criteria.MaxScore)
                    .HasDefaultValue(10f);
            });

            builder.Entity<TeamZaiavka>(entity =>
            {
                entity.ToTable("TeamZaiavkas");
                entity.HasKey(zaiavka => zaiavka.Id);
                entity.Property(zaiavka => zaiavka.Id).ValueGeneratedOnAdd();

                entity.Property(zaiavka => zaiavka.TeamName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(zaiavka => zaiavka.ProjectName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(zaiavka => zaiavka.ProjectDescription)
                    .IsRequired();

                entity.Property(zaiavka => zaiavka.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(zaiavka => zaiavka.Motivation)
                    .HasMaxLength(1000);

                entity.Property(zaiavka => zaiavka.Status)
                    .HasDefaultValue("Wait");

                entity.Property(zaiavka => zaiavka.SubmitedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(zaiavka => zaiavka.AdminComment)
                    .HasMaxLength(1000);

                entity.Property(zaiavka => zaiavka.TeamMemberIds)
                    .HasConversion(
                        value => string.Join(',', value ?? new List<string>()),
                        value => string.IsNullOrWhiteSpace(value)
                            ? new List<string>()
                            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (left, right) => (left ?? new List<string>()).SequenceEqual(right ?? new List<string>()),
                        value => value == null
                            ? 0
                            : value.Aggregate(0, (current, item) => HashCode.Combine(current, item.GetHashCode())),
                        value => value == null ? new List<string>() : value.ToList()));
            });

            builder.Entity<ProjectReview>(entity =>
            {
                entity.ToTable("ProjectReviews");
                entity.HasKey(review => review.Id);
                entity.Property(review => review.Id).ValueGeneratedOnAdd();

                entity.Property(review => review.Comment)
                    .HasMaxLength(2000);

                entity.Property(review => review.TotalScore)
                    .HasDefaultValue(0f);

                entity.Property(review => review.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(review => review.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(review => new { review.ProjectId, review.JuryUserId })
                    .IsUnique();

                entity.HasOne<Project>()
                    .WithMany()
                    .HasForeignKey(review => review.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(review => review.JuryUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProjectCriterionScore>(entity =>
            {
                entity.ToTable("ProjectCriterionScores");
                entity.HasKey(score => score.Id);
                entity.Property(score => score.Id).ValueGeneratedOnAdd();

                entity.Property(score => score.Score)
                    .HasDefaultValue(0f);

                entity.HasIndex(score => new { score.ProjectReviewId, score.CriteriaId })
                    .IsUnique();

                entity.HasOne<ProjectReview>()
                    .WithMany()
                    .HasForeignKey(score => score.ProjectReviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Criteria>()
                    .WithMany()
                    .HasForeignKey(score => score.CriteriaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
