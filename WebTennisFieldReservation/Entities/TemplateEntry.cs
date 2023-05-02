using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace WebTennisFieldReservation.Entities
{    
    public class TemplateEntry
    {
        //composite key defined in OnModelCreating
        [ForeignKey(nameof(Template))]
        public int TemplateId { get; set; }        
        public int WeekSlot { get; set; }  //[0 - 167] where 0 is Monday 00:00-01:00, 1 is Monday 01:00-02:00, ... , 167 is Sunday 23:00-24:00 (can be a byte)

        [Required]
        [Column(TypeName="decimal(18,2)")]
        public decimal Price{ get; set; }

        //navigation
        public Template Template { get; set; } = null!;
    }
}
