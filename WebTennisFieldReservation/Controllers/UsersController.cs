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

namespace WebTennisFieldReservation.Controllers
{
    [Route("/users")]
    [AutoValidateAntiforgeryToken]
    public class UsersController : Controller
    {
               

        public UsersController()
        {           
        }

        [Authorize(Policy = AuthorizationPoliciesNames.IsAdmin)]
        public IActionResult Index()
        {            
            //TODO
            return View();
        }

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
                    Email = registrationInfo.Email,
                    Address = registrationInfo.Address,
                    BirthDate = registrationInfo.BirthDate,
                    EmailConfirmed = false,
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
        public async Task<IActionResult> ConfirmEmail(ConfirmMailModel confirmationData, [FromServices] ICourtComplexRepository repo, [FromServices] ITokenManager tokenManager, [FromServices] TokenManagerSettings tokenManagerSettings)
        {
            //we check if we have a token string passed as a parameter
            if (ModelState.IsValid)
            {
                //if so we check if the token string is deserializable to a valid SecurityToken
                try
                {
                    SecurityToken token = tokenManager.RetrieveTokenFromString(confirmationData.TokenString, ProtectorPurposesNames.EmailConfirmation);
                    
                    //we check if the token has not expired
                    if (DateTimeOffset.Now <= token.IssueTime.Add(TimeSpan.FromMinutes(tokenManagerSettings.ValidTimeSpanInMinutes)))
                    {
                        //we finally try to update the confirmation status
                        int usersUpdated = await repo.ConfirmUserEmail(token.UserId, token.SecurityStamp);

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
            
            return NotFound(); 
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

        [HttpPost("resendconfirmatioemail")]
        public async Task<IActionResult> ResendConfirmationEmail(OnlyEmailModel modelData, [FromServices] ICourtComplexRepository repo, [FromServices] ISingleUserMailSender mailSender, [FromServices] ITokenManager tokenManager)
        {
            if (ModelState.IsValid)
            {
                var tokenData = await repo.GetDataForConfirmationTokenAsync(modelData.Email);
                
                //we check if we found the email, if not found tokenData will be = to its default
                if(tokenData != default)
                {
                    //we create a new token
                    string token = tokenManager.GenerateToken(ProtectorPurposesNames.EmailConfirmation, tokenData.Id, tokenData.SecurityStamp, DateTimeOffset.Now);

                    //we try to send it
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

        [HttpGet("resetpassword")]
        public IActionResult ResetPassword()
        {
            
            return View();
        }


        private Task SendConfirmationEmailAsync(string recipientEmail, string tokenString, ISingleUserMailSender mailSender)
        {
            string confirmationMailSubject = "Please confirm you email";
            string confirmationMailBodyTemplate = "Click <a href=\"{0}\">here</a> to confirm your email";
           
            string mailBody = String.Format(confirmationMailBodyTemplate, Url.Action(nameof(ConfirmEmail), "users", new { token = tokenString }, Request.Scheme, Request.Host.Value));
            
            return mailSender.SendEmailAsync(recipientEmail, confirmationMailSubject, mailBody);
        }

    }
}
