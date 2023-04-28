using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.Users;

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

        public Task<int> ConfirmUserEmailAsync(Guid id, Guid securityStamp)
        {
            // we set EmailConfirmed = true for the user with Id = id if secStamp coincides and email is still unconfirmed
            // we return the nr of rows updated
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == securityStamp && user.EmailConfirmed == false)
                        .ExecuteUpdateAsync(user => user.SetProperty(user => user.EmailConfirmed, true));
        }

        public Task<(Guid Id, Guid SecurityStamp)> GetDataForTokenAsync(string email)
        {
            //we check if a user exists with Email == email otherwise we return default
            return _context.Users.Where(user => user.Email.Equals(email)).Select(user => new ValueTuple<Guid, Guid>(user.Id, user.SecurityStamp)).SingleOrDefaultAsync();
        }

        public Task<int> ResetUserPasswordAsync(Guid id, Guid oldSecurityStamp, byte[] pwdHash, byte[] salt, int iters, Guid newSecurityStamp)
        {
            //we should probaly check if email is confirmed
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == oldSecurityStamp)
                .ExecuteUpdateAsync(user =>                
                    user.SetProperty(user => user.PwdHash, pwdHash)
                        .SetProperty(user => user.PwdSalt, salt)
                        .SetProperty(user => user.Pbkdf2Iterations, iters)
                        .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                );
        }

        public Task<(Guid Id, Guid SecurityStamp, byte[] pwdHash, byte[] salt, int iters)> GetDataForLoginCheckAsync(string email)
        {
            //we check email confirmed
            return _context.Users.Where(user => user.Email == email && user.EmailConfirmed == true)
                .Select(user => new ValueTuple<Guid, Guid, byte[], byte[], int>(user.Id, user.SecurityStamp, user.PwdHash, user.PwdSalt, user.Pbkdf2Iterations))
                .SingleOrDefaultAsync();
        }

        public async Task<bool> IsAdminAsync(Guid id)
        {
            AdminUser? admin = await _context.AdminUsers.FindAsync(id);
            return admin != null;
        }

        public Task<(string Firstname, string Lastname, string Email)> GetAuthenticatedUserDataAsync(Guid id, Guid securityStamp)
        {
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == securityStamp)
                .Select(user => new ValueTuple<string, string, string>(user.FirstName, user.LastName, user.Email))
                .SingleOrDefaultAsync();
        }

        public Task<List<UserPartialModel>> GetAllUsersDataAsync()
        {
            return _context.Users.Select(user => new UserPartialModel() { 
                Id = user.Id,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }).ToListAsync();
        }

        public Task<EditUserDataModel?> GetUserDataByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id).Select(user => 
                new EditUserDataModel(){ 
                    Address = user.Address,
                    BirthDate = user.BirthDate,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName            
                }).SingleOrDefaultAsync();
        }

        public async Task<int> UpdateUserDataByIdAsync(Guid id, EditUserDataModel userData)
        {
            try
            {
                int updatedUsers = await _context.Users.Where(user => user.Id == id).ExecuteUpdateAsync( user => 
                    user.SetProperty(user => user.Address, userData.Address)
                        .SetProperty(user => user.FirstName, userData.FirstName)
                        .SetProperty(user => user.LastName, userData.LastName)
                        .SetProperty(user => user.BirthDate, userData.BirthDate)
                        .SetProperty(user => user.Email, userData.Email)
                );
                return updatedUsers;
            }
            catch(SqlException ex)
            {
                return 0;
            }
        }

        public Task<(byte[] pwdHash, byte[] salt, int iters)> GetPasswordDataByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id)
                .Select(user => new ValueTuple<byte[], byte[], int>(user.PwdHash, user.PwdSalt, user.Pbkdf2Iterations))
                .SingleOrDefaultAsync();
        }

        public Task<int> UpdatePasswordDataByIdAsync(Guid id, byte[] pwdHash, byte[] salt, int iters, Guid newSecurityStamp)
        {
            return _context.Users.Where(user => user.Id == id).ExecuteUpdateAsync(user =>
                    user.SetProperty(user => user.PwdHash, pwdHash)
                        .SetProperty(user => user.PwdSalt, salt)
                        .SetProperty(user => user.Pbkdf2Iterations, iters)
                        .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                    );                        
        }

        public Task<int> DeleteUserByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id).ExecuteDeleteAsync();
        }

        public async Task<bool> AddTemplateAsync(EditTemplateModel templateData)
        {
            templateData.CourtAvailabilityTemplateEntries
            
            _context.CourtsAvailabilityTemplates.Add(templateData);






            //we retrieve the highest id so far
            try
            {
                int maxId = await _context.CourtsAvailabilityTemplates.Select(temp => temp.Id).MaxAsync();
            }
            catch(InvalidOperationException ex)
            {
                
            }
            
            
            
            //we need to check for name uniqueness
            try
            {
                await _context.SaveChangesAsync();
                //we then need to check for the id
                int templateID = await _context.CourtsAvailabilityTemplates.Where(temp => temp.Name == templateData.Name).Select(temp => temp.Id).SingleOrDefaultAsync();

            }
            catch(DbUpdateException ex)
            {
                return false;
            }
        }
    }
}
