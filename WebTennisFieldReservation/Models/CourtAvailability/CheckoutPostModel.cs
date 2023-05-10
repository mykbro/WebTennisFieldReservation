using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutPostModel
	{
		[Required]
		public string JsonPayload { get; set; } = null!;
	}
}
