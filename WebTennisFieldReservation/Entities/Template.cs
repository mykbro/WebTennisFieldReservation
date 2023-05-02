using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Template
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Name { get; set; } = null!;
       
        [StringLength(256)]
        public string? Description { get; set; }

        //navigation propertiese
        public ICollection<TemplateEntry> TemplateEntries { get; set; } = new List<TemplateEntry>();        
    }
}
