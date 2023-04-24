using Microsoft.AspNetCore.Authentication;

namespace WebTennisFieldReservation.AuthenticationSchemes.MyAuthScheme
{
    public class MyAuthSchemeOptions : AuthenticationSchemeOptions
    {
        public string? ProtectorPurposeString { get; set; }
        public string? CookieName { get; set; }
        public TimeSpan? CookieMaxAge { get; set; }
        public string? AccessDeniedPath { get; set; }
        public string? ReturnUrlParameter { get; set; }
        public string? LoginPath { get; set; }
      
    }
}
