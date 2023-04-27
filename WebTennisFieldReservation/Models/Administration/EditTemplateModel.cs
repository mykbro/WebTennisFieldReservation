using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Administration
{
    public class EditTemplateModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(64, ErrorMessage = "{0} must be up to {1} chars long")]
        [DisplayName("Template name")]
        public string Name { get; set; } = null!;

        [StringLength(256, ErrorMessage = "{0} must be up to {1} chars long")]
        [DisplayName("Template description")]
        public string? Description { get; set; }

    }
}
