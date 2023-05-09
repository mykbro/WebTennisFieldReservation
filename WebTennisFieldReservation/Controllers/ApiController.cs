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
        [AllowAnonymous]
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
                bool addOk = await _repo.AddReservationSlotsAsync(slotsData);

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

		[HttpGet("slots")]
		public async Task<IActionResult> Slots(int? courtId, DateTime? mondayDateUtc)
		{
            //we need to check that courtId and mondayDateUtc are valid
            //we skip the IsMonday date check
            if (courtId != null && mondayDateUtc != null)
            {
				DateTime mondayDate = mondayDateUtc.Value.ToLocalTime();  //we convert to localTime

				List<ReservationSlotModel> slotModels = await _repo.GetReservationSlotsForCourtBetweenDatesAsync(courtId.Value, mondayDate, mondayDate.AddDays(6));
				return Json(slotModels);
			}
            else
            {
                return BadRequest();
            }
            
		}

        [HttpGet("templateentries")]
        public async Task<IActionResult> TemplateEntries(int? templateId)
        {
            if(templateId != null)
            {
				List<ReservationSlotModel> slotModels = await _repo.GetReservatonSlotsFromTemplateAsync(templateId.Value);
                return Json(slotModels);
            }
            else
            {
                return BadRequest();
            }
        }

		[HttpGet("availability")]
		[AllowAnonymous]
		public async Task<IActionResult> Availability(DateTime? date)
		{
			if(date != null)
            {
                List<SlotAvailabilityModel> slots = await _repo.GetAllCourtsSlotAvailabilityForDate(date.Value);
                return Json(slots);
            }
            else
            {
                return BadRequest();
            }
		}


	}
}
