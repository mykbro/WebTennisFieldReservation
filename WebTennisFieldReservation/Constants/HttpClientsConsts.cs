using System.Text.Json;

namespace WebTennisFieldReservation.Constants
{
	public static class HttpClientsConsts
	{
		public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
	}
}
