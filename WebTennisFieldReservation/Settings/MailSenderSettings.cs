namespace WebTennisFieldReservation.Settings
{
    public class MailSenderSettings
    {
        public string HostName { get; set; } = null!;
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string User { get; set; } = null!;
        public string? Password { get; set; }
    }
}
