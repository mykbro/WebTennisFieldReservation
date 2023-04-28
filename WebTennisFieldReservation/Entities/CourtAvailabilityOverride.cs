using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    //[PrimaryKey(nameof(CourtId), nameof(Day), nameof(DaySlot))]
    public class CourtAvailabilityOverride
    {
        [ForeignKey(nameof(Court))]
        public int CourtId { get; set; }
        public DateTime Day { get; set; }
        public byte DaySlot { get; set; }

        //navigation
        public Court Court { get; set; } = null!;
    }
}
