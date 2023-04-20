namespace WebTennisFieldReservation.Services
{
    public interface IPasswordHasher
    {
        public (byte[] Password, byte[] Salt) GeneratePasswordAndSalt(string pwd);
        public int Iterations { get; }
    }
}
