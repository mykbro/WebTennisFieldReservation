using System.ComponentModel.DataAnnotations;
using WebTennisFieldReservation.ValidationAttributes;

namespace WebTennisFieldReservation.Models.Administration
{
    public class TemplateEntryModel
    {         
        [Required]        
        public bool IsSelected { get; set; }

        [PositiveAndNotNullIfSelected(ErrorMessage = "A positive price must be provided for selected entries")]
        public decimal? Price { get; set; }
    }
}
