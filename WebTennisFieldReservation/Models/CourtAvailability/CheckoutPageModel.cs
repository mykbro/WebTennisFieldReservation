namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutPageModel
	{
		public List<SlotModel> Entries { get; }
		public decimal TotalPrice { get; }
		public Guid PaymentToken { get; }

		public CheckoutPageModel(List<SlotModel> entries, Guid paymentToken) 
		{ 
			Entries = entries;
			TotalPrice = Entries.Select(e => e.Price).Sum();
			PaymentToken = paymentToken;
		}

	}
}
