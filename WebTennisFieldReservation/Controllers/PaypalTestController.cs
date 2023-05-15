using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Services.HttpClients;

namespace WebTennisFieldReservation.Controllers
{
	public class PaypalTestController : Controller
	{

        public async Task<IActionResult> Index([FromServices] PaypalAuthenticationClient paypalClient)
		{
			try
			{
				string authToken = await paypalClient.GetAuthToken();
				return Ok();
			}
			catch
			{
				return BadRequest();
			}
		}
	}
}
