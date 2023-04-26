using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Users
{
    public class EditUserDataModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [RegularExpression("^[A-Z][A-Z, a-z, ]*$", ErrorMessage = "{0} should only contain letters and start with an uppercase")]
        [DisplayName("First name")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [RegularExpression("^[A-Z][A-Z, a-z, ]*$", ErrorMessage = "{0} should only contain letters and start with an uppercase")]
        [DisplayName("Last name")]
        public string LastName { get; set; } = null!;

        [StringLength(64, MinimumLength = 4, ErrorMessage = "{0} must be between {1} and {2} chars")]
        [DisplayName("Address")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Not a valid email address")]
        [DisplayName("Email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Date)]
        [DisplayName("Birth date")]
        public DateTime BirthDate { get; set; }
    }
}
