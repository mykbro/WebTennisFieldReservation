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

        // necessary for scoped injection
        public void Dispose()
        {
            _context.Dispose();
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

        public Task<int> ConfirmUserEmailAsync(Guid id, Guid securityStamp)
        {
            // we set EmailConfirmed = true for the user with Id = id if secStamp coincides and email is still unconfirmed
            // we return the nr of rows updated
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == securityStamp && user.EmailConfirmed == false)
                        .ExecuteUpdateAsync(user => user.SetProperty(user => user.EmailConfirmed, true));
        }

        public Task<User?> GetDataForTokenAsync(string email)
        {
            //we check if a user exists with Email == email otherwise we return default
            return _context.Users.Where(user => user.Email.Equals(email)).Select(user => new User() { Id = user.Id, SecurityStamp = user.SecurityStamp }).SingleOrDefaultAsync();
        }

        public Task<int> ResetUserPasswordAsync(User userData, Guid newSecurityStamp)
        {
            //we should probaly check if email is confirmed
            return _context.Users.Where(user => user.Id == userData.Id && user.SecurityStamp == userData.SecurityStamp)
                .ExecuteUpdateAsync(user =>                
                    user.SetProperty(user => user.PwdHash, userData.PwdHash)
                        .SetProperty(user => user.PwdSalt, userData.PwdSalt)
                        .SetProperty(user => user.Pbkdf2Iterations, userData.Pbkdf2Iterations)
                        .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                );
        }

        public Task<User?> GetDataForLoginCheckAsync(string email)
        {
            //we check email confirmed
            return _context.Users.Where(user => user.Email == email && user.EmailConfirmed == true)
                .Select(user => new User() 
                { 
                    Id = user.Id, 
                    SecurityStamp = user.SecurityStamp, 
                    PwdHash = user.PwdHash, 
                    PwdSalt = user.PwdSalt, 
                    Pbkdf2Iterations = user.Pbkdf2Iterations
                }) 
                .SingleOrDefaultAsync();
        }

        public async Task<bool> IsAdminAsync(Guid id)
        {
            AdminUser? admin = await _context.AdminUsers.FindAsync(id);
            return admin != null;
        }

        public Task<User?> GetAuthenticatedUserDataAsync(Guid id, Guid securityStamp)
        {
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == securityStamp)
                .Select(user => new User() { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email })
                .SingleOrDefaultAsync();
        }

        public Task<List<User>> GetAllUsersDataAsync()
        {
            return _context.Users.Select(user => new User() { 
                Id = user.Id,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }).ToListAsync();
        }

        public Task<User?> GetUserDataByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id).Select(user => 
                new User(){ 
                    Address = user.Address,
                    BirthDate = user.BirthDate,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName            
                }).SingleOrDefaultAsync();
        }

        public async Task<int> UpdateUserDataAsync(User userData)
        {
            try
            {
                int updatedUsers = await _context.Users.Where(user => user.Id == userData.Id).ExecuteUpdateAsync( user => 
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

        public Task<User?> GetPasswordDataByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id)
                .Select(user => new User() { PwdHash = user.PwdHash, PwdSalt = user.PwdSalt, Pbkdf2Iterations = user.Pbkdf2Iterations })
                .SingleOrDefaultAsync();
        }

        public Task<int> UpdatePasswordDataAsync(User userData, Guid newSecurityStamp)
        {
            return _context.Users.Where(user => user.Id == userData.Id).ExecuteUpdateAsync(user =>
                    user.SetProperty(user => user.PwdHash, userData.PwdHash)
                        .SetProperty(user => user.PwdSalt, userData.PwdSalt)
                        .SetProperty(user => user.Pbkdf2Iterations, userData.Pbkdf2Iterations)
                        .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                    );                        
        }

        public Task<int> DeleteUserByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id).ExecuteDeleteAsync();
        }

        public async Task<bool> AddTemplateAsync(Template templateToAdd)
        {           

            //we add the template (with its entries) to the dbContext
            await _context.AddAsync(templateToAdd);
            
            //we need to check for template Name uniqueness
            try
            {
                await _context.SaveChangesAsync();  
                return true;
            }
            catch(DbUpdateException ex)
            {
                if(ex.InnerException is SqlException sqlEx)
                {
                    //we should also check for error 2601 (duplicate row)
                    return false;
                }
                else
                {
                    //if we're here it's not due to uniqueness constraint
                    throw;
                }
                
            }            
        }

        public Task<List<Template>> GetAllTemplatesAsync()
        {
            return _context.Templates.Select(t => new Template() {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description
            }).ToListAsync();
        }

        public Task<int> DeleteTemplateByIdAsync(int id)
        {
            return _context.Templates.Where(t => t.Id == id).ExecuteDeleteAsync();
        }

        public Task<Template?> GetTemplateDataByIdAsync(int id)
        {
            //we must include the TemplateEntries
            return _context.Templates
                .Where(t => t.Id == id)
                .Include(t => t.TemplateEntries)
                .SingleOrDefaultAsync();
            
        }

        public async Task<int> UpdateTemplateAsync(Template templateData)
        {
            /* we cannot use ExecuteUpdate due to linked navigation properties */
            /* also we do not care about any possible update between the read step and the update step... last update win */

            //we retrieve the template we have in the db linked to the id
            Template? template = await _context.Templates
                .Where(t => t.Id == templateData.Id)
                .Include(t => t.TemplateEntries)
                .SingleOrDefaultAsync();                      

            if (template == null)
            {
                return 0;
            }
            else
            {
                //we update it
                template.Name = templateData.Name;
                template.Description = templateData.Description;
                template.TemplateEntries = templateData.TemplateEntries;

                //and we try to save the changes
                try
                {
                    int updatedRows = await _context.SaveChangesAsync();
                    //we return 1 anyway... updatedRows will take into account the nr of updated TemplateEntries
                    return 1;   
                }
                catch(DbUpdateException ex)
                {
                    //duplicate name
                    return -1;
                }
                catch (InvalidOperationException ex)
                {
                    //duplicate weekslots (exception thrown by EF and not the db)
                    return -1;
                }
            }
        }

        public async Task<bool> AddCourtAsync(CourtModel courtData)
        {
            _context.Courts.Add(new Court() { Name = courtData.Name });

            //we need to check for court Name uniqueness
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    //we should also check for error 2601 (duplicate row)
                    return false;
                }
                else
                {
                    //if we're here it's not due to uniqueness constraint
                    throw;
                }

            }
        }

        public Task<List<CourtRowModel>> GetAllCourtsAsync()
        {
            return _context.Courts.Select(c => new CourtRowModel() { Id = c.Id, Name = c.Name }).ToListAsync();
        }

        public Task<int> DeleteCourtByIdAsync(int id)
        {
            return _context.Courts.Where(c => c.Id == id).ExecuteDeleteAsync();
        }

        public Task<CourtModel?> GetCourtDataByIdAsync(int id)
        {
            return _context.Courts.Where(c => c.Id == id).Select(c => new CourtModel() { Name = c.Name}).SingleOrDefaultAsync();
        }

        public async Task<int> UpdateCourtByIdAsync(int id, CourtModel courtData)
        {
            try
            {
                int updatedCourts = await _context.Courts.Where(c => c.Id == id).ExecuteUpdateAsync(c =>
                            c.SetProperty(c => c.Name, courtData.Name)
                        );

                return updatedCourts;
            }
            catch(SqlException ex)
            {
                return -1;
            }
        }
    }
}
