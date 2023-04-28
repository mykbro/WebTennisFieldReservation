using WebTennisFieldReservation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Design;

namespace WebTennisFieldReservation.Data
{
    public class CourtComplexDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;
        public DbSet<Court> Courts { get; set; } = null!;
        public DbSet<CourtAvailabilityTemplate> CourtsAvailabilityTemplates { get; set; } = null!;
        public DbSet<CourtAvailabilityTemplateEntry> CourtAvailabilityTemplateEntries { get; set; } = null!;
        public DbSet<CourtAvailabilityOverride> CourtAvailabilityOverrides { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<ReservationEntry> ReservationEntries { get; set; } = null!;

        public CourtComplexDbContext(DbContextOptions options) : base(options)
        {
            //for dependency injection
        }

        public static CourtComplexDbContext CreateDbContext(string connectionString, bool log = false)
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            
            optionsBuilder.UseSqlServer(connectionString);

            if (log)
            {
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            }

            return new CourtComplexDbContext(optionsBuilder.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CourtAvailabilityTemplateEntry>().HasKey(entry => new { entry.TemplateId, entry.WeekDay, entry.DaySlot});
            modelBuilder.Entity<CourtAvailabilityOverride>().HasKey(entry => new { entry.CourtId, entry.Day, entry.DaySlot });
            modelBuilder.Entity<ReservationEntry>().HasKey(entry => new { entry.ReservationId, entry.ReservationEntryWeakId});
        }
    }
}
