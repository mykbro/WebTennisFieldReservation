using System.ComponentModel.DataAnnotations;
using WebTennisFieldReservation.Models.Api;

namespace WebTennisFieldReservation.Models.Administration
{
	public class ReservationSlotsModel
	{
		public List<CourtSelectionModel> CourtItems { get; set; } = null!;
		public List<TemplateSelectionModel> TemplateItems { get; set; } = null!;

		[DataType(DataType.Date)]
		public DateTime DefaultDate { get; set; }
	}
}
