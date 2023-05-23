namespace WebTennisFieldReservation.Utilities.Paypal
{
    public class PaypalCheckOrderResponse
    {
        public string id { get; set; } = null!;
        public string status { get; set; } = null!;
        public string intent { get; set; } = null!;
    }
}
