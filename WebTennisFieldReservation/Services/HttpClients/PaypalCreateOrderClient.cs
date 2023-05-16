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

        public async Task<PaypalCreateOrderResponse> CreateOrderAsync(string authToken, Guid paymentToken, int numSlots, decimal totalAmount)
        {
            //we add the auth token...
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            //...and the idempotency token
            _httpClient.DefaultRequestHeaders.Add(HttpHeadersNames.PayPalRequestId, paymentToken.ToString());

            //we create and populate the PaypalCreateOrderRequest object
            PaypalCreateOrderRequest orderData = new PaypalCreateOrderRequest()
            {
                intent = "AUTHORIZE",
                purchase_units = new List<PaypalPurchaseUnit>()
                {
                    new PaypalPurchaseUnit()
                    {
                        description = $"Reservation for {numSlots} slots",
                        amount = new PaypalAmount()
                        {
                            currency_code = "EUR",
                            value = totalAmount.ToString().Replace(',' , '.')           //paypal wants the DOT
                        }
                    }
                },
                payment_source = new PaypalPaymentSource
                {
                    paypal = n
                    {

                    }
                }
			};

			//we try to make the request
			HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync<PaypalCreateOrderRequest>("", orderData);

            //we check the response
            if (httpResponse.IsSuccessStatusCode)
            {
                PaypalCreateOrderResponse? createOrderResponse = await httpResponse.Content.ReadFromJsonAsync<PaypalCreateOrderResponse>(HttpClientsConsts.JsonOptions);
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
