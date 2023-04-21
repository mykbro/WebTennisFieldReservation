using Microsoft.EntityFrameworkCore;
using WebTennisFieldReservation.Entities;

namespace WebTennisFieldReservation.Data
{
    public class DbCourtComplexRepository : ICourtComplexRepository, IDisposable
    {
        private readonly CourtComplexDbContext _context;

        public DbCourtComplexRepository(string connString, bool log = false) 
        { 
            _context = CourtComplexDbContext.CreateDbContext(connString, log);
        }

        public async Task<bool> AddUserAsync(User u)
        {
            _context.Users.Add(u);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch(DbUpdateException ex) 
            {
                return false;
            }
        }

        // necessary for scoped injection
        public void Dispose()
        {
            _context.Dispose();
        }

        public Task<IEnumerable<User>> GetUsersHavingMail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<int> ConfirmUserEmail(Guid id, Guid securityStamp)
        {
            // we set EmailConfirmed = true for the user with Id = id if secStamp coincides and email is still unconfirmed
            // we return the nr of rows updated
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == securityStamp && user.EmailConfirmed == false)
                        .ExecuteUpdateAsync(user => user.SetProperty(user => user.EmailConfirmed, true));
        }

        public Task<(Guid Id, Guid SecurityStamp)> GetDataForConfirmationTokenAsync(string email)
        {
            //we check if a user exists with Email == email and EmailConfirmed == false, otherwise we return default
            return _context.Users.Where(user => user.Email.Equals(email) && user.EmailConfirmed == false).Select(user => new ValueTuple<Guid, Guid>(user.Id, user.SecurityStamp)).SingleOrDefaultAsync();
        }
    }
}
