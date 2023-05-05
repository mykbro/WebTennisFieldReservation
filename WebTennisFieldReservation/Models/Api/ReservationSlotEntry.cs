using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Api
{
	public class ReservationSlotEntry
	{
		[Required]
		public int Slot { get; set; }
		[Required]
		public decimal Price { get; set; }
	}
}
