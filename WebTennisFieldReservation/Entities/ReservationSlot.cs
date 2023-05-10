using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTennisFieldReservation.Entities
{
	[Index(nameof(CourtId), nameof(Date), nameof(DaySlot), IsUnique = true)]
	public class ReservationSlot
	{
		[Key]
		public int Id { get; set; }
		[ForeignKey(nameof(Court))]		
		public int CourtId { get; set; }
		[Required]
		public DateTime Date { get; set; }
		[Required]
		public int DaySlot { get; set; }
		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }
		[Required]
		public bool IsAvailable { get; set; }

		//navigation
		public Court Court { get; set; } = null!;
	}
}
