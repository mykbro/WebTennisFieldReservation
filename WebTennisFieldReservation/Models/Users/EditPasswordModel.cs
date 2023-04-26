using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Users
{
    public class EditPasswordModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.Password)]
        [DisplayName("Current password")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.Password)]
        [DisplayName("New password")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.Password)]
        [DisplayName("Confirm new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Password fields must match")]
        public string NewPasswordConfirmation { get; set; } = null!;
    }
}
