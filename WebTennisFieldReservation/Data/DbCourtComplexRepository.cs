﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.Api;
using WebTennisFieldReservation.Models.CourtAvailability;
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

        public Task<DataForTokenModel?> GetDataForTokenAsync(string email)
        {
            //we check if a user exists with Email == email otherwise we return default
            return _context.Users.Where(user => user.Email.Equals(email)).Select(user => new DataForTokenModel() { Id = user.Id, SecurityStamp = user.SecurityStamp }).SingleOrDefaultAsync();
        }

        public Task<int> ResetUserPasswordAsync(Guid id, PasswordResetModel pwdResetData)
        {
            //we should probaly check if email is confirmed
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == pwdResetData.OldSecurityStamp)
                .ExecuteUpdateAsync(user =>                
                    user.SetProperty(user => user.PwdHash, pwdResetData.PasswordHash)
                        .SetProperty(user => user.PwdSalt, pwdResetData.Salt)
                        .SetProperty(user => user.Pbkdf2Iterations, pwdResetData.Iters)
                        .SetProperty(user => user.SecurityStamp, pwdResetData.NewSecurityStamp)
                );
        }

        public Task<DataForLoginCheckModel?> GetDataForLoginCheckAsync(string email)
        {
            //we check email confirmed
            return _context.Users.Where(user => user.Email == email && user.EmailConfirmed == true)
                .Select(user => new DataForLoginCheckModel()
                {
                    Id = user.Id, 
                    SecuritStamp = user.SecurityStamp, 
                    PwdHash = user.PwdHash, 
                    Salt = user.PwdSalt, 
                    Iters = user.Pbkdf2Iterations 
                })
                .SingleOrDefaultAsync();
        }

        public async Task<bool> IsAdminAsync(Guid id)
        {
            AdminUser? admin = await _context.AdminUsers.FindAsync(id);
            return admin != null;
        }

        public Task<AuthenticatedUserDataModel?> GetAuthenticatedUserDataAsync(Guid id, Guid securityStamp)
        {
            return _context.Users.Where(user => user.Id == id && user.SecurityStamp == securityStamp)
                .Select(user => new AuthenticatedUserDataModel() { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email })
                .SingleOrDefaultAsync();
        }

        public Task<List<UserRowModel>> GetAllUsersDataAsync()
        {
            return _context.Users.Select(user => new UserRowModel() { 
                Id = user.Id,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }).ToListAsync();
        }

        public Task<UserModel?> GetUserDataByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id).Select(user => 
                new UserModel(){ 
                    Address = user.Address,
                    BirthDate = user.BirthDate,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName            
                }).SingleOrDefaultAsync();
        }

        public async Task<int> UpdateUserDataByIdAsync(Guid id, UserModel userData)
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

        public Task<PasswordDataModel?> GetPasswordDataByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id)
                .Select(user => new PasswordDataModel() { PasswordHash = user.PwdHash, Salt = user.PwdSalt, Iters = user.Pbkdf2Iterations })
                .SingleOrDefaultAsync();
        }

        public Task<int> UpdatePasswordDataByIdAsync(Guid id, PasswordUpdateModel pwdUpdateData)
        {
            return _context.Users.Where(user => user.Id == id).ExecuteUpdateAsync(user =>
                    user.SetProperty(user => user.PwdHash, pwdUpdateData.PasswordHash)
                        .SetProperty(user => user.PwdSalt, pwdUpdateData.Salt)
                        .SetProperty(user => user.Pbkdf2Iterations, pwdUpdateData.Iters)
                        .SetProperty(user => user.SecurityStamp, pwdUpdateData.NewSecurityStamp)
                    );                        
        }

        public Task<int> DeleteUserByIdAsync(Guid id)
        {
            return _context.Users.Where(user => user.Id == id).ExecuteDeleteAsync();
        }

        public async Task<bool> AddTemplateAsync(TemplateModel templateData)
        {
            Template templateToAdd = new Template()
            {
                Name = templateData.Name,
                Description = templateData.Description
            };

            //we need to populate the navigation property TemplateEntries in order to autopopulate the entries with the correct auto-generated TemplateId
            for(int i=0; i < templateData.TemplateEntryModels.Count; i++)
            {
                if (templateData.TemplateEntryModels[i].IsSelected)
                {
                    //here Price is not null but we cannot use ! (dunno why) so we use ?? "0m"
                    templateToAdd.TemplateEntries.Add(new TemplateEntry() { WeekSlot = i, Price = templateData.TemplateEntryModels[i].Price ?? 0m });
                }
            }   

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

        public Task<List<TemplateRowModel>> GetAllTemplatesAsync()
        {
            return _context.Templates.Select(t => new TemplateRowModel() {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description
            }).ToListAsync();
        }

        public Task<List<TemplateSelectionModel>> GetAllTemplatesForDropdownAsync()
        {
            return _context.Templates.Select(t => new TemplateSelectionModel()
            {
                Id = t.Id,
                Name = t.Name,               
            }).ToListAsync();
        }

        public Task<int> DeleteTemplateByIdAsync(int id)
        {
            return _context.Templates.Where(t => t.Id == id).ExecuteDeleteAsync();
        }

        public async Task<TemplateModel?> GetTemplateDataByIdAsync(int id)
        {
            //we must include the TemplateEntries
            Template? t = await _context.Templates
                .Where(t => t.Id == id)
                .Include(t => t.TemplateEntries)
                .SingleOrDefaultAsync();

            if (t != null)
            {
                TemplateModel toReturn = new TemplateModel()
                {
                    Name = t.Name,
                    Description = t.Description,
                    TemplateEntryModels = new List<TemplateEntryModel>(168)       
                };

                //we need to initialize all the entries in the array, we can do it with a singleton to spare a lot of instantiations
                TemplateEntryModel singleton = new TemplateEntryModel();
                for(int i=0; i < 168; i++)
                {
                    toReturn.TemplateEntryModels.Add(singleton);
                }

                //and we update only the entries that we have in the database
                foreach(TemplateEntry entry in t.TemplateEntries)
                {
                    toReturn.TemplateEntryModels[entry.WeekSlot] = new TemplateEntryModel() { IsSelected = true, Price = entry.Price};
                }

                return toReturn;
            }
            else
            {
                return null;
            }          
        }

        public async Task<int> UpdateTemplateByIdAsync(int id, TemplateModel templateData)
        {
            //we add a template to tracked entities
            Template template = new Template() { Id = id };
            _context.Templates.Attach(template);
           
            //we update its properties (after attachment)
            template.Name = templateData.Name;
            template.Description = templateData.Description;

            //we init the entries to an empty list
            template.TemplateEntries = new List<TemplateEntry>();

			//...and we need to insert the new entries (which can be the same as the old one :D)
			for (int i = 0; i < templateData.TemplateEntryModels.Count; i++)
            {
                if (templateData.TemplateEntryModels[i].IsSelected)
                {
                    //here Price is not null but we cannot use ! (dunno why) so we use ?? "0m"
                    template.TemplateEntries.Add(new TemplateEntry() { WeekSlot = i, Price = templateData.TemplateEntryModels[i].Price ?? 0m });
                }
            }

            using(var trans = await _context.Database.BeginTransactionAsync()) 
            {
                //we delete all the entries
                await _context.TemplateEntries.Where(entry => entry.TemplateId == id).ExecuteDeleteAsync();

				//we try to update
				try
				{
					int updatedRows = await _context.SaveChangesAsync();
                    await trans.CommitAsync();
					//we return 1 anyway... updatedRows will take into account the nr of updated TemplateEntries
					return 1;
				}
				catch (DbUpdateException ex)
				{
					//duplicate name
                    await trans.RollbackAsync();
					return -1;
				}
				catch (InvalidOperationException ex)
				{
					//duplicate weekslots (exception thrown by EF and not the db)
					await trans.RollbackAsync();
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

        public Task<List<CourtSelectionModel>> GetAllCourtsForDropdownAsync()
        {
            return _context.Courts.Select(c => new CourtSelectionModel() { Id = c.Id, Name = c.Name }).ToListAsync();
        }

		public async Task<bool> AddReservationSlotsAsync(PostedReservationSlotsModel reservationSlotsData)
		{
            List<ReservationSlot> slotEntities = new List<ReservationSlot>(reservationSlotsData.SlotEntries.Count);     //we init capacity
            DateTime mondayAsLocal = reservationSlotsData.MondayDateUtc.ToLocalTime();

			foreach (ReservationSlotModel entry in reservationSlotsData.SlotEntries)
            {
                //we map from weekSlot to (weekDat + daySlot)
                int weekDayNr = entry.Slot / 24;
                int daySlot = entry.Slot % 24;

                //we calculate the date taking into account that the passed MondayDate is UTC
                DateTime day = mondayAsLocal.AddDays(weekDayNr);

                //we create the entry and add it to the list
                slotEntities.Add(new ReservationSlot()
                {
                    CourtId = reservationSlotsData.CourtId,
                    Date = day,
                    DaySlot = daySlot,
                    Price = entry.Price,
                    IsAvailable = true
                });
            }

            //we add all the entries to the context
            _context.ReservationsSlots.AddRange(slotEntities);

            //we atomically delete and update the slots in the date range (deafult isolation lvl, no reads)
			using (var trans = await _context.Database.BeginTransactionAsync())
            {  
                
                await _context.ReservationsSlots
                    .Where(slot => slot.CourtId == reservationSlotsData.CourtId && mondayAsLocal <= slot.Date && slot.Date <= mondayAsLocal.AddDays(6))
                    .ExecuteDeleteAsync();                 

				try
				{
					await _context.SaveChangesAsync();
					await trans.CommitAsync();
                    return true;
				}
				catch
				{
                    await trans.RollbackAsync();
					return false;
				}
			}
		}

		public async Task<List<ReservationSlotModel>> GetReservationSlotsForCourtBetweenDatesAsync(int courtId, DateTime from, DateTime to)
		{
            List<ReservationSlot> slots = await _context.ReservationsSlots
               .Where(slot => slot.CourtId == courtId && from <= slot.Date && slot.Date <= to).ToListAsync();

            List<ReservationSlotModel> slotModels = new List<ReservationSlotModel>(slots.Count);

            //we need to map from daySlot to weekSlot
            foreach(ReservationSlot slot in slots)
            {
                int daysFromMonday = (slot.Date - from).Days;

                var slotModel = new ReservationSlotModel()
                {
                    Price = slot.Price,
                    Slot = daysFromMonday * 24 + slot.DaySlot
				};

                slotModels.Add(slotModel);
            }

            return slotModels;                
		}

		public Task<List<ReservationSlotModel>> GetReservatonSlotsFromTemplateAsync(int templateId)
		{
            //we just map TemplateEntries to ReservationSlotModels
            return _context.TemplateEntries
                .Where(entry => entry.TemplateId == templateId)
                .Select(entry => new ReservationSlotModel()
                {
                    Slot = entry.WeekSlot,
                    Price = entry.Price
                })
                .ToListAsync();
		}

		public Task<List<SlotAvailabilityForDateModel>> GetSlotAvailabilityForDateForAllCourtsAsync(DateTime date)
		{
			return _context.ReservationsSlots
                .Where(slot => slot.Date == date)
                .Select(slot => new SlotAvailabilityForDateModel()
                {
                    Id = slot.Id,
                    CourtId = slot.CourtId,
                    DaySlot = slot.DaySlot,
                    IsAvailable = slot.IsAvailable,
                    Price = slot.Price
                })
                .ToListAsync();
		}

		public Task<List<SlotModel>> GetSlotDataByIdListAsync(List<int> ids)
		{
			return _context.ReservationsSlots
                .Include(slot => slot.Court)
                .Where(slot =>  ids.Contains(slot.Id) && slot.IsAvailable == true)      //preliminary check for availability (also for forged data)
                .Select(slot => new SlotModel() 
                { 
                    Date = slot.Date,
                    CourtName = slot.Court.Name,
                    DaySlot = slot.DaySlot,
                    Price = slot.Price
                })
                .ToListAsync();
		}
	}
}
