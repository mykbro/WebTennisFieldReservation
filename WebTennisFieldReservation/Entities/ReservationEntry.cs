using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    
    
    public class ReservationEntry
    {
        [Key]
		[ForeignKey(nameof(ReservationSlot))]
		public int ReservationSlotId { get; set; }

		[ForeignKey(nameof(Reservation))]
        public Guid ReservationId { get; set;}

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }

		// Navigation
		public Reservation Reservation { get; set; } = null!;
        public ReservationSlot ReservationSlot { get; set; } = null!;
    }
}
