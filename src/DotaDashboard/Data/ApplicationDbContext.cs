using DotaDashboard.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotaDashboard.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Match> Matches { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Hero> Heroes { get; set; }
    public DbSet<MatchPlayer> MatchPlayers { get; set; }
    public DbSet<Job> Jobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Match entity
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.MatchId);
            entity.HasIndex(e => e.StartTime).HasDatabaseName("IX_Matches_StartTime");
            entity.HasIndex(e => e.ProcessedAt).HasDatabaseName("IX_Matches_ProcessedAt");
            entity.HasIndex(e => e.WinnerRadiant).HasDatabaseName("IX_Matches_WinnerRadiant");

            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.ProcessedAt)
                .HasColumnType("timestamp with time zone");
        });

        // Configure Player entity
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId);
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_Players_Name");
            entity.HasIndex(e => e.TotalMatches).HasDatabaseName("IX_Players_TotalMatches");

            entity.Property(e => e.LastUpdated)
                .HasColumnType("timestamp with time zone");
        });

        // Configure Hero entity
        modelBuilder.Entity<Hero>(entity =>
        {
            entity.HasKey(e => e.HeroId);
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_Heroes_Name");
            entity.HasIndex(e => e.TotalPicks).HasDatabaseName("IX_Heroes_TotalPicks");

            entity.Property(e => e.LastUpdated)
                .HasColumnType("timestamp with time zone");
        });

        // Configure MatchPlayer entity (junction table)
        modelBuilder.Entity<MatchPlayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MatchId).HasDatabaseName("IX_MatchPlayers_MatchId");
            entity.HasIndex(e => e.PlayerId).HasDatabaseName("IX_MatchPlayers_PlayerId");
            entity.HasIndex(e => e.HeroId).HasDatabaseName("IX_MatchPlayers_HeroId");
            entity.HasIndex(e => new { e.MatchId, e.PlayerId }).HasDatabaseName("IX_MatchPlayers_Match_Player");

            // Configure relationships with cascade delete
            entity.HasOne(e => e.Match)
                .WithMany(m => m.MatchPlayers)
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Player)
                .WithMany(p => p.MatchPlayers)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Hero)
                .WithMany(h => h.MatchPlayers)
                .HasForeignKey(e => e.HeroId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Job entity
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId);
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Jobs_Status");
            entity.HasIndex(e => e.Type).HasDatabaseName("IX_Jobs_Type");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Jobs_CreatedAt");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp with time zone");
        });
    }
}
