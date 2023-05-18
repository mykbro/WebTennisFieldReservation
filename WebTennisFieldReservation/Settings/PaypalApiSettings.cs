namespace WebTennisFieldReservation.Settings
{
	public class PaypalApiSettings
	{
		public string AuthUrl { get; set; } = null!;
		public string ClientId { get; set; } = null!;
		public string Secret { get; set; } = null!;
		public string CreateOrderUrl { get; set; } = null!;
		public string CheckoutPageUrl { get; set; } = null!;
		public string CapturePaymentUrl { get; set; } = null!;
        public int ClientTimeoutInSecs { get; set; }
    }
}
