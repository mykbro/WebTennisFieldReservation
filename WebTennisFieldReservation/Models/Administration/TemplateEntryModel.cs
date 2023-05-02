using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Administration
{
    public class TemplateEntryModel
    { 
        
        [Required]
        [Range(0, 167)]
        public int WeekSlot { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
