using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Data;

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
			return View();
		}
	}
}
