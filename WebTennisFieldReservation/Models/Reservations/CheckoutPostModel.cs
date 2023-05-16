using System.ComponentModel.DataAnnotations;
using WebTennisFieldReservation.ValidationAttributes;

namespace WebTennisFieldReservation.Models.Reservations
{
    public class CheckoutPostModel
    {
        [Required]
        [CollectionLength(minSize: 1)]
        public List<int> SlotIds { get; set; } = null!;

        [Required]
        public Guid CheckoutToken { get; set; }
    }
}
