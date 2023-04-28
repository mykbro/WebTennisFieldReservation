using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    [Index(nameof(CourtId), nameof(Day), nameof(DaySlot), IsUnique = true)]
    //[PrimaryKey(nameof(ReservationId), nameof(ReservationEntryWeakId))]
    public class ReservationEntry
    {
        [ForeignKey(nameof(Reservation))]
        public Guid ReservationId { get; set;}

        [Required]
        public int ReservationEntryWeakId { get; set;}

        [ForeignKey(nameof(Court))]
        public int CourtId { get; set;}
        [Required]
        public DateTime Day { get; set; }
        [Required]
        public byte DaySlot { get; set; }


        // Navigation
        public Reservation Reservation { get; set; } = null!;
        public Court Court { get; set; } = null!;
    }
}
