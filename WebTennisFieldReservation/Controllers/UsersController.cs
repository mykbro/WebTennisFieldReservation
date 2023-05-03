using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Users;
using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Services;
using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Utilities;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;

namespace WebTennisFieldReservation.Controllers
{
    [Route("/users")]
    [AutoValidateAntiforgeryToken]
    public class UsersController : Controller
    {
               
        private static readonly AuthenticationProperties RememberMeProperty = new AuthenticationProperties() { IsPersistent = true };
        private static readonly AuthenticationProperties DoNotRememberMeProperty = new AuthenticationProperties() { IsPersistent = false };


        public UsersController()
        {           
        }

        /*
        public IActionResult Index()
        {            
            //TODO
            return View();
        }
        */

        [HttpGet("register")]
        public IActionResult Register()
        {            
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserModel registrationInfo, string? returnUrl, [FromServices] ICourtComplexRepository repo, [FromServices] IPasswordHasher pwdHasher, [FromServices] ISingleUserMailSender mailSender, [FromServices] ITokenManager tokenManager)
        {
            if (ModelState.IsValid)
            {
                var pwdInfo = pwdHasher.GeneratePasswordAndSalt(registrationInfo.Password);

                User toAdd = new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = registrationInfo.FirstName,
                    LastName = registrationInfo.LastName,
                    Email = registrationInfo.Email.ToLower(),       //note the .ToLower()
                    Address = registrationInfo.Address,
                    BirthDate = registrationInfo.BirthDate,
                    EmailConfirmed = true,                          //set to true just for testing
                    RegistrationTimestamp = DateTimeOffset.Now,
                    Pbkdf2Iterations = pwdHasher.Iterations,
                    SecurityStamp = Guid.NewGuid(),
                    PwdHash = pwdInfo.Password,
                    PwdSalt = pwdInfo.Salt
                };

                bool userAdded = await repo.AddUserAsync(toAdd);

                if (userAdded)
                {
                    string token = tokenManager.GenerateToken(ProtectorPurposesNames.EmailConfirmation, toAdd.Id, toAdd.SecurityStamp, DateTimeOffset.Now);

                    try
                    {
                        await SendConfirmationEmailAsync(toAdd.Email, token, mailSender);
                    }
                    catch
                    {

                    }
                    
                    return RedirectToAction(nameof(RegisterSuccess), new {ReturnUrl = returnUrl});
                }
                else
                {
                    ModelState.AddModelError("", "Email already registered");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [HttpGet("register/success")]
        public IActionResult RegisterSuccess(string? returnUrl)
        {
            string temp = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            return View((object) temp);     //must cast to object to use a string as a model
        }

        [HttpGet("confirmemail")]
        public IActionResult ConfirmEmail(string? token)
        {
            return View();
        }
        
        [HttpPost("confirmemail")]        
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel modelData, [FromServices] ICourtComplexRepository repo, [FromServices] ITokenManager tokenManager, [FromServices] TokenManagerSettings tokenManagerSettings)
        {            
            if (ModelState.IsValid)
            {
                //if so we check if the token string is deserializable to a valid SecurityToken
                try
                {
                    SecurityToken secTok = tokenManager.RetrieveTokenFromString(modelData.Token, ProtectorPurposesNames.EmailConfirmation);
                    
                    //we check if the token has not expired
                    if (DateTimeOffset.Now <= secTok.IssueTime.Add(TimeSpan.FromMinutes(tokenManagerSettings.EmailConfirmationValidTimeSpanInMinutes)))
                    {
                        //we finally try to update the confirmation status
                        int usersUpdated = await repo.ConfirmUserEmailAsync(secTok.UserId, secTok.SecurityStamp);

                        //we check how many users we updated... 0 -> already confirmed, 1 -> OK, 2+ -> BIIIG PROBLEMS !!
                        if (usersUpdated == 1) 
                        {
                            return RedirectToAction(nameof(ConfirmEmailSuccess));   
                        }
                    }                   
                }
                catch(CryptographicException ex)
                {
                    //tampered token string
                }  
                catch(FormatException ex)
                {
                    //malformed base64 token string
                }
            }

            return BadRequest();
        }

        [HttpGet("confirmemail/success")]
        public IActionResult ConfirmEmailSuccess()
        {    
            return View();
        }

        [HttpGet("resendconfirmationemail")]
        public IActionResult ResendConfirmationEmail()
        {
            return View();
        }

        [HttpPost("resendconfirmationemail")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmEmailModel modelData, [FromServices] ICourtComplexRepository repo, [FromServices] ISingleUserMailSender mailSender, [FromServices] ITokenManager tokenManager)
        {
            if (ModelState.IsValid)
            {
                // GetDataForTokenAsync doesn't check if email is already confirmed... it's a tradeoff for reusing the method in other context
                // the confirmation link will fail anyway
                var tokenData = await repo.GetDataForTokenAsync(modelData.Email);
                
                // we check if we found the email, if not found tokenData will be = to its default
                if(tokenData != default)
                {
                    // we create a new token
                    string token = tokenManager.GenerateToken(ProtectorPurposesNames.EmailConfirmation, tokenData.Id, tokenData.SecurityStamp, DateTimeOffset.Now);

                    // we try to send it
                    try
                    {
                        await SendConfirmationEmailAsync(modelData.Email, token, mailSender);
                    }
                    catch
                    {

                    }
                }
            }

            //we return "email sent" anyway
            return RedirectToAction(nameof(ResendConfirmationEmailDone));
        }

        [HttpGet("resendconfirmationemail/done")]
        public IActionResult ResendConfirmationEmailDone()
        {
            return View();
        }

        [HttpGet("sendresetpasswordlink")]
        public IActionResult SendResetPasswordLink()
        {            
            return View();
        }

        [HttpPost("sendresetpasswordlink")]
        public async Task<IActionResult> SendResetPasswordLink(SendResetLinkModel modelData, [FromServices] ICourtComplexRepository repo, [FromServices] ISingleUserMailSender mailSender, [FromServices] ITokenManager tokenManager)
        {
            if(ModelState.IsValid)
            {
                var tokenData = await repo.GetDataForTokenAsync(modelData.Email);

                if(tokenData != default)
                {
                    // we create a new token
                    string token = tokenManager.GenerateToken(ProtectorPurposesNames.PasswordReset, tokenData.Id, tokenData.SecurityStamp, DateTimeOffset.Now);

                    // we try to send the mail
                    try
                    {
                        await SendPwdResetEmailAsync(modelData.Email, token, mailSender);
                    }
                    catch
                    {

                    }
                }
            }

            return RedirectToAction(nameof(SendResetPasswordLinkDone));
        }

        [HttpGet("sendresetpasswordlink/done")]
        public IActionResult SendResetPasswordLinkDone()
        {
            return View();
        }

        [HttpGet("resetpassword")]
        public IActionResult ResetPassword(string? token)
        {
            return View();
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(PasswordResetModel modelData, [FromServices] ICourtComplexRepository repo, [FromServices] ITokenManager tokenManager, [FromServices] IPasswordHasher pwdHasher, [FromServices] TokenManagerSettings tokenManagerSettings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SecurityToken secTok = tokenManager.RetrieveTokenFromString(modelData.Token, ProtectorPurposesNames.PasswordReset);

                    //we check if the token has not expired
                    if (DateTimeOffset.Now <= secTok.IssueTime.Add(TimeSpan.FromMinutes(tokenManagerSettings.PwdResetValidTimeSpanInMinutes)))
                    {
                        var pwdInfo = pwdHasher.GeneratePasswordAndSalt(modelData.Password);
                        
                        int usersUpdated = await repo.ResetUserPasswordAsync(new User()
                        {
                            Id = secTok.UserId,
                            SecurityStamp = secTok.SecurityStamp,
                            PwdHash = pwdInfo.Password,
                            PwdSalt = pwdInfo.Salt,
                            Pbkdf2Iterations = pwdHasher.Iterations
                        }, Guid.NewGuid());

                        if (usersUpdated == 1) 
                        { 
                            return RedirectToAction(nameof(ResetPasswordSuccess));
                        }
                    }
                }
                catch (CryptographicException ex)
                {
                    //tampered token string
                }
                catch (FormatException ex)
                {
                    //malformed base64 token string
                }

            }

            return BadRequest();
        }

