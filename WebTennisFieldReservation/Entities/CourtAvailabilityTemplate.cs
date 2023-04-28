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
       
        [StringLength(256)]
        public string? Description { get; set; }

        //navigation propertiese
        public ICollection<CourtAvailabilityTemplateEntry> CourtAvailabilityTemplateEntries { get; set; } = new List<CourtAvailabilityTemplateEntry>();
        public ICollection<Court> Courts { get; set; } = new List<Court>();
    }
}
