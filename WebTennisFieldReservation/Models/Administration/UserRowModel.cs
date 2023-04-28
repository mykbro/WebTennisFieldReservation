
namespace WebTennisFieldReservation.Models.Administration
{
    public class UserRowModel
    {
        
        public Guid Id { get; set; }
      
        public string FirstName { get; set; } = null!;
       
        public string LastName { get; set; } = null!;
       
        public string? Address { get; set; }      
       
        public string Email { get; set; } = null!;
       
        public DateTime BirthDate { get; set; }
    }
}
