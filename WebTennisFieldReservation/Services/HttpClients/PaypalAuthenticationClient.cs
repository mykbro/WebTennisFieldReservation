﻿using WebTennisFieldReservation.Settings;
using System.Net.Http.Headers;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Exceptions;
using WebTennisFieldReservation.Utilities;
using System.Text.Json;
using System.Buffers.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebTennisFieldReservation.Services.HttpClients
{
    public class PaypalAuthenticationClient
    {
        private static readonly HttpContent RequestContent;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

		private HttpClient _httpClient;

        //static constructor
        static PaypalAuthenticationClient()
        {
            RequestContent = new FormUrlEncodedContent(new Dictionary<string, string>() { { "grant_type", "client_credentials" } });
            RequestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
		}

		public PaypalAuthenticationClient(HttpClient httpClient, PaypalApiSettings paypalApiSettings)
        {
			//instead of calculating base64AuthCred every time we should calculate it lazily on the first time, 
			//and then store it in a static variable using a double-checked lock
			string basicAuthCred = $"{paypalApiSettings.ClientId}:{paypalApiSettings.Secret}";
			byte[] basicAuthCredBytes = Encoding.UTF8.GetBytes(basicAuthCred);
            string base64AuthCred = Convert.ToBase64String(basicAuthCredBytes);

			_httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(paypalApiSettings.AuthUrl);            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthCred);                     
        }


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <exception cref="PaypalAuthFailedException"></exception>
		/// <exception cref="HttpRequestException"></exception>
		public async Task<string> GetAuthToken()
        { 
			HttpResponseMessage httpResponse = await _httpClient.PostAsync("", RequestContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                PaypalAuthResponse? response = await httpResponse.Content.ReadFromJsonAsync<PaypalAuthResponse>(JsonOptions);

                if(response != null)
                {
                    return response.Access_Token;
                }
                else
                {
					throw new PaypalAuthFailedException();
				}
            }
            else
            {
                throw new PaypalAuthFailedException();
            }

        }
    }
}
