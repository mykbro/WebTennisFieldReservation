using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Api
{
	public class ReservationSlotEntryModel
	{
		[Required]
		[Range(0, 167)]
		public int Slot { get; set; }

		[Required]
		[Range(typeof(Decimal),"0", "1000000")]			//we set the max to 1 milion instead of Decimal.MaxValue
		public decimal Price { get; set; }
	}
}
