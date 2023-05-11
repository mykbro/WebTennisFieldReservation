using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    public class Reservation
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<ReservationEntry> ReservationEntries { get; set; } = new List<ReservationEntry>();
    }
}
