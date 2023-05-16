using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.CourtAvailability;
using WebTennisFieldReservation.Models.Reservations;
using WebTennisFieldReservation.Services.HttpClients;
using WebTennisFieldReservation.Services.SingleUserMailSender;
using WebTennisFieldReservation.Utilities.Paypal;

namespace WebTennisFieldReservation.Controllers
{
    [Route("/reservations")]
	[AutoValidateAntiforgeryToken]
	[Authorize]
	public class ReservationsController : Controller
	{
		private readonly ICourtComplexRepository _repo;

        public ReservationsController(ICourtComplexRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("checkout")]
		[IgnoreAntiforgeryToken]    //we use the post just for generating a view so we can ignore AF		
		public async Task<IActionResult> Checkout(CheckoutPostModel checkoutData)
		{
			if (ModelState.IsValid)
			{
				List<SlotModel> slotsData = await _repo.GetSlotDataByIdListAsync(checkoutData.SlotIds);

				//we check that the nr of retrieved slot matches the number of passed ids
				//this protects against duplicate ids and/or slots already taken/not available
				if (slotsData.Count == checkoutData.SlotIds.Count)
				{
					//we should probably do this ordering directly in the query
					var checkoutEntries = slotsData.OrderBy(entry => entry.Date).ThenBy(entry => entry.DaySlot);

					//we create a CheckoutToken (for payment idempotence)
					Guid checkoutToken = Guid.NewGuid();

					return View(new CheckoutPageModel(checkoutEntries.ToList(), checkoutToken));
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

		[HttpPost("create")]
		public async Task<IActionResult> Create(CheckoutPostModel checkoutData, [FromServices] PaypalCreateOrderClient createOrderClient, [FromServices] PaypalAuthenticationClient authClient)     //we reuse the same postModel
		{
			if (ModelState.IsValid)
			{
				Guid reservationId = checkoutData.CheckoutToken;
				Guid paymentToken = Guid.NewGuid();

				//we create an order to be placed with status "Created" in the db
				CreateReservationModel createData = new CreateReservationModel()
				{
					SlotIds = checkoutData.SlotIds,
					Timestamp = DateTimeOffset.Now,
					UserId = Guid.Parse(User.FindFirstValue(ClaimsNames.Id)),
					ReservationId = reservationId,
					PaymentConfirmationToken = paymentToken
				};

				//we try to insert it
				bool success = await _repo.AddReservationFromSlotIdListAsync(createData);

				if (success)
				{
					//we retrieve the reservation total amount (which we cannot know until we place the order)
					decimal totalAmount = await _repo.GetReservationTotalPriceAsync(reservationId);

					//we try to create a paypal order
					try 
					{						
						string authToken = await authClient.GetAuthTokenAsync();
						PaypalCreateOrderResponse paypalResponse = await createOrderClient.CreateOrderAsync(authToken, paymentToken, checkoutData.SlotIds.Count, totalAmount);
						
						//if we succeded we update the db with the payment id


					}
					catch(Exception ex)
					{

					}

					//we return the confirmation page
					return RedirectToAction(nameof(ReservationSuccess), new { createData.ReservationId });
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
		public async Task<IActionResult> Reserve(CheckoutPostModel checkoutData, [FromServices] ISingleUserMailSender mailSender/*, [FromServices] IServiceScopeFactory scopeFactory*/)     //we reuse the same postModel
		{
			if (ModelState.IsValid)
			{
				CreateReservationModel createData = new CreateReservationModel()
				{
					SlotIds = checkoutData.SlotIds,
					Timestamp = DateTimeOffset.Now,
					UserId = Guid.Parse(User.FindFirstValue(ClaimsNames.Id)),
					ReservationId = checkoutData.CheckoutToken,
					PaymentConfirmationToken = Guid.NewGuid(),
					PaymentId = "blablabla"
				};

				bool success = await _repo.AddReservationFromSlotIdListAsync(createData);

				if (success)
				{
					//we start a concurrent Task that sends a confirmation mail and then update the database
					_ = Task.Run(async () =>
					{
						//we send the confirmation mail
						string mailSubject = "Reservation confirmed";
						string mailBody = $"Your reservation #{createData.ReservationId} was confirmed !";

						await mailSender.SendEmailAsync(User.FindFirstValue(ClaimsNames.Email), mailSubject, mailBody);

						/*
						//we must request a new ICourtComplexRepository because the "external" _repo must not be accessed concurrently and may be already disposed 
						using (var scope = scopeFactory.CreateScope())
						{
							var repo = scope.ServiceProvider.GetService<ICourtComplexRepository>();
							await repo!.ConfirmReservationEmailSentAsync(reservationId.Value);
						}
						*/
					});

					//we return the confirmation page
					return RedirectToAction(nameof(ReservationSuccess), new { createData.ReservationId });
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
			if (reservationId != null)
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
