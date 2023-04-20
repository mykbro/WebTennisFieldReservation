using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class CourtAvailabilityTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Name { get; set; } = null!;
    }
}
