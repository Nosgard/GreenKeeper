using GreenKeeper.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenKeeper.Database
{
    public class GreenKeeperDbContext : DbContext
    {
        public GreenKeeperDbContext(DbContextOptions<GreenKeeperDbContext> options)
            : base(options)
        {
        }

        public DbSet<Plant> Plants => Set<Plant>();
        public DbSet<CareSchedule> CareSchedules => Set<CareSchedule>();
        public DbSet<SunlightRequirement> SunlightRequirements => Set<SunlightRequirement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Plant>()
                .HasMany(p => p.CareSchedules)
                .WithOne(cs => cs.SelectedPlant)
                .HasForeignKey(cs => cs.PlantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Plant>()
                .HasOne(p => p.SunlightRequirement)
                .WithOne(sr => sr.SelectedPlant)
                .HasForeignKey<SunlightRequirement>(sr => sr.PlantId)
                .OnDelete(DeleteBehavior.Cascade);

            // One care schedule per care type and plant.
            modelBuilder.Entity<CareSchedule>()
                .HasIndex(cs => new { cs.PlantId, cs.Care })
                .IsUnique();
        }
    }
}
