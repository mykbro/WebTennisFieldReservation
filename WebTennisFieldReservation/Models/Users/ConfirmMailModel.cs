using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Users
{
    public class ConfirmMailModel
    {
        [Required]
        public string TokenString { get; set; } = null!;
    }
}
