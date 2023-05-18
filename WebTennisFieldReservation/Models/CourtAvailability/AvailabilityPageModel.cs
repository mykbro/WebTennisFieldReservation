using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class AvailabilityPageModel
	{		
		[DataType(DataType.Date)]
		public DateTime Today { get; set; }

		public Guid CheckoutToken { get; set; }
	}
}
