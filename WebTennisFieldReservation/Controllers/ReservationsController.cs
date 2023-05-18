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

					return View(new CheckoutPageModel(checkoutEntries.ToList(), checkoutData.CheckoutToken));
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

				//we try to insert it (this guard against POST replays)
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

						//we update the reservation to PaymentCreated (inserting the paymentId)
						int reservationsUpdated = await _repo.UpdateReservationToPaymentCreatedAsync(reservationId, paypalResponse.id);		

						if(reservationsUpdated == 1)
						{
							//we redirect to the payment page
							return Redirect(paypalSettings.CheckoutPageUrl + "?token=" + paypalResponse.id);
						}
						else
						{
							//something went wrong, shouldn't be here
							return BadRequest();
						}
						
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
		public async Task<IActionResult> Confirm([Required] Guid reservationId, [Required] Guid confirmationToken, string token, [FromServices] PaypalCapturePaymentClient capturePaymentClient, [FromServices] PaypalAuthenticationClient authClient, [FromServices] ISingleUserMailSender mailSender)
		{			
			//we also need the confirmationToken, which only paypal can know, otherwise one can forge a reservationId during checkout
			//and call this endpoint with the payment token that he can see in the URL during paypal checkout (without approving the payment)
			//ofc the check will fail (payment not found) but this can disrupt the process
			//(even better we should save the confirmationToken hash or save nothing and rely on a DataProtection token to prevent db dumps attacks)
			if (ModelState.IsValid)
			{
				// we first try to update the reservation state from Placed to PaymentApproved;
				// this will:
				// 1- protect against a replay/concurrent call to this endpoint
				// 2- protect against any forgery thanks to the confirmationToken				
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

						//we first try to retrieve an authToken
						try
						{
							string authToken = await authClient.GetAuthTokenAsync();

							//we then try to make a Capture request
							try
							{
								PaypalOrderResponse response = await capturePaymentClient.CapturePayment(authToken, token);

								//we check if everything went fine
								if (response.status == "COMPLETED")
								{
									//we try to send a confirmation mail, if we don't succeed we continue anyway
									//we want to send it synchronously (awaiting) and not concurrently because we want (or at least try)
									//to give a feedback before updating the database to CONFIRMED because we can't be sure that the success page will be displayed
									string mailSubject = "Reservation confirmed";
									string mailBody = $"Your reservation #{reservationId} was confirmed !";

									try
									{
										await mailSender.SendEmailAsync(User.FindFirstValue(ClaimsNames.Email), mailSubject, mailBody);
									}
									catch (Exception ex)
									{
										//log the failure
									}

									//we then update the database
									updatesDone = await _repo.UpdateReservationToConfirmedAsync(reservationId);

									//profilactic check
									if (updatesDone == 1)
									{
										//we finally return a success page
										return RedirectToAction(nameof(ReservationSuccess), new { reservationId });
									}
									else
									{
										//something went terribly wrong if we're here...
										//we should probably delete the order and refund it
										return BadRequest();
									}
								}
								else
								{
									//we got a response but it's not good
									//we let the background service do the cleaning
									return BadRequest();
								}
							}
							catch (Exception ex)
							{
								// something went wrong, there are 2 cases:								
								// 1- bad answer from capturePayment (not a PaypalOrderResponse) -> no capture happened, we can give up
								// 2- NO ANSWER from capturePayment -> this is the trickiest, we can retry the capture a few times but if we fail we cannot say anything,
								//	  we leave the reservation Fulfilled but we need a background service to check for a previous capture and confirm or abort the order.
								//	  We can return a "Reservation pending" page to give at least some feedback to the user.

								//we let the background service do the cleaning
								return BadRequest();
							}
						}
						catch(Exception ex)
						{
							//authentication error/no answer from auth - >we can retry a few times and then give up
							return BadRequest();
						}								
					}
					else
					{
						//we weren't able to fulfill the reservation
						//we let the background service do the cleaning
						return BadRequest();
					}
				}
				else
				{
					//the order was a replay or a forging attempt
					return NotFound();
				}
			}
			else
			{
				return NotFound();
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
