namespace WebTennisFieldReservation.Models.Users
{
    public class DataForLoginCheckModel
    {
        public Guid Id { get; set; }
        public Guid SecuritStamp { get; set; }
        public byte[] PwdHash { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
        public int Iters { get; set; }
    }
}
