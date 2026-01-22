using Domain.Entities.Users;
using Domain.Entities.Education;
using Domain.Entities.Testing;
using Domain.Entities.Monetization;
using Domain.Entities.Gamification;
using Domain.Entities.CMS;
using Domain.Entities.Reference;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<TestSession> TestSessions { get; set; }
    public DbSet<TestTemplate> TestTemplates { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<DuelMatch> DuelMatches { get; set; }
    public DbSet<League> Leagues { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<NewsItem> NewsItems { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<University> Universities { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Major> Majors { get; set; }
    public DbSet<ClusterDefinition> ClusterDefinitions { get; set; }
    public DbSet<RedListQuestion> RedListQuestions { get; set; }
    public DbSet<UserLoginActivity> UserLoginActivities { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentityTables(modelBuilder);
        ConfigureUserEntities(modelBuilder);
        ConfigureEducationEntities(modelBuilder);
        ConfigureTestingEntities(modelBuilder);
        ConfigureMonetizationEntities(modelBuilder);
        ConfigureGamificationEntities(modelBuilder);
        ConfigureCmsEntities(modelBuilder);
        ConfigureReferenceEntities(modelBuilder);
    }

    private void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
    }

    private void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.XP).HasDefaultValue(0);
            entity.HasIndex(p => p.UserId).IsUnique();
        });

        modelBuilder.Entity<UserLoginActivity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.UserId, a.LoginDate });
            entity.Property(a => a.LoginDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private void ConfigureEducationEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.Subject)
                .WithMany(s => s.Topics)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.HasOne(q => q.Subject)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(q => q.Topic).HasMaxLength(200);
            entity.Property(q => q.Content).IsRequired();
            entity.Property(q => q.Explanation).IsRequired();
            entity.Property(q => q.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(a => a.Text).IsRequired();
        });
    }

    private void ConfigureTestingEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestSession>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.User)
                .WithMany(u => u.TestSessions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Template)
                .WithMany(tt => tt.TestSessions)
                .HasForeignKey(t => t.TestTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(t => t.StartedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(t => t.TotalScore).HasDefaultValue(0);
        });

        modelBuilder.Entity<TestTemplate>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.SubjectDistributionJson).IsRequired();
            entity.HasIndex(t => t.ClusterNumber);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(ua => ua.Id);
            entity.HasOne(ua => ua.TestSession)
                .WithMany(ts => ts.Answers)
                .HasForeignKey(ua => ua.TestSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ua => ua.ChosenAnswer)
                .WithMany()
                .HasForeignKey(ua => ua.ChosenAnswerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(ua => ua.TimeSpentSeconds).HasDefaultValue(0);
        });

        modelBuilder.Entity<RedListQuestion>(entity =>
        {
            entity.HasKey(rl => rl.Id);
            entity.HasOne(rl => rl.User)
                .WithMany()
                .HasForeignKey(rl => rl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rl => rl.Question)
                .WithMany()
                .HasForeignKey(rl => rl.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(rl => rl.ConsecutiveCorrectCount).HasDefaultValue(0);
            entity.Property(rl => rl.AddedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(rl => new { rl.UserId, rl.QuestionId }).IsUnique();
        });
    }

    private void ConfigureMonetizationEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(sp => sp.Id);
            entity.Property(sp => sp.Name).IsRequired().HasMaxLength(50);
            entity.Property(sp => sp.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(us => us.Id);
            entity.HasOne(us => us.User)
                .WithOne(u => u.Subscription)
                .HasForeignKey<UserSubscription>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(us => us.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(us => us.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(us => us.UserId).IsUnique();
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(pt => pt.Id);
            entity.HasOne(pt => pt.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(pt => pt.Amount).HasPrecision(10, 2);
            entity.Property(pt => pt.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(pt => pt.ExternalTransactionId);
        });
    }

    private void ConfigureGamificationEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DuelMatch>(entity =>
        {
            entity.HasKey(dm => dm.Id);
            
            entity.HasOne(dm => dm.Player1)
                .WithMany(u => u.DuelsAsPlayer1)
                .HasForeignKey(dm => dm.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dm => dm.Player2)
                .WithMany(u => u.DuelsAsPlayer2)
                .HasForeignKey(dm => dm.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dm => dm.Winner)
                .WithMany()
                .HasForeignKey(dm => dm.WinnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(dm => dm.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(dm => dm.Player1Score).HasDefaultValue(0);
            entity.Property(dm => dm.Player2Score).HasDefaultValue(0);
        });

        modelBuilder.Entity<League>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired();
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private void ConfigureCmsEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(tm => tm.Id);
            entity.Property(tm => tm.FullName).IsRequired().HasMaxLength(100);
            entity.Property(tm => tm.Role).IsRequired().HasMaxLength(100);
            entity.Property(tm => tm.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<NewsItem>(entity =>
        {
            entity.HasKey(ni => ni.Id);
            entity.Property(ni => ni.Title).IsRequired().HasMaxLength(200);
            entity.Property(ni => ni.Body).IsRequired();
            entity.Property(ni => ni.PublishedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(ni => ni.IsPublished).HasDefaultValue(false);
        });
    }

    private void ConfigureReferenceEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Province).IsRequired().HasMaxLength(50);
            entity.Property(s => s.District).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
            entity.Property(u => u.City).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasOne(f => f.University)
                .WithMany(u => u.Faculties)
                .HasForeignKey(f => f.UniversityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasOne(m => m.Faculty)
                .WithMany(f => f.Majors)
                .HasForeignKey(m => m.FacultyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
            entity.Property(m => m.MinScore2024).IsRequired();
            entity.Property(m => m.MinScore2025).IsRequired();
        });

        modelBuilder.Entity<ClusterDefinition>(entity =>
        {
            entity.HasKey(cd => cd.Id);
            entity.Property(cd => cd.ClusterNumber).IsRequired();
            entity.Property(cd => cd.Description).HasMaxLength(500);
            entity.Property(cd => cd.SubjectIdsJson).IsRequired();
        });
    }
}
