using EkoaGym.Domain.Constants;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EkoaGym.Infrastructure.Persistence;

public class EkoaGymDbContext(DbContextOptions<EkoaGymDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<WorkoutPlanExercise> WorkoutPlanExercises => Set<WorkoutPlanExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<SessionSetLog> SessionSetLogs => Set<SessionSetLog>();
    public DbSet<BodyMetricLog> BodyMetricLogs => Set<BodyMetricLog>();
    public DbSet<ProgressPhoto> ProgressPhotos => Set<ProgressPhoto>();
    public DbSet<LeagueRoom> LeagueRooms => Set<LeagueRoom>();
    public DbSet<LeagueMembership> LeagueMemberships => Set<LeagueMembership>();
    public DbSet<SessionScore> SessionScores => Set<SessionScore>();
    public DbSet<ExerciseLimit> ExerciseLimits => Set<ExerciseLimit>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<FeedReaction> FeedReactions => Set<FeedReaction>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // All our DateTime values are UTC by convention (DateTime.UtcNow); SQL Server's datetime2 has no
        // timezone info, so EF Core reads them back as Kind=Unspecified, which serializes without the "Z"
        // suffix. Force Kind=Utc on read so JSON output is consistent whether the value came fresh from
        // memory or was reloaded from the database.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.PreferredLanguage).IsRequired().HasMaxLength(10).HasDefaultValue(SupportedLanguages.Default);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.TokenHash).IsRequired();
            entity.HasIndex(rt => rt.TokenHash).IsUnique();
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TokenHash).IsRequired();
            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Sex).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.ActivityLevel).HasConversion<string>().HasMaxLength(30);
            entity.Property(p => p.CurrentWeightKg).HasColumnType("decimal(6,2)");
            entity.HasIndex(p => p.UserId).IsUnique();
            entity.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Type).IsRequired().HasMaxLength(100);
            entity.Property(g => g.TargetValue).HasColumnType("decimal(10,2)");
            entity.HasIndex(g => g.UserId);
            entity.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MuscleGroup).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Equipment).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Difficulty).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.InstructionText).IsRequired();
            entity.Property(e => e.IntensityFactor).HasColumnType("decimal(3,2)");
            entity.HasIndex(e => e.MuscleGroup);
            entity.HasIndex(e => e.Equipment);
            entity.HasIndex(e => e.Difficulty);
            entity.HasIndex(e => e.Name);
            entity.HasData(ExerciseSeedData.All);
        });

        modelBuilder.Entity<WorkoutPlan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(p => p.UserId);
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkoutPlanExercise>(entity =>
        {
            entity.HasKey(pe => pe.Id);
            entity.Property(pe => pe.WeightKg).HasColumnType("decimal(6,2)");
            entity.HasIndex(pe => pe.PlanId);
            entity.HasOne(pe => pe.Plan)
                .WithMany()
                .HasForeignKey(pe => pe.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pe => pe.Exercise)
                .WithMany()
                .HasForeignKey(pe => pe.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.UserId, s.StartedAt });
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Restrict (not Cascade) to avoid a second cascade path alongside the direct User -> WorkoutSession FK above.
            entity.HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SessionSetLog>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.ActualWeightKg).HasColumnType("decimal(6,2)");
            entity.HasIndex(s => new { s.SessionId, s.ExerciseId });
            entity.HasOne(s => s.Session)
                .WithMany()
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Exercise)
                .WithMany()
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(s => s.VerificationStatus).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<BodyMetricLog>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.WeightKg).HasColumnType("decimal(6,2)");
            entity.Property(b => b.WaistCm).HasColumnType("decimal(6,2)");
            entity.Property(b => b.ChestCm).HasColumnType("decimal(6,2)");
            entity.Property(b => b.ArmsCm).HasColumnType("decimal(6,2)");
            entity.HasIndex(b => new { b.UserId, b.Date });
            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgressPhoto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.ImageUrl).IsRequired();
            entity.Property(p => p.ContentType).IsRequired().HasMaxLength(100);
            entity.HasIndex(p => new { p.UserId, p.Date });
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeagueRoom>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Tier).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(r => new { r.Tier, r.WeekStartDate });
        });

        modelBuilder.Entity<LeagueMembership>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Points).HasColumnType("decimal(10,2)");
            entity.Property(m => m.MovementSincePreviousWeek).HasConversion<string>().HasMaxLength(20);
            // One membership per user per room; a user's memberships across different weeks are separate rows.
            entity.HasIndex(m => new { m.RoomId, m.UserId }).IsUnique();
            entity.HasIndex(m => m.UserId);
            entity.HasOne(m => m.Room)
                .WithMany()
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionScore>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TotalScore).HasColumnType("decimal(10,2)");
            entity.Property(s => s.ConsistencyBonus).HasColumnType("decimal(10,2)");
            entity.Property(s => s.PrBonus).HasColumnType("decimal(10,2)");
            entity.Property(s => s.BreakdownJson).IsRequired();
            entity.HasIndex(s => s.SessionId).IsUnique();
            entity.HasOne(s => s.Session)
                .WithMany()
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExerciseLimit>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.MaxWeightKg).HasColumnType("decimal(6,2)");
            entity.HasIndex(l => l.ExerciseId).IsUnique();
            entity.HasOne(l => l.Exercise)
                .WithMany()
                .HasForeignKey(l => l.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(ExerciseLimitSeedData.All);
        });

        modelBuilder.Entity<VerificationRequest>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.ImageUrl).IsRequired();
            entity.Property(v => v.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(v => v.SessionSetLogId).IsUnique();
            entity.HasOne(v => v.SessionSetLog)
                .WithMany()
                .HasForeignKey(v => v.SessionSetLogId)
                .OnDelete(DeleteBehavior.Cascade);
            // Restrict — avoids a second cascade path alongside User -> WorkoutSession -> SessionSetLog -> VerificationRequest.
            entity.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Reason).HasMaxLength(500);
            entity.HasIndex(r => new { r.ReporterId, r.CreatedAt });
            entity.HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Cascade);
            // Restrict — a Report row already cascades via ReporterId; a second Cascade path via
            // TargetUserId would hit the same "multiple cascade paths" SQL Server error.
            entity.HasOne(r => r.TargetUser)
                .WithMany()
                .HasForeignKey(r => r.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeedReaction>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new { f.SessionId, f.UserId }).IsUnique();
            entity.HasOne(f => f.Session)
                .WithMany()
                .HasForeignKey(f => f.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            // Restrict — avoids a second cascade path alongside User -> WorkoutSession -> FeedReaction.
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

file class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v,
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

file class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    v => v,
    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
