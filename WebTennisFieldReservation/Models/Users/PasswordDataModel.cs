namespace WebTennisFieldReservation.Models.Users
{
    public class PasswordDataModel
    {
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
        public int Iters { get; set; }
    }
}
