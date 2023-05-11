using SmtpLibrary;
using System.Net.Mail;

namespace WebTennisFieldReservation.Services.SingleUserMailSender
{
    public class SingleUserPooledMailSender : ISingleUserMailSender, IDisposable
    {
        private readonly SmtpClientPoolSender _mailSender;
        private readonly string _fromAddress;

        public SingleUserPooledMailSender(SmtpClientPoolSender mailSender, string fromAddress)
        {
            _mailSender = mailSender;
            _fromAddress = fromAddress;
        }

        public void Dispose()
        {
            _mailSender.Dispose();
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage message = new MailMessage(_fromAddress, email, subject, htmlMessage) { IsBodyHtml = true };
            return _mailSender.SendMailAsync(message);
        }


    }
}
