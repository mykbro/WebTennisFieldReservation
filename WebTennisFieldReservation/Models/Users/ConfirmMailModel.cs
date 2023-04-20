using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Users
{
    public class ConfirmMailModel
    {
        [Required]
        public string Token { get; set; } = null!;
    }
}
