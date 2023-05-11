using SmtpLibrary;
using System.Net.Mail;

namespace WebTennisFieldReservation.Services.SingleUserMailSender
{
    public class ConsoleMailSender : ISingleUserMailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(htmlMessage);
            Console.ForegroundColor = previous;

			return Task.CompletedTask;
        }

    }
}
