namespace WebTennisFieldReservation.Models.Reservations
{
    public class ReservationCheckerModel
    {
        public Guid ReservationId { get; set; }
        public string PaymentToken { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
    }
}
