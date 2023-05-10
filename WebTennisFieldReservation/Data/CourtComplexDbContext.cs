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
        public DbSet<Template> Templates { get; set; } = null!;
        public DbSet<TemplateEntry> TemplateEntries { get; set; } = null!;       
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<ReservationEntry> ReservationEntries { get; set; } = null!;
        public DbSet<ReservationSlot> ReservationsSlots { get; set; } = null!;

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
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(Console.WriteLine, new[] {RelationalEventId.TransactionStarted, RelationalEventId.TransactionCommitted, RelationalEventId.TransactionRolledBack, RelationalEventId.CommandExecuted });
            }

            return new CourtComplexDbContext(optionsBuilder.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TemplateEntry>().HasKey(entry => new { entry.TemplateId, entry.WeekSlot});           
            modelBuilder.Entity<ReservationEntry>().HasKey(entry => new { entry.ReservationId, entry.ReservationEntryWeakId});	
		}
    }
}
