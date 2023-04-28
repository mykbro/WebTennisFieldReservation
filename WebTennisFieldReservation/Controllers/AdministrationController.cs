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
			List<UserRowModel> users = await _repo.GetAllUsersDataAsync();
			return View(users);
		}
		

		[HttpGet("courts")]
        public IActionResult Courts()
        {
            return View();
        }

        [HttpGet("templates")]
        public async Task<IActionResult> Templates()
        {

            List<TemplateRowModel> templates = await _repo.GetAllTemplatesAsync();
            return View(templates);
        }

		[HttpPost("users/{id:guid}/delete")]
		public async Task<IActionResult> DeleteUser(Guid id)
		{
			//we should probably check that the user was deleted 
			int deletedUsers = await _repo.DeleteUserByIdAsync(id);

			return RedirectToAction(nameof(Users));
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
                //we also need to check that the TemplateEntries posted are not repeated and inside the [0 - 167] range (167 = 7*24 - 1)
                int distinctEntries = templateData.TemplateEntries.Distinct().Count();

                if(distinctEntries != templateData.TemplateEntries.Count)
                {
					ModelState.AddModelError("", "Malformed posted data");
					return View();
				}

                for(int i = 0; i < templateData.TemplateEntries.Count; i++)
                {
                    if (templateData.TemplateEntries[i] < 0 || templateData.TemplateEntries[i] > 167)
                    {
						ModelState.AddModelError("", "Malformed posted data");
						return View();
					}
                }

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

        [HttpGet("templates/{id:int}/details")]
        public async Task<IActionResult> TemplateDetails(int id)
        {
            return View();
        }

        [HttpPost("templates/{id:int}/delete")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            int deletedRows = await _repo.DeleteTemplateByIdAsync(id);

            if(deletedRows == 1) 
            {
                return RedirectToAction(nameof(Templates));
            }
            else
            {
                return NotFound();
            }            
        }


    }
}
