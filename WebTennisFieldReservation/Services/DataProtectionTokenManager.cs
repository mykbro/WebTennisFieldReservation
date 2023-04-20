using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using WebTennisFieldReservation.Settings;
using SecurityToken = WebTennisFieldReservation.Utilities.SecurityToken;

namespace WebTennisFieldReservation.Services
{
    public class DataProtectionTokenManager : ITokenManager
    {       
        private readonly IDataProtectionProvider _provider;

        public DataProtectionTokenManager(IDataProtectionProvider protectorProvider)
        {           
            _provider = protectorProvider;                        
        }

        public string GenerateToken(string purpose, Guid userId, Guid securityStamp)
        {
            SecurityToken token = new SecurityToken(userId, securityStamp, DateTimeOffset.Now);
            byte[] tokenAsBytes = token.SerializeToBytes();
            IDataProtector protector = _provider.CreateProtector(purpose);

            return Base64UrlEncoder.Encode(protector.Protect(tokenAsBytes));
        }

        public bool ValidateToken(string token, TimeSpan validityTS, string purpose, Guid userId, Guid securityStamp)
        {
            try
            {
                IDataProtector protector = _provider.CreateProtector(purpose);
                byte[] plainText = protector.Unprotect(Base64UrlEncoder.DecodeBytes(token));
                SecurityToken candidateToken = SecurityToken.DeserializeFromBytes(plainText);

                return candidateToken.UserId == userId
                        && candidateToken.SecurityStamp == securityStamp
                        && DateTimeOffset.Now <= candidateToken.IssueTime.Add(validityTS);
            }
            catch(CryptographicException ex)
            {
                return false;
            }                 
        }
    }
}
