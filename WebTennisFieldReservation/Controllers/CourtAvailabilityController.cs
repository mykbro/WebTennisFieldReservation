using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.CourtAvailability;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Services.SingleUserMailSender;

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
		public async Task<IActionResult> Reserve(CheckoutPostModel checkoutData, [FromServices] ISingleUserMailSender mailSender, [FromServices] IServiceScopeFactory scopeFactory)		//we reuse the same postModel
		{
			if(ModelState.IsValid) 
			{
				CreateReservationModel createData = new CreateReservationModel()
				{
					SlotIds = checkoutData.SlotIds,
					Timestamp = DateTimeOffset.Now,
					UserId = Guid.Parse(User.FindFirstValue(ClaimsNames.Id))
				};

				Guid? reservationId = await _repo.AddReservationFromSlotIdListAsync(createData);

				if(reservationId != null)
				{
					//we start a concurrent Task that sends a confirmation mail and then update the database
					_ = Task.Run(async () =>
						{
							//we send the confirmation mail
							string mailSubject = "Reservation confirmed";
							string mailBody = $"Your reservation #{reservationId} was confirmed !";

							await mailSender.SendEmailAsync(User.FindFirstValue(ClaimsNames.Email), mailSubject, mailBody);

							//we must request a new ICourtComplexRepository because the "external" _repo must not be accessed concurrently and may be already disposed 
							using (var scope = scopeFactory.CreateScope())
							{
								var repo = scope.ServiceProvider.GetService<ICourtComplexRepository>();
								await repo!.ConfirmReservationEmailSentAsync(reservationId.Value);
							}							
						});			

					//we return the confirmation page
					return RedirectToAction(nameof(ReservationSuccess), new { reservationId });
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

		[HttpGet("reserve/success")]
		public IActionResult ReservationSuccess(Guid? reservationId)
		{
			if(reservationId != null)
			{
				return View(reservationId.Value);
			}
			else
			{
				return BadRequest();
			}
		}

	}
}
