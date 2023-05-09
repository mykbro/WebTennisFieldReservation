using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class AvailabilityPageModel
	{
		[Required]
		[DataType(DataType.Date)]
		public DateTime Today { get; set; }
	}
}
