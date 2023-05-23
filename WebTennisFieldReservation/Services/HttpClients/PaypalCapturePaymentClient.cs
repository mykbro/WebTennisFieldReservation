using Microsoft.AspNetCore.Http;
using WebTennisFieldReservation.Exceptions;
using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Utilities.Paypal;
using System.Net.Http.Headers;

namespace WebTennisFieldReservation.Services.HttpClients
{
	public class PaypalCapturePaymentClient
	{
		private readonly HttpClient _httpClient;

        public PaypalCapturePaymentClient(HttpClient httpClient, PaypalApiSettings settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.CapturePaymentUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.ClientTimeoutInSecs);
        }

        public async Task<PaypalOrderResponse> CapturePaymentAsync(string authToken, string paymentId)
        {
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{paymentId}/capture", new {});     //by sending no content we capture the whole amount

            if (response.IsSuccessStatusCode)
            {
                PaypalOrderResponse? responseContent = await response.Content.ReadFromJsonAsync<PaypalOrderResponse>();

				if (responseContent != null)
				{
					return responseContent;
				}
				else
				{
					throw new PaypalCapturePaymentException();
				}
			}
			else
			{
				string debug = await response.Content.ReadAsStringAsync();
				throw new PaypalCapturePaymentException();
			}
		}


    }
}
