using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutSlotEntryModel
	{		
		public int DaySlot { get; set; }	
		public int CourtId { get; set; }
	}
}
