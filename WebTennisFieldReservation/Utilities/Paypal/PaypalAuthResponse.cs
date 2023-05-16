namespace WebTennisFieldReservation.Utilities.Paypal
{
    public class PaypalAuthResponse
    {
        public string scope { get; set; } = null!;
        public string access_token { get; set; } = null!;
        public string token_type { get; set; } = null!;
        public string app_id { get; set; } = null!;
        public int expires_in { get; set; }
        public string nonce { get; set; } = null!;
    }
}
