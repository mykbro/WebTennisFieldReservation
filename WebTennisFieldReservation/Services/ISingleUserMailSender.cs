namespace WebTennisFieldReservation.Services
{
    public interface ISingleUserMailSender
    {
        // Allows to capture a single user and send single msgs from that user
        public Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
