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
                /* we leave the check to the db and the entry model
                //we also need to check that the TemplateEntries posted are not repeated and inside the [0 - 167] range (167 = 7*24 - 1)                
                if (!AreTemplateEntriesOk(templateData))
                {
					ModelState.AddModelError("", "Malformed posted data");
					return View();
				}                
                */

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
            EditTemplateModel? templateData = await _repo.GetTemplateDataByIdAsync(id);

            if (templateData != null)
            {
                return View(templateData);
            }
            else
            {
                return NotFound();
            }            
        }

        [HttpPost("templates/{id:int}/details")]
        public async Task<IActionResult> TemplateDetails(int id, EditTemplateModel templateData)
        {
            if (ModelState.IsValid)
            {                

                int templatesUpdated = await _repo.UpdateTemplateByIdAsync(id, templateData);
                
                if (templatesUpdated == 1)
                {
                    return RedirectToAction(nameof(Templates));
                }
                else if(templatesUpdated == 0)
                {
                    return NotFound();
                }
                else //returned -1
                {
                    ModelState.AddModelError("", "Template name already used");
                    return View(templateData);
                }
            }
            else
            {
                return View(templateData);
            }
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

        
        /* we now leave the checks to the database and the entry model
        private bool AreTemplateEntriesOk(EditTemplateModel templateData)
        {
            // entries must be distinct and between [0 - 167]
            // (in this way we iterate the list a couple of times... the count is still low so we can do it,
            //  however we can do everything in one loop using a Set for distinctness checks)

            //we check if the count of distinct values is the same as the total count
            int distinctEntries = templateData.TemplateEntryModels.Distinct().Count();

            if (distinctEntries != templateData.TemplateEntryModels.Count)
            {
                return false;
            }

            //we then check that the elements are in the interval
            for (int i = 0; i < templateData.TemplateEntryModels.Count; i++)
            {
                if (templateData.TemplateEntryModels[i] < 0 || templateData.TemplateEntryModels[i] > 167)
                {
                    return false;
                }
            }

            return true;
        }
        */

    }
}
