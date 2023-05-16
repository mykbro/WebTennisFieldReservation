namespace WebTennisFieldReservation.Utilities.Paypal
{
	public class PaypalCreateOrderRequest
	{
		public List<PaypalPurchaseUnit> purchase_units { get; set; } = null!;
		public string intent { get; set; } = null!;
	}
}
