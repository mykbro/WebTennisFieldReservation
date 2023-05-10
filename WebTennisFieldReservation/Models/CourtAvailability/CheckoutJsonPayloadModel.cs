using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutJsonPayloadModel
	{			
		public DateTime Date { get; set; }		
		public List<CheckoutSlotEntryModel> Slots { get; set; } = null!;
	}
}
