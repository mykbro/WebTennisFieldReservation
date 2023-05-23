using SmtpLibrary;
using System.Net.Mail;
using WebTennisFieldReservation.Settings;

namespace WebTennisFieldReservation.Services.SingleUserMailSender
{
    public class SingleUserPooledMailSender : ISingleUserMailSender, IDisposable
    {
        private readonly SmtpClientPoolSender _mailSender;
        private readonly string _fromAddress;

        public SingleUserPooledMailSender(MailSenderSettings settings)
        {
            string smtpPassword = File.ReadAllText(settings.PasswordFileName);
            SmtpClientFactory smtpClientFactory = new SmtpClientFactory(settings.HostName, settings.Port, settings.UseSSL, settings.User, smtpPassword);
            _mailSender = new SmtpClientPoolSender(smtpClientFactory, settings.StartingPoolSize, settings.MaxPoolSize);
            
            _fromAddress = settings.User;
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