        [HttpGet("resetpassword/success")]
        public IActionResult ResetPasswordSuccess()
        {
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl)
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserModel loginData, string? returnUrl, [FromServices] ICourtComplexRepository repo, [FromServices] IPasswordHasher pwdHasher, [FromServices] ClaimsPrincipalFactory claimsFactory)
        {
            if (ModelState.IsValid)
            {               
                User partialUserData = await repo.GetDataForLoginCheckAsync(loginData.Email);

                // we check if a confirmed user was found
                if(partialUserData != default)
                {
                    // we check if the passwords match (using the db iters, not the live value in the pwdHasher
                    if(pwdHasher.ValidatePassword(loginData.Password, partialUserData.PwdHash, partialUserData.PwdSalt, partialUserData.Pbkdf2Iterations))
                    {
                        // we check if the user is an admin
                        bool isAdmin = await repo.IsAdminAsync(partialUserData.Id);

                        // we can then proceed to build the claimsprincipal and signIn
                        ClaimsPrincipal userCp = claimsFactory.CreatePrincipal(partialUserData.Id, partialUserData.SecurityStamp, isAdmin, DateTimeOffset.Now);
                        await HttpContext.SignInAsync(AuthenticationSchemesNames.MyAuthScheme, userCp, loginData.RememberMe ? RememberMeProperty : DoNotRememberMeProperty);

                        // we check if the returnUrl is valid
                        if(Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return Redirect("/");
                        }
                    }
                    else //invalid password
                    {
                        ModelState.AddModelError("", "Invalid username and/or password");
                    }
                }
                else //invalid username
                {
                    ModelState.AddModelError("", "Invalid username and/or password");
                }
            }
            
            return View();
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout(string? returnUrl)
        {          
            HttpContext.SignOutAsync();

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("/");
            }
        }

