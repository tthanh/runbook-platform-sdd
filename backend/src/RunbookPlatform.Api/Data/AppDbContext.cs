using Microsoft.EntityFrameworkCore;
using RunbookPlatform.Api.Domain;

namespace RunbookPlatform.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Runbook> Runbooks => Set<Runbook>();
    public DbSet<Step> Steps => Set<Step>();
    public DbSet<RunbookVersion> RunbookVersions => Set<RunbookVersion>();
    public DbSet<RunbookVersionStep> RunbookVersionSteps => Set<RunbookVersionStep>();
    public DbSet<Execution> Executions => Set<Execution>();
    public DbSet<StepRecord> StepRecords => Set<StepRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Runbook>(e =>
        {
            e.Property(r => r.Name).IsRequired();
            e.HasMany(r => r.Steps).WithOne().HasForeignKey(s => s.RunbookId);
            e.HasMany(r => r.Versions).WithOne().HasForeignKey(v => v.RunbookId);
        });

        modelBuilder.Entity<Step>(e =>
        {
            e.Property(s => s.Text).IsRequired();
        });

        modelBuilder.Entity<RunbookVersion>(e =>
        {
            // ADR-0001: per-Runbook sequential identity, DB-enforced.
            e.HasIndex(v => new { v.RunbookId, v.Number }).IsUnique();
            e.Property(v => v.NameAtPublish).IsRequired();
            e.HasMany(v => v.Steps).WithOne().HasForeignKey(s => s.RunbookVersionId);
        });

        modelBuilder.Entity<RunbookVersionStep>(e =>
        {
            e.Property(s => s.Text).IsRequired();
        });

        modelBuilder.Entity<Execution>(e =>
        {
            e.Property(x => x.IncidentId).IsRequired();
            // FR-015: one Execution per incident.
            e.HasIndex(x => x.IncidentId).IsUnique();
            e.Property(x => x.Status).HasConversion<string>();
            // Backing-field navigation for the append-only records list (_stepRecords).
            e.HasMany(x => x.StepRecords)
             .WithOne()
             .HasForeignKey(r => r.ExecutionId);
        });

        modelBuilder.Entity<StepRecord>(e =>
        {
            e.Property(r => r.Outcome).HasConversion<string>();
        });
    }
}
