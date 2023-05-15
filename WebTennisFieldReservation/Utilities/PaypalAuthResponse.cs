namespace WebTennisFieldReservation.Utilities
{
	public class PaypalAuthResponse
	{
		public string Scope { get; set; } = null!;
		public string Access_Token { get; set; } = null!;
		public string Token_Type { get; set; } = null!;
		public string App_Id { get; set; } = null!;
		public int Expires_In { get; set; }
		public string Nonce { get; set; } = null!;
	}
}
