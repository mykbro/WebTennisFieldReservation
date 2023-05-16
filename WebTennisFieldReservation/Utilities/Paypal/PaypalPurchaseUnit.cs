namespace WebTennisFieldReservation.Utilities.Paypal
{
	public class PaypalPurchaseUnit
	{
		public string description { get; set; } = null!;
		public PaypalAmount amount { get; set; } = null!;
	}
}
