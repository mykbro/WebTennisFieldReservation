using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.CourtAvailability;

namespace WebTennisFieldReservation.Controllers
{
	[Route("/courtavailability")]
	public class CourtAvailabilityController : Controller
	{
		private readonly ICourtComplexRepository _repo;

		public CourtAvailabilityController(ICourtComplexRepository repo)
		{
			_repo = repo;
		}

		[HttpGet("view")]
		public new IActionResult View()
		{
			var pageData = new AvailabilityPageModel() { Today = DateTime.Now };
			return View(pageData);
		}

		[HttpPost("checkout")]
		[IgnoreAntiforgeryToken]    //we use the post just for generating a view so we can ignore AF
		[Authorize]					//no particular policy needed
		public async Task<IActionResult> Checkout(CheckoutPostModel checkoutData)
		{
			if(ModelState.IsValid)
			{
				List<SlotModel> slotsData = await _repo.GetSlotDataByIdListAsync(checkoutData.SlotIds);

				//we check that the nr of retrieved slot matches the number of passed ids
				//this protects against duplicate ids and/or slots already taken/not available
				if(slotsData.Count == checkoutData.SlotIds.Count)
				{
					//we should probably do this ordering directly in the query
					var checkoutEntries = slotsData.OrderBy(entry => entry.Date).ThenBy(entry => entry.DaySlot);
					
					return View(new CheckoutPageModel(checkoutEntries.ToList()));
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

		[HttpPost("reserve")]
		[Authorize]
		public IActionResult Reserve()
		{
			return Ok();
		}
	}
}
