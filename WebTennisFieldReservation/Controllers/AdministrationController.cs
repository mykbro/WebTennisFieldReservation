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
        public async Task<IActionResult> Courts()
        {
            List<CourtRowModel> courts = await _repo.GetAllCourtsAsync();
            return View(courts);
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
            //we share the view between Create and Details (edit)            

            //we prepare an empty model to pass to the shared (between Create and Edit) view
            //every IsSelected will be initialized to 'false' and every Price to 'null'
            //we create only ONE instance that we use to fill the whole list... here it's fine
            var emptyTemplate = new TemplateModel() { TemplateEntryModels = new List<TemplateEntryModel>(168) };
            TemplateEntryModel singleton = new TemplateEntryModel();
            
            for(int i = 0; i < 168; i++)
            {
                emptyTemplate.TemplateEntryModels.Add(singleton);
            } 

            ViewData["Title"] = "Create a new template";
            return View("CreateOrEditTemplate", emptyTemplate);
        }

		[HttpPost("templates/create")]
		public async Task<IActionResult> CreateTemplate(TemplateModel templateData)
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
                    ViewData["Title"] = "Create a new template";
                    ModelState.AddModelError("", "Template name already used");
                    return View("CreateOrEditTemplate", templateData);
                }                
            }
            else
            {
                ViewData["Title"] = "Create a new template";
                return View("CreateOrEditTemplate", templateData);
            }
		}

        [HttpGet("templates/{id:int}/details")]
        public async Task<IActionResult> TemplateDetails(int id)
        {
            //we share the view between Create and Details (edit)            

            TemplateModel? templateData = await _repo.GetTemplateDataByIdAsync(id);

            if (templateData != null)
            {
                ViewData["Title"] = "Edit template";
                return View("CreateOrEditTemplate", templateData);
            }
            else
            {
                return NotFound();
            }            
        }

        [HttpPost("templates/{id:int}/details")]
        public async Task<IActionResult> TemplateDetails(int id, TemplateModel templateData)
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
                    ViewData["Title"] = "Edit template";
                    ModelState.AddModelError("", "Template name already used");
                    return View("CreateOrEditTemplate", templateData);
                }
            }
            else
            {
                ViewData["Title"] = "Edit template";
                return View("CreateOrEditTemplate", templateData);
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

        [HttpPost("courts/{id:int}/delete")]
        public async Task<IActionResult> DeleteCourt(int id)
        {
            int deletedRows = await _repo.DeleteCourtByIdAsync(id);

            if (deletedRows == 1)
            {
                return RedirectToAction(nameof(Courts));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("courts/create")]
        public IActionResult CreateCourt()
        {
            ViewData["Title"] = "Create a new court";
            return View("CreateOrEditCourt");
        }

        [HttpPost("courts/create")]
        public async Task<IActionResult> CreateCourt(CourtModel courtData)
        {
            if(ModelState.IsValid) 
            { 
                bool wasAdded = await _repo.AddCourtAsync(courtData);

                if(wasAdded)
                {
                    return RedirectToAction(nameof(Courts));
                }
                else
                {
                    ModelState.AddModelError("", "Court name already used");
                    ViewData["Title"] = "Create a new court";
                    return View("CreateOrEditCourt");
                }
            }
            else
            {
                ViewData["Title"] = "Create a new court";
                return View("CreateOrEditCourt");
            }            
        }

        [HttpGet("courts/{id:int}/details")]
        public async Task<IActionResult> CourtDetails(int id)
        {
            CourtModel? courtData = await _repo.GetCourtDataByIdAsync(id);

            if(courtData != null)
            {
                ViewData["Title"] = "Edit court";
                return View("CreateOrEditCourt", courtData);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("courts/{id:int}/details")]
        public async Task<IActionResult> CourtDetails(int id, CourtModel courtData)
        {
            if(ModelState.IsValid)
            {
                int updatedUsers = await _repo.UpdateCourtByIdAsync(id, courtData);

                if(updatedUsers == 1)
                {
                    return RedirectToAction(nameof(Courts));
                }
                else if(updatedUsers == 0)
                {                    
                    return NotFound();
                }
                else //-1
                {
                    ModelState.AddModelError("", "Court name already used");
                    ViewData["Title"] = "Edit court";
                    return View("CreateOrEditCourt");
                }
            }
            else
            {
                ViewData["Title"] = "Edit court";
                return View("CreateOrEditCourt", courtData);
            }            
        }

        [HttpGet("reservationslots")]
        public async Task<IActionResult> ReservationSlots()
        {           
            DateTime today = DateTime.Now.Date;            

			var model = new ReservationSlotsModel()
            {
                CourtItems = await _repo.GetAllCourtsForDropdownAsync(),
                TemplateItems = await _repo.GetAllTemplatesForDropdownAsync(),
                DefaultDate = today                
            };

            return View(model);
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
