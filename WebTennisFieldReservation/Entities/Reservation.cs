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
        
        [StringLength(32)]        
        public string? PaymentId { get; set; }      //can be null on order creation

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<ReservationEntry> ReservationEntries { get; set; } = new List<ReservationEntry>();
    }

    public enum ReservationStatus
    {
        Pending,                //we place it in the db for "fixing" the price. We do not "block" the slots.
        PaymentCreated,         //we created the payment on paypal
        PaymentAuthorized,      //the user completed the paypal checkout successfully
        Fulfilled,              //the slots were available and we were able to fulfill the order
        Completed               //we captured the authorized payment
    }
}
