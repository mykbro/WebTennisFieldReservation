using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.CourtAvailability;

namespace WebTennisFieldReservation.Controllers
{
	[Route("courtavailability")]
	public class CourtAvailabilityController : Controller
	{
		private readonly ICourtComplexRepository _repo;

		public CourtAvailabilityController(ICourtComplexRepository repo)
		{
			_repo = repo;
		}
		
		public IActionResult Index()
		{
			var pageData = new AvailabilityPageModel() { Today = DateTime.Now };
			return View(pageData);
		}
	}
}
