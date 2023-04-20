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
        private static readonly string ConfirmationMailSubject = "Please confirm you email";
        private static readonly string ConfirmationMailBodyTemplate = "Click <a href=\"{0}\">here</a> to confirm your email";

        private readonly ICourtComplexRepository _repo;

        public UsersController(ICourtComplexRepository repo)
        {
            _repo = repo;
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
        public async Task<IActionResult> Register(RegisterUserModel registrationInfo, string? returnUrl, [FromServices] IPasswordHasher pwdService, [FromServices] ISingleUserMailSender mailSender, [FromServices] ITokenManager tokenManager)
        {
            if (ModelState.IsValid)
            {
                var pwdInfo = pwdService.GeneratePasswordAndSalt(registrationInfo.Password);

                User toAdd = new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = registrationInfo.FirstName,
                    LastName = registrationInfo.LastName,
                    Email = registrationInfo.Email,
                    Address = registrationInfo.Address,
                    BirthDate = registrationInfo.BirthDate,
                    EmailConfirmed = false,
                    Pbkdf2Iterations = pwdService.Iterations,
                    SecurityStamp = Guid.NewGuid(),
                    PwdHash = pwdInfo.Password,
                    PwdSalt = pwdInfo.Salt
                };

                bool userAdded = await _repo.AddUserAsync(toAdd);

                if (userAdded)
                {
                    string token = tokenManager.GenerateToken(ProtectorPurposesNames.EmailConfirmation, toAdd.Id, toAdd.SecurityStamp, DateTimeOffset.Now);                    
                    string mailBody = String.Format(UsersController.ConfirmationMailBodyTemplate, Url.Action(nameof(EmailConfirmation), "users", new { token = token }, Request.Scheme, Request.Host.Value));
                    await mailSender.SendEmailAsync(registrationInfo.Email, UsersController.ConfirmationMailSubject, mailBody);
                    return RedirectToAction(nameof(RegistrationOk), new {ReturnUrl = returnUrl});
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

        [HttpGet("registrationok")]
        public IActionResult RegistrationOk(string? returnUrl)
        {
            string temp = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            return View((object) temp);
        }

        [HttpGet("emailconfirmation")]
        public IActionResult EmailConfirmation(string? token)
        {
            return View();
        }
        
        [HttpPost("emailconfirmation")]
        public async Task<IActionResult> EmailConfirmation(ConfirmMailModel confirmationData, [FromServices] ITokenManager tokenManager, [FromServices] TokenManagerSettings tokenManagerSettings)
        {
            //we check if we have a token passed as a parameter
            if (ModelState.IsValid)
            {
                //if so we check if the token string is deserializable to a valid SecurityToken
                try
                {
                    SecurityToken token = tokenManager.RetrieveTokenFromString(confirmationData.Token, ProtectorPurposesNames.EmailConfirmation);
                    
                    //we check if the token has not expired
                    if (DateTimeOffset.Now <= token.IssueTime.Add(TimeSpan.FromMinutes(tokenManagerSettings.ValidTimeSpanInMinutes)))
                    {
                        //we finally try to update the confirmation status
                        int usersUpdated = await _repo.UpdateUserEmailConfirmationByIdAndSecurityStampAsync(token.UserId, token.SecurityStamp);

                        //we check how many users we updated... 0 -> already confirmed, 1 -> OK, 2+ -> BIIIG PROBLEMS !!
                        if (usersUpdated == 1) 
                        {
                            return RedirectToAction(nameof(EmailConfirmed));   
                        }
                    }                   
                }
                catch(CryptographicException ex)
                {
                    //forged token string
                }  
                catch(FormatException ex)
                {
                    //malformed base64 token string
                }
            }
            
            return NotFound(); 
        }

        [HttpGet("emailconfirmed")]
        public IActionResult EmailConfirmed()
        {    
            return View();
        }

    }
}
