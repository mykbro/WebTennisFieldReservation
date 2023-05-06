using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Court
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        // Navigation 
        public ICollection<ReservationEntry> ReservationEntries { get; set;} = new List<ReservationEntry>();
		public ICollection<ReservationSlot> ReservationSlots { get; set; } = new List<ReservationSlot>();
	}
}
