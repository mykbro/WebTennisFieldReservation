using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.Api;

namespace WebTennisFieldReservation.Controllers
{
    [Route("/api")]
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = AuthorizationPoliciesNames.IsAdmin)]
    public class ApiController : Controller
    {
        private readonly ICourtComplexRepository _repo;

        public ApiController(ICourtComplexRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("courts")]
        public async Task<IActionResult> Courts()
        {
            List<CourtSelectionModel> payload = await _repo.GetAllCourtsForDropdownAsync();
            return Json(payload);
        }

        [HttpGet("templates")]
        public async Task<IActionResult> Templates()
        {
            List<TemplateSelectionModel> payload = await _repo.GetAllTemplatesForDropdownAsync();
            return Json(payload);
        }

        [HttpPost("slots")]        
		public async Task<IActionResult> Slots([FromBody] PostedReservationSlotsModel slotsData)
		{
            if(ModelState.IsValid)
            {                
                bool addOk = await _repo.AddReservationSlots(slotsData);

			    if(addOk)
                {
                    return Ok();
                }
                else
                {
					return BadRequest();
				}
			}
            else
            {
                return BadRequest();
            }
            
		}
	}
}
