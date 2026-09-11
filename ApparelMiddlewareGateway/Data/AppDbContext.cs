using Microsoft.EntityFrameworkCore;
using ApparelMiddlewareGateway.Models;

namespace ApparelMiddlewareGateway.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tables in the SQL Database
        public DbSet<SpreaderTelemetry> SpreaderTelemetryRecords { get; set; }
        public DbSet<CuttingJob> CuttingJobs { get; set; }
        public DbSet<Roll> Rolls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce the "one roll per cutting job" rule at the database level so a
            // race between two concurrent submissions cannot bypass the app-side check.
            modelBuilder.Entity<SpreaderTelemetry>()
                .HasIndex(r => new { r.CuttingJobNo, r.RollID })
                .IsUnique();

            // Cutting job -> rolls (composite key on the roll).
            modelBuilder.Entity<Roll>()
                .HasKey(r => new { r.CuttingJobNo, r.RollID });

            modelBuilder.Entity<Roll>()
                .HasOne(r => r.CuttingJob)
                .WithMany(j => j.Rolls)
                .HasForeignKey(r => r.CuttingJobNo)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data so the LCNC frontend and the penetration tests have jobs to load.
            // CJ-9920 is routed to Spreader_01 (the line the test token is scoped to);
            // CJ-9921 is routed to Spreader_02 and is used to demonstrate the BOLA denial.
            modelBuilder.Entity<CuttingJob>().HasData(
                new CuttingJob { CuttingJobNo = "CJ-9920", TargetLine = "Spreader_01", Status = "Active" },
                new CuttingJob { CuttingJobNo = "CJ-9921", TargetLine = "Spreader_02", Status = "Active" });

            modelBuilder.Entity<Roll>().HasData(
                new Roll { CuttingJobNo = "CJ-9920", RollID = "R-1045", Status = "Pending" },
                new Roll { CuttingJobNo = "CJ-9920", RollID = "R-1046", Status = "Pending" },
                new Roll { CuttingJobNo = "CJ-9920", RollID = "R-1047", Status = "Pending" },
                new Roll { CuttingJobNo = "CJ-9921", RollID = "R-2050", Status = "Pending" },
                new Roll { CuttingJobNo = "CJ-9921", RollID = "R-2051", Status = "Pending" });
        }
    }
}
