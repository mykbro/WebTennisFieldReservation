using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    public class AdminUser
    {
        [Key]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        public bool IsSuperAdmin { get; set; }

        // Navigation
        public User User { get; set; } = null!;


    }
}
