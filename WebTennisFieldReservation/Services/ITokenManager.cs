namespace WebTennisFieldReservation.Services
{
    public interface ITokenManager
    {
        public string GenerateToken(string purpose, Guid userId, Guid securityStamp);
        public bool ValidateToken(string token, TimeSpan validityTS, string purpose, Guid userId, Guid securityStamp);
    }
}
