using System.Net.Http.Headers;
using WebTennisFieldReservation.Exceptions;
using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Utilities.Paypal;

namespace WebTennisFieldReservation.Services.HttpClients
{
    public class PaypalCheckOrderClient
    {
        private readonly HttpClient _httpClient;

        public PaypalCheckOrderClient(HttpClient httpClient, PaypalApiSettings settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.CheckOrderUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.ClientTimeoutInSecs);
        }

        public async Task<string> CheckOrderAsync(string authToken, string paymentId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync(paymentId);

            if (response.IsSuccessStatusCode)
            {
                PaypalCheckOrderResponse? content = await response.Content.ReadFromJsonAsync<PaypalCheckOrderResponse>();
                
                if(content != null)
                {
                    return content.status;
                }
                else
                {
                    //shouldn't reach here
                    throw new PaypalCheckOrderException();
                }
            }
            else
            {
                throw new PaypalCheckOrderException();
            }
        }
    }

}
