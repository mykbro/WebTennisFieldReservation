using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    public class ReservationEntry
    {
		//composite key defined in DbContext

		[ForeignKey(nameof(Reservation))]
		public Guid ReservationId { get; set; }

		[Required]
		public int EntryNr { get; set; }

		[ForeignKey(nameof(ReservationSlot))]
		public int ReservationSlotId { get; set; }		

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }

		// Navigation
		public Reservation Reservation { get; set; } = null!;
        public ReservationSlot ReservationSlot { get; set; } = null!;
    }
}
