using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace WebTennisFieldReservation.Entities
{
    //[PrimaryKey(nameof(CourtId), nameof(WeekDay), nameof(DaySlot))]
    public class CourtAvailabilityTemplateEntry
    {        
        public int CourtId { get; set; }
        public byte WeekDay { get; set; }
        public byte DaySlot { get; set; }       //0 -> [00:00 - 00:59] ... 23 -> [23:00 - 23:59]
    }
}
