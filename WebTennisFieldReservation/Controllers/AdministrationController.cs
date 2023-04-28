using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;

namespace WebTennisFieldReservation.Controllers
{
    [Route("/administration")]
    [Authorize(Policy = AuthorizationPoliciesNames.IsAdmin)]
    [AutoValidateAntiforgeryToken]
    public class AdministrationController : Controller
    {
        private readonly ICourtComplexRepository _repo;

        public AdministrationController(ICourtComplexRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

		[HttpGet("users")]
		public async Task<IActionResult> Users()
		{
			List<UserPartialModel> users = await _repo.GetAllUsersDataAsync();
			return View(users);
		}
		

		[HttpGet("courts")]
        public IActionResult Courts()
        {
            return View();
        }

        [HttpGet("templates")]
        public IActionResult Templates()
        {
            //return View();
            return RedirectToAction(nameof(CreateTemplate));
        }

		[HttpPost("users/{id:guid}/delete")]
		public async Task<IActionResult> DeleteUser(Guid id)
		{
			//we should probably check that the user was deleted 
			int deletedUsers = await _repo.DeleteUserByIdAsync(id);

			return RedirectToAction(nameof(Users));
		}

        [HttpGet]
        public IActionResult Templates()
        {
            
            return View();
        }

        [HttpGet("templates/create")]
        public IActionResult CreateTemplate()
        {
            return View();
        }

		[HttpPost("templates/create")]
		public async Task<IActionResult> CreateTemplate(EditTemplateModel templateData)
		{
            if(ModelState.IsValid)
            {
                bool templateAdded = await _repo.AddTemplateAsync(templateData);

                if (templateAdded)
                {
                    return RedirectToAction(nameof(Templates));
                }
                else
                {
                    ModelState.AddModelError("", "Template name already used");
                    return View();
                }                
            }
            else
            {
                return View();
            }
		}


	}
}
