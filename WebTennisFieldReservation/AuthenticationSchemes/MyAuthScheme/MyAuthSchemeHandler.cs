﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Constants.Names;
using Microsoft.AspNetCore.Http.Extensions;

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

                        // we extract user data from the claims that we need to check
                        string? idAsString = principal.FindFirstValue(ClaimsNames.Id);
                        string? securityStampAsString = principal.FindFirstValue(ClaimsNames.SecurityStamp);
                        //string? isAdminAsString = principal.FindFirstValue(ClaimsNames.IsAdmin);
                        //string? issueTimeAsString = principal.FindFirstValue(ClaimsNames.IssueTime);

                        // we parse, shouldn't throw
                        Guid id = Guid.Parse(idAsString);
                        Guid secStamp = Guid.Parse(securityStampAsString);
                        //bool isAdmin = Boolean.Parse(isAdminAsString);
                        //DateTimeOffset issueTime = DateTimeOffset.Parse(issueTimeAsString);

                        //we check in the db if the id is still there and the secStamp is still the same
                        //and contextually we retrieve the user data that we need in every request
                        //PLEASE NOTE that we don't check for any expiration (but we could using the issueTime Claim) 
                        var userData = await _repo.GetAuthenticatedUserDataAsync(id, secStamp);                        

                        if(userData != default)
                        {
                            //we add user data claims to the principal
                            var identity = principal.Identity as ClaimsIdentity;
                            identity!.AddClaim(new Claim(ClaimsNames.Fullname, userData.FirstName + " " + userData.LastName));
                            identity!.AddClaim(new Claim(ClaimsNames.Email, userData.Email));

                            //and we return the principal in an auth ticket
                            AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);
                            return AuthenticateResult.Success(ticket);
                        }
                        else
                        {
                            //cookie data is not valid anymore
                            await HandleSignOutAsync(null);
                            return AuthenticateResult.Fail("Cookie expired");
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        //encryption authentication failed
                        await HandleSignOutAsync(null);
                        return AuthenticateResult.Fail("Invalid auth cookie");
                    }
                }
                catch (FormatException ex)
                {
                    //string base64 was malformed
                    await HandleSignOutAsync(null);
                    return AuthenticateResult.Fail("Invalid auth cookie");
                }
                
            }
            else
            {
                //no cookie
                return AuthenticateResult.Fail("Auth cookie not found");
            }
            
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            //we get a protector
            IDataProtector protector = _dataProtectionProvider.CreateProtector(ProtectionPurpose);
            byte[] cpAsBytes;


            //we serialize the ClaimsPrincipal
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    user.WriteTo(bw);
                }
                
                cpAsBytes = ms.ToArray();
            }

            //we encrypt the ClaimsPrincipal
            byte[] encryptedData = protector.Protect(cpAsBytes);
            
            //we create a Base64UrlSafe string
            string cookieVal = Base64UrlEncoder.Encode(encryptedData);

            //we create/update the auth cookie in the response
            bool persistCookie = properties?.IsPersistent ?? false;
            
            CookieOptions cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Secure = true,
                MaxAge = persistCookie ? Options.CookieMaxAge : null                
            };

            Context.Response.Cookies.Append(CookieName, cookieVal, cookieOptions);
            return Task.CompletedTask;
        }
    

        protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            Context.Response.Cookies.Delete(CookieName);
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            
            string? accessDeniendPath = Options.AccessDeniedPath;
            
            if(accessDeniendPath != null)
            {
                Context.Response.Redirect(accessDeniendPath);
            }
            else
            {
                Context.Response.StatusCode = 403;
            }

            return Task.CompletedTask;
            
            //return Context.ForbidAsync("Cookies", properties);        //could have done just this
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            
            string? loginPath = Options.LoginPath;
            string? returnUrlParam = Options.ReturnUrlParameter;

            if (loginPath != null)
            {
                if(returnUrlParam != null)
                {
                    Context.Response.Redirect(loginPath + $"?{returnUrlParam}={UrlEncoder.Encode(Context.Request.GetEncodedPathAndQuery())}");
                }
                else
                {
                    Context.Response.Redirect(loginPath);
                }
            }
            else
            {
                Context.Response.StatusCode = 401;
            }

            return Task.CompletedTask;

            //return Context.ChallengeAsync("Cookies", properties);     //could have done just this
        }
    }
}
