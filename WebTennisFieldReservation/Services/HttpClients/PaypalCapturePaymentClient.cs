using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Utilities.Paypal;

namespace WebTennisFieldReservation.Services.HttpClients
{
	public class PaypalCapturePaymentClient
	{
		private readonly HttpClient _httpClient;

        public PaypalCapturePaymentClient(HttpClient httpClient, PaypalApiSettings settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.CapturePaymentUrl);
        }

        public async Task<PaypalOrderResponse> 


    }
}
