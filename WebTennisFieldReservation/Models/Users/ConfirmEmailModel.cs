using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Users
{
    public class ConfirmEmailModel
    {
        [Required]
        public string Token { get; set; } = null!;
    }
}
