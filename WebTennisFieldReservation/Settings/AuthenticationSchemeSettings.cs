namespace WebTennisFieldReservation.Settings
{
    public class AuthenticationSchemeSettings
    {
        public string? ProtectorPurposeString { get; set; }
        public string? CookieName { get; set; }
        public TimeSpan? CookieMaxAge { get; set; }
    }
}
