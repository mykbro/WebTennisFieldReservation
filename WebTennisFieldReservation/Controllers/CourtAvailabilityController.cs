using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.CourtAvailability;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Services.SingleUserMailSender;
using Microsoft.AspNetCore.Antiforgery;

namespace WebTennisFieldReservation.Controllers
{
	[Route("/courtavailability")]	
	public class CourtAvailabilityController : Controller
	{		

		[HttpGet("view")]
		public new IActionResult View()
		{
			var pageData = new AvailabilityPageModel() { Today = DateTime.Now };
			return View(pageData);
		}
	}
}
