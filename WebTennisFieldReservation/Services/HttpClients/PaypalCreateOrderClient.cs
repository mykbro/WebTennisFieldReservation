using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Utilities.Paypal;
using System.Net.Http.Headers;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Constants;
using WebTennisFieldReservation.Exceptions;

namespace WebTennisFieldReservation.Services.HttpClients
{
	public class PaypalCreateOrderClient
	{
		private readonly HttpClient _httpClient;

        public PaypalCreateOrderClient(HttpClient httpClient, PaypalApiSettings settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.CreateOrderUrl);            
        }

        public async Task<PaypalOrderResponse> CreateOrderAsync(string authToken, Guid reservationId, Guid confirmationToken, int numSlots, decimal totalAmount)
        {
            //we add the auth token...
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            //...and the idempotency token
            _httpClient.DefaultRequestHeaders.Add(HttpHeadersNames.PayPalRequestId, confirmationToken.ToString());

            //we create and populate the PaypalCreateOrderRequest object
            var orderData = new {
                intent = "AUTHORIZE",
                purchase_units = new[] {
                    new {
                        description = $"Reservation for {numSlots} slots",
                        amount = new {
                            currency_code = "EUR",
                            value = totalAmount.ToString().Replace(',', '.')           //paypal wants the DOT
                        }
                    }
                },
                payment_source = new {
                    paypal = new {
                        experience_context = new {
                            return_url = $"http://localhost/reservations/confirm?reservationId={reservationId}&confirmationToken={confirmationToken}"
                        }
                    }
                }
			};

			//we try to make the request
			HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("", orderData);

            //we check the response
            if (httpResponse.IsSuccessStatusCode)
            {
                PaypalOrderResponse? createOrderResponse = await httpResponse.Content.ReadFromJsonAsync<PaypalOrderResponse>(HttpClientsConsts.JsonOptions);
                if(createOrderResponse != null)
                {
                    return createOrderResponse;
                }
                else
                {
					throw new PaypalCreateOrderFailedException();
				}
            }
            else
            {
                string debug = await httpResponse.Content.ReadAsStringAsync();
                throw new PaypalCreateOrderFailedException();
            }
        }
    }
}
