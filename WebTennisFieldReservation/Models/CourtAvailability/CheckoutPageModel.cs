namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutPageModel
	{
		public List<SlotModel> Entries { get; }
		public decimal TotalPrice { get; }
		public Guid CheckoutToken { get; }
		

		public CheckoutPageModel(List<SlotModel> entries, Guid checkoutToken) 
		{ 
			Entries = entries;
			TotalPrice = Entries.Select(e => e.Price).Sum();
			CheckoutToken = checkoutToken;
		}

	}
}
