using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Constants.Names;

namespace WebTennisFieldReservation.AuthenticationSchemes.MyAuthScheme
{
    public class MyAuthSchemeHandler : SignInAuthenticationHandler<MyAuthSchemeOptions>
    {
        private readonly ICourtComplexRepository _repo;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        private string CookieName => Options.CookieName ?? Scheme.Name;
        private string ProtectionPurpose => Options.ProtectorPurposeString ?? Scheme.Name;

        public MyAuthSchemeHandler(IOptionsMonitor<MyAuthSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ICourtComplexRepository repo, IDataProtectionProvider protectorProvider):base(options, logger, encoder, clock)
        {
            _repo = repo;
            _dataProtectionProvider = protectorProvider;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? cookieValue = Context.Request.Cookies[CookieName];

            //we check if the auth cookie exists
            if (cookieValue != null)
            {
                      

                try
                {
                    //we try to convert the token in bytes
                    byte[] cookieAsBytes = Base64UrlEncoder.DecodeBytes(cookieValue);

                    try
                    {
                        //we try to decrypt it         
                        IDataProtector protector = _dataProtectionProvider.CreateProtector(ProtectionPurpose);
                        byte[] claimsAsBytes = protector.Unprotect(cookieAsBytes);

                        ClaimsPrincipal principal;

                        //if we succeed we build the ClaimsPrincipal from the bytes
                        using (MemoryStream ms = new MemoryStream(claimsAsBytes))
                        {
                            using (BinaryReader br = new BinaryReader(ms))
                            {
                                principal = new ClaimsPrincipal(br);
                            }
                        }

                        //we extract user data from the claims
                        string? idAsString = principal.FindFirstValue(ClaimsNames.Id);
                        string? securityStampAsString = principal.FindFirstValue(ClaimsNames.SecurityStamp);
                        string? issueTimeAsString = principal.FindFirstValue(ClaimsNames.IssueTime);

                        //we parse everything, shouldn't throw
                        Guid id = Guid.Parse(idAsString);
                        Guid secStamp = Guid.Parse(securityStampAsString);
                        DateTimeOffset issueTime = DateTimeOffset.Parse(issueTimeAsString);

                        //we check in the db if the id is still there and the secStamp is still the same
                        //and contextually we retrieve the user data

                        var userData = await _repo.GetAuthenticatedUserData



                        //we add user data claims to the principal




                        //and we return the principal in an auth ticket
                        AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

                        return Task.FromResult(AuthenticateResult.Success(ticket));                                                  
                        

                    }
                    catch (CryptographicException ex)
                    {
                        //encryption authentication failed
                        return Task.FromResult(AuthenticateResult.Fail("Invalid auth cookie"));
                    }
                }
                catch (FormatException ex)
                {
                    //string base64 was malformed
                    return Task.FromResult(AuthenticateResult.Fail("Invalid auth cookie"));
                }
                
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("Auth cookie not found"));
            }
            
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            Context.Response.Cookies.Delete(CookieName);

            return Task.CompletedTask;
        }
    }
}
