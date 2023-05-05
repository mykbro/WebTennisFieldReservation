using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Api
{
    public class PostedReservationSlotsModel
    {
        [Required]
        public DateTime MondayDateUtc { get; set; }

        [Required]
        public List<ReservationSlotEntry> SlotEntries { get; set; } = null!;

    }
}
