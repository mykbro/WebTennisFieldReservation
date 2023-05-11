namespace WebTennisFieldReservation.Services.PasswordHasher
{
    public interface IPasswordHasher
    {
        public (byte[] PasswordHash, byte[] Salt) GeneratePasswordAndSalt(string pwd);
        public bool ValidatePassword(string pwdToValidate, byte[] pwdHash, byte[] pwdSalt, int iters);
        public int Iterations { get; }
    }
}