        [HttpGet("{id:guid}/details")]
        //[Authorize(Policy = AuthorizationPoliciesNames.SameUser)]
        public async Task<IActionResult> Details(Guid id, [FromServices] ICourtComplexRepository repo, [FromServices] IAuthorizationService authorizer)
        {
            //we must follow the "SameUser" authZ policy
            AuthorizationResult authResult = await authorizer.AuthorizeAsync(User, id, AuthorizationPoliciesNames.SameUser);

            if (authResult.Succeeded)
            {
                //we check for user data for this id
                User? userData = await repo.GetUserDataByIdAsync(id);
                
                //if any we populate the view
                if(userData != null)
                {
                    var model = new EditUserDataModel() 
                    { 
                        FirstName = userData.FirstName, 
                        LastName = userData.LastName,
                        Address = userData.Address,
                        BirthDate = userData.BirthDate,
                        Email = userData.Email
                    };

                    return View(model);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Challenge();
                }
                else
                {
                    return Forbid();
                }
            }
        }

        [HttpPost("{id:guid}/details")]
        //[Authorize(Policy = AuthorizationPoliciesNames.SameUser)]
        public async Task<IActionResult> Details(EditUserDataModel userData, Guid id, [FromServices] ICourtComplexRepository repo, [FromServices] IAuthorizationService authorizer)
        {
            
            //we first check for authZ
            AuthorizationResult authResult = await authorizer.AuthorizeAsync(User, id, AuthorizationPoliciesNames.SameUser);

            if (authResult.Succeeded)
            {
                //we then check for the model validity
                if (ModelState.IsValid)
                {
                    //we first need to lowerCase the Email field
                    userData.Email = userData.Email.ToLower();

                    //we try to update the user's data (will fail on a duplicate email)
                    var user = new User()
                    {
                        Id = id,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        Address = userData.Address,
                        BirthDate = userData.BirthDate,
                        Email = userData.Email
                    };

                    int usersUpdated = await repo.UpdateUserDataAsync(user);

                    if(usersUpdated == 1) 
                    {
                        return RedirectToAction(nameof(UserUpdated));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email already registered");
                        return View();
                    }
                }
                else
                {
                    return View();
                }
            }
            else
            {
                if(!User.Identity!.IsAuthenticated)
                {
                    return Challenge();
                }
                else
                {
                    return Forbid();
                }                
            }
        }

        [HttpGet("userupdated")]
        public IActionResult UserUpdated()
        {
            return View();
        }

        
        [HttpGet("{id:guid}/editpassword")]
        //[Authorize(Policy = AuthorizationPoliciesNames.LoggedRecently)]
        //we manually check the "LoggedRecently" policy to issue a Challenge instead of a Forbid
        public async Task<IActionResult> EditPassword(Guid id, [FromServices] IAuthorizationService authorizer) 
        {
            AuthorizationResult sameUserCheck = await authorizer.AuthorizeAsync(User, id, AuthorizationPoliciesNames.SameUser);

            if(sameUserCheck.Succeeded)
            {
                //we then check if we recently logged
                AuthorizationResult recentLogCheck = await authorizer.AuthorizeAsync(User, AuthorizationPoliciesNames.LoggedRecently);

                if (recentLogCheck.Succeeded)
                {
                    return View();
                }
                else
                {
                    //we return a challenge and not a Forbid !!!
                    return Challenge();
                }
            }
            else
            {                
                if(!User.Identity!.IsAuthenticated)
                {
                    return Challenge();
                }
                else
                {
                    return Forbid();
                }
            }
        }

