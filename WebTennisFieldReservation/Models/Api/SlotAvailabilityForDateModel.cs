namespace WebTennisFieldReservation.Models.Api
{
	public class SlotAvailabilityForDateModel
	{
		public int Id { get; set; }
		public int DaySlot { get; set; }
		public bool IsAvailable { get; set; }
		public decimal Price { get; set; }
		public int CourtId { get; set; }
	}
}
