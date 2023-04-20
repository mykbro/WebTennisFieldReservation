using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
    [Index(nameof(Email), IsUnique = true)]    
    public class User
    {
        [Key]        
        public Guid Id { get; set; }

        [Required]
        [StringLength(64)]        
        public string FirstName { get; set; } = null!;
        
        [Required]
        [StringLength(64)]
        public string LastName { get; set; } = null!;
        
        [StringLength(64)]
        public string? Address { get; set; }

        [Required]
        [StringLength(64)]        
        public string Email { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }

        #region __SecurityInfo

        [Required]
        [Column(TypeName = "binary(32)")]
        public byte[] PwdHash { get; set; } = null!;

        [Required]
        [Column(TypeName = "binary(32)")]
        public byte[] PwdSalt { get; set; } = null!;

        [Required]
        public int Pbkdf2Iterations { get; set; }

        [Required]
        public Guid SecurityStamp { get; set; }     //checked on authentication for any security related changes (like password change, role change, etc..)

        [Required]
        public bool EmailConfirmed { get; set; }
        #endregion



        //  Properties        
        public string FullName => FirstName + LastName;      
    }
}