        [HttpPost("{id:guid}/editpassword")]
        //[Authorize(Policy = AuthorizationPoliciesNames.LoggedRecently)]
        //we manually check the "LoggedRecently" policy to issue a Challenge instead of a Forbid
        public async Task<IActionResult> EditPassword(EditPasswordModel pwdData, Guid id, [FromServices] IAuthorizationService authorizer, [FromServices] ICourtComplexRepository repo, [FromServices] IPasswordHasher pwdHasher, [FromServices] ClaimsPrincipalFactory claimsPrincipalFactory)
        {
            AuthorizationResult sameUserCheck = await authorizer.AuthorizeAsync(User, id, AuthorizationPoliciesNames.SameUser);

            if (sameUserCheck.Succeeded)
            {
                //we then check if we recently logged
                AuthorizationResult recentLogCheck = await authorizer.AuthorizeAsync(User, AuthorizationPoliciesNames.LoggedRecently);

                if (recentLogCheck.Succeeded)
                {
                    if (ModelState.IsValid)
                    {
                        //we retrieve the user current password data (hash, salt, iters)
                        var userSecurityData = await repo.GetPasswordDataByIdAsync(id);

                        if (userSecurityData != default)
                        {
                            //if we found something as we should we check the supplied password with the one in the db
                            bool pwdValid = pwdHasher.ValidatePassword(pwdData.CurrentPassword, userSecurityData.PwdHash, userSecurityData.PwdSalt, userSecurityData.Pbkdf2Iterations);

                            if (pwdValid)
                            {
                                //we generate the new security data
                                var newPwdData = pwdHasher.GeneratePasswordAndSalt(pwdData.NewPassword);
                                Guid newSecStamp = Guid.NewGuid();

                                //we can update the password with the current iters and a new securityStamp
                                var user = new User()
                                {
                                    Id = id,
                                    PwdHash = newPwdData.Password,
                                    PwdSalt = newPwdData.Salt,
                                    Pbkdf2Iterations = pwdHasher.Iterations
                                };

                                int usersUpdated = await repo.UpdatePasswordDataAsync(user, newSecStamp);

                                //we do a little check
                                if (usersUpdated != 1)
                                {
                                    throw new Exception("Password update didn't return 'usersUpdated == 1'");
                                }

                                //we resign-in the user with the updated security stamp
                                bool wasAdmin = Boolean.Parse(User.FindFirstValue(ClaimsNames.IsAdmin));
                                var claimsPrincipal = claimsPrincipalFactory.CreatePrincipal(id, newSecStamp, wasAdmin, DateTimeOffset.Now);
                                await HttpContext.SignInAsync(AuthenticationSchemesNames.MyAuthScheme, claimsPrincipal);

                                //and we return to the details page for this user
                                return RedirectToAction(nameof(Details), new { id = id });
                            }
                            else //current password wasn't valid
                            {
                                ModelState.AddModelError("", "Wrong current password");
                                return View();
                            }
                        }
                        else //somehow the password wasn't found... shouldn't enter here
                        {
                            return NotFound();
                        }
                    }
                    else //model invalid
                    {
                        return View();
                    }
                }
                else    //our login wasn't recent
                {
                    //we return a challenge and not a Forbid !!!
                    return Challenge();
                }
                
            }
            else
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Challenge();
                }
                else
                {
                    return Forbid();
                }
            }
        }


        ///////////////////////////////////////////////

        private Task SendConfirmationEmailAsync(string recipientEmail, string tokenString, ISingleUserMailSender mailSender)
        {
            string confirmationMailSubject = "Please confirm you email";
            string confirmationMailBodyTemplate = "Click <a href=\"{0}\">here</a> to confirm your email";
           
            string mailBody = String.Format(confirmationMailBodyTemplate, Url.Action(nameof(ConfirmEmail), "users", new { token = tokenString }, Request.Scheme, Request.Host.Value));

            //return mailSender.SendEmailAsync(recipientEmail, confirmationMailSubject, mailBody);
            return Task.CompletedTask;
        }

        private Task SendPwdResetEmailAsync(string recipientEmail, string tokenString, ISingleUserMailSender mailSender)
        {
            string resetPwdMailSubject = "Reset your password";
            string resetPwdMailBodyTemplate = "Click <a href=\"{0}\">here</a> to reset your password";

            string mailBody = String.Format(resetPwdMailBodyTemplate, Url.Action(nameof(ResetPassword), "users", new { token = tokenString }, Request.Scheme, Request.Host.Value));

            //return mailSender.SendEmailAsync(recipientEmail, resetPwdMailSubject, mailBody);
            return Task.CompletedTask;
        }

    }
}
