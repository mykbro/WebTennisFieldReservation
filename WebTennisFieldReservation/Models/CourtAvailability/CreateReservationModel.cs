namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CreateReservationModel
	{
		public Guid UserId { get; set; }
		public List<int> SlotIds { get; set; } = null!;
		public DateTimeOffset Timestamp { get; set; }
	}
}
