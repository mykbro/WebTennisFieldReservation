namespace WebTennisFieldReservation.Entities
{
    //[PrimaryKey(nameof(CourtId), nameof(Day), nameof(DaySlot))]
    public class CourtAvailabilityOverride
    {
        public int CourtId { get; set; }
        public DateTime Day { get; set; }
        public byte DaySlot { get; set; }
    }
}
