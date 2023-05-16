namespace WebTennisFieldReservation.Utilities.Paypal
{
	public class PaypalCreateOrderResponse
	{
		public string id { get; set; } = null!;
		public string status { get; set; } = null!;
		public List<PaypalLink> links { get; set; } = null!;
		
	}
}
