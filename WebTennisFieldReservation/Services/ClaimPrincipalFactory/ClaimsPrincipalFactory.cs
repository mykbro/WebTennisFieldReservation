using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;
using WebTennisFieldReservation.Constants.Names;

namespace WebTennisFieldReservation.Services.ClaimPrincipalFactory
{
    public class ClaimsPrincipalFactory
    {
        private readonly string? _authSchemeName;

        public ClaimsPrincipalFactory(string? authSchemeName)
        {
            _authSchemeName = authSchemeName;
        }

        public ClaimsPrincipal CreatePrincipal(Guid id, Guid secStamp, bool isAdmin, DateTimeOffset issueTime, bool rememberMe)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimsNames.Id, id.ToString()));
            claims.Add(new Claim(ClaimsNames.SecurityStamp, secStamp.ToString()));
            claims.Add(new Claim(ClaimsNames.IsAdmin, isAdmin.ToString()));
            claims.Add(new Claim(ClaimsNames.IssueTime, issueTime.ToString()));
            claims.Add(new Claim(ClaimsNames.RememberMe, rememberMe.ToString()));

            ClaimsIdentity identity = new ClaimsIdentity(claims, _authSchemeName);

            return new ClaimsPrincipal(identity);
        }
    }
}
