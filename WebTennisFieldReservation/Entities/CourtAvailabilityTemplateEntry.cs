using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace WebTennisFieldReservation.Entities
{    
    public class CourtAvailabilityTemplateEntry
    {
        //composite key defined in OnModelCreating
        [ForeignKey(nameof(CourtAvailabilityTemplate))]
        public int TemplateId { get; set; }
        public byte WeekDay { get; set; }       //0 -> Monday ... 6 -> Sunday
        public byte DaySlot { get; set; }       //0 -> [00:00 - 00:59] ... 23 -> [23:00 - 23:59]

        //navigation
        public CourtAvailabilityTemplate CourtAvailabilityTemplate { get; set; } = null!;
    }
}
