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

        public (byte[] Password, byte[] Salt) GeneratePasswordAndSalt(string pwd)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(32);
            byte[] pwdBytes = Rfc2898DeriveBytes.Pbkdf2(pwd, saltBytes, _numIters, HashAlgorithmName.SHA256, 32);

            return (pwdBytes, saltBytes);
        }
    }
}
