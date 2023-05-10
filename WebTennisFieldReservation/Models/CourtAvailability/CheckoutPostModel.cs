using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutPostModel
	{
		[Required]
		public List<int> SlotIds { get; set; } = null!;
	}
}
