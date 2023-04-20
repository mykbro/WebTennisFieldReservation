using WebTennisFieldReservation.Utilities;

namespace WebTennisFieldReservation.Services
{
    public interface ITokenManager
    {
        public string GenerateToken(string purpose, Guid userId, Guid securityStamp, DateTimeOffset issueTime);
        public SecurityToken RetrieveTokenFromString(string token, string purpose);
        public bool ValidateToken(SecurityToken token, TimeSpan validityTS, Guid userId, Guid securityStamp);
    }
}
