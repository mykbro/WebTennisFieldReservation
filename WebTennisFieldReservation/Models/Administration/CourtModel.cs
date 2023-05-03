using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebTennisFieldReservation.Models.Administration
{
    public class CourtModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DisplayName("Court name")]
        public string Name { get; set; } = null!;
    }
}
