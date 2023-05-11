using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using WebTennisFieldReservation.Settings;
using SecurityToken = WebTennisFieldReservation.Utilities.SecurityToken;

namespace WebTennisFieldReservation.Services.TokenManager
{
    public class DataProtectionTokenManager : ITokenManager
    {
        private readonly IDataProtectionProvider _provider;

        public DataProtectionTokenManager(IDataProtectionProvider protectorProvider)
        {
            _provider = protectorProvider;
        }

        public string GenerateToken(string purpose, Guid userId, Guid securityStamp, DateTimeOffset issueTime)
        {
            SecurityToken token = new SecurityToken(userId, securityStamp, issueTime);
            byte[] tokenAsBytes = token.SerializeToBytes();
            IDataProtector protector = _provider.CreateProtector(purpose);

            return Base64UrlEncoder.Encode(protector.Protect(tokenAsBytes));
        }

        public SecurityToken RetrieveTokenFromString(string token, string purpose)
        {
            IDataProtector protector = _provider.CreateProtector(purpose);
            byte[] plainText = protector.Unprotect(Base64UrlEncoder.DecodeBytes(token));

            return SecurityToken.DeserializeFromBytes(plainText);
        }

        public bool ValidateToken(SecurityToken token, TimeSpan validityTS, Guid userId, Guid securityStamp)
        {
            return token.UserId == userId
                        && token.SecurityStamp == securityStamp
                        && DateTimeOffset.Now <= token.IssueTime.Add(validityTS);
        }
    }
}
