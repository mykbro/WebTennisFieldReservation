﻿using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Models.Api
{
    public class PostedReservationSlotsModel
    {
        [Required]
        public DateTime MondayDateUtc { get; set; }

        [Required]
        public int CourtId { get; set; }

        [Required]
        public List<ReservationSlotModel> SlotEntries { get; set; } = null!;

    }
}
