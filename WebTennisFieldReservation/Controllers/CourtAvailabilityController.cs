using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Data;
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
		public IActionResult Checkout(CheckoutPostModel postData)
		{
			if(ModelState.IsValid)
			{
				return View();
			}
			else
			{
				return BadRequest();
			}
		}
	}
}
