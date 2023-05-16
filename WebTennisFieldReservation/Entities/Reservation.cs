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

        [Required]
        public ReservationStatus Status { get; set; }

        [Required]
        public Guid PaymentConfirmationToken { get; set; }

        [Required]
        [StringLength(32)]        
        public string PaymentId { get; set; } = null!;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<ReservationEntry> ReservationEntries { get; set; } = new List<ReservationEntry>();
    }

    public enum ReservationStatus
    {
        Pending = 0,
        Authorized = 1,
        Confirmed = 2
    }
}
