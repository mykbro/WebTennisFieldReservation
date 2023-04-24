namespace WebTennisFieldReservation.Services
{
    public interface IPasswordHasher
    {
        public (byte[] Password, byte[] Salt) GeneratePasswordAndSalt(string pwd);
        public bool ValidatePassword(string pwdToValidate, byte[] pwdHash, byte[] pwdSalt, int iters);
        public int Iterations { get; }
    }
}
