namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CreateReservationModel
	{
		public Guid UserId { get; set; }
		public Guid ReservationId { get; set; }
		public List<int> SlotIds { get; set; } = null!;
		public DateTimeOffset Timestamp { get; set; }
		public Guid PaymentConfirmationToken { get; set; }
		public string PaymentId { get; set; } = null!;
	}
}
