using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.CourtAvailability;

namespace WebTennisFieldReservation.Controllers
{
	[Route("/courtavailability")]
	public class CourtAvailabilityController : Controller
	{		
		private static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions() { NumberHandling = JsonNumberHandling.AllowReadingFromString };
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
				try
				{
					CheckoutJsonPayloadModel? payload = JsonSerializer.Deserialize<CheckoutJsonPayloadModel>(postData.JsonPayload, SerializationOptions);
					//we don't need an additional check on the payload (like DaySlot in [0-23]) because we're checking against db availability anyway

					//if here payload should always be != null
					if (payload != null)
					{
						//we need to retrieve the prices from the db

					}
					else
					{

					}

					return View();
				}
				catch (SerializationException ex)
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
