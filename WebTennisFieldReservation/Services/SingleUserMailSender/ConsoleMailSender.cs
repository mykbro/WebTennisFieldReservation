using SmtpLibrary;
using System.Net.Mail;

namespace WebTennisFieldReservation.Services.SingleUserMailSender
{
    public class ConsoleMailSender : ISingleUserMailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine(htmlMessage);
            return Task.CompletedTask;
        }

    }
}
