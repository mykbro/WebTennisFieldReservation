using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebTennisFieldReservation.Models.Users
{
    public class PasswordResetRequestModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password fields must match")]
        [DisplayName("Password confirmation")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;
    }
}
