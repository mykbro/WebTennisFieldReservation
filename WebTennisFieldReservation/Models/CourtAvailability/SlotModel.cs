namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class SlotModel
	{		
		public int DaySlot { get; set; }
		public DateTime Date { get; set; }		
		public decimal Price { get; set; }
		public int CourtId { get; set; }
	}
}
