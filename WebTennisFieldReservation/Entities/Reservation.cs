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
        public DateTimeOffset CreatedOn { get; set; }

        [Required]
        public DateTimeOffset LastUpdateOn { get; set; }

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
        Pending,                //we place it in the db for "fixing" the price with a NULL paymentId. We do not reserve the slots yet.
        PaymentCreated,         //we update the paymentId field.
		PaymentApproved,        //the user approved the payment and the returnUrl has been called
                                //we need this to protect against replayed/concurrent order confirmations. 
        Fulfilled,              //the slots were available and we were able to fulfill the order reserving the slots. We need to capture the payment.
		Confirmed,              //we successfully captured the payment and tried to send a confirmation email
        Aborted                 //something went wrong during the payment/fulfillment but we want to keep the reservation in the db							 
	}
}
