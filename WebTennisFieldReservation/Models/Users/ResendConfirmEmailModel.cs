using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebTennisFieldReservation.Models.Users
{
    public class ResendConfirmEmailModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Not a valid email address")]
        [DisplayName("Email address")]
        public string Email { get; set; } = null!;
    }
}
