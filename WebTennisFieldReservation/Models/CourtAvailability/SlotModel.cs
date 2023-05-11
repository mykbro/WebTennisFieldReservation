namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class SlotModel
	{
		public int Id { get; set; }
		public int DaySlot { get; set; }
		public DateTime Date { get; set; }		
		public decimal Price { get; set; }
		public string CourtName { get; set; } = null!;
	}
}
