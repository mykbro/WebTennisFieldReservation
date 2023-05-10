namespace WebTennisFieldReservation.Models.CourtAvailability
{
	public class CheckoutPageModel
	{
		public List<SlotModel> Entries { get; }
		public decimal TotalPrice { get; }

		public CheckoutPageModel(List<SlotModel> entries) 
		{ 
			Entries = entries;
			TotalPrice = Entries.Select(e => e.Price).Sum();
		}

	}
}
