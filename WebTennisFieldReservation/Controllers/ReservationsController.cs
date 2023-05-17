using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.CourtAvailability;
using WebTennisFieldReservation.Models.Reservations;
using WebTennisFieldReservation.Services.HttpClients;
using WebTennisFieldReservation.Services.SingleUserMailSender;
using WebTennisFieldReservation.Settings;
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
		public async Task<IActionResult> Create(CheckoutPostModel checkoutData, [FromServices] PaypalCreateOrderClient createOrderClient, [FromServices] PaypalAuthenticationClient authClient, [FromServices] PaypalApiSettings paypalSettings)     //we reuse the same postModel
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
						PaypalOrderResponse paypalResponse = await createOrderClient.CreateOrderAsync(authToken, reservationId, paymentToken, checkoutData.SlotIds.Count, totalAmount);

						//if we succeded we update the db with the payment id
						await _repo.UpdateReservationToPaymentCreatedAsync(reservationId, paypalResponse.id);

						//and we redirect to the payment page
						return Redirect(paypalSettings.CheckoutPageUrl + "?token=" + paypalResponse.id);
					}
					catch(Exception ex)
					{
						//something went wrong
						return BadRequest();
					}
					
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

		[HttpGet("confirm")]
		[AllowAnonymous]	//we allow the confirmation even when logged out
		public async Task<IActionResult> Confirm([Required] Guid reservationId, [Required] Guid confirmationToken, string token, [FromServices] PaypalCapturePaymentClient capturePaymentClient, [FromServices] PaypalAuthenticationClient authClient)
		{			
			//we also need the confirmationToken, which only paypal can know, otherwise one can forge a reservationId during checkout
			//and call this endpoint with a random payment token that will be saved in the database and checked for capturing;
			//ofc the check will fail (payment not found) but this can disrupt the process
			//(even better we should save the confirmationToken hash or save nothing and rely on a DataProtection token to prevent db dumps attacks)
			if (ModelState.IsValid)
			{
				// we first try to update the reservation state from Placed to PaymentApproved;
				// this will:
				// 1- protect against a replay/concurrent call to this endpoint
				// 2- protect against any forgery thanks to the confirmationToken
				// 3- atomically add the paymentId to the reservation
				int updatesDone = await _repo.UpdateReservationToPaymentApprovedAsync(reservationId, confirmationToken, token);

				//we check if the update happened
				if (updatesDone == 1)
				{
					// we try to confirm the order marking the slots as taken,
					// we'll fail if any of the slots is taken or has been taken in the meanwhile (between the checkout and now)					
					bool reservationFulfilled = await _repo.TryToFulfillReservationAsync(reservationId);

					if(reservationFulfilled)
					{
						//we have to capture the payment, send a confirmation mail, update the db and return a success page (all in this order)
						try
						{
							string authToken = await authClient.GetAuthTokenAsync();
							PaypalOrderResponse response = await capturePaymentClient.CapturePayment(authToken, token);

							//we check if everything went fine
							if(response.status == "COMPLETED")
							{
								
							}

							return Ok();
						}
						catch(Exception ex)
						{
							//something went wrong
							return BadRequest();
						}						
					}
					else
					{
						//we weren't able to fulfill the reservation
						//we should remove the order from the db etc etc...
						return BadRequest();
					}
				}
				else
				{
					//the order was already deleted/authorized or there was a forging attempt
					return NotFound();
				}
			}
			else
			{
				return NotFound();
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

		[HttpGet("confirm/success")]
		[AllowAnonymous]
		public IActionResult ReservationSuccess([Required] Guid reservationId)
		{
			if (ModelState.IsValid)
			{
				return View(reservationId);
			}
			else
			{
				return NotFound();
			}
		}
	}
}
