using System.Security.Cryptography;

namespace WebTennisFieldReservation.Services
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private readonly int _numIters;

        public int Iterations => _numIters;

        public Pbkdf2PasswordHasher(int numIters)
        {
            _numIters = numIters;
        }

        public (byte[] PasswordHash, byte[] Salt) GeneratePasswordAndSalt(string pwd)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(32);
            byte[] pwdBytes = Rfc2898DeriveBytes.Pbkdf2(pwd, saltBytes, _numIters, HashAlgorithmName.SHA256, 32);

            return (pwdBytes, saltBytes);
        }

        public bool ValidatePassword(string pwdToValidate, byte[] pwdHash, byte[] pwdSalt, int iters)
        {
            byte[] tempHash = Rfc2898DeriveBytes.Pbkdf2(pwdToValidate, pwdSalt, iters, HashAlgorithmName.SHA256, 32); 
            return ByteArrayCompare(pwdHash, tempHash);
        }

        private static bool ByteArrayCompare(byte[] first, byte[] second)
        {  

            if(first.Length != second.Length)
            {
                return false;
            }
            else
            { 
                // we should probably check the whole array anyway for timing attacks
                for(int i = 0; i < first.Length; i++)
                {
                    if (first[i] != second[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
