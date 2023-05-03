using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.Users;

namespace WebTennisFieldReservation.Data
{
    public interface ICourtComplexRepository
    {
        public Task<bool> AddUserAsync(User u);  
        public Task<int> ConfirmUserEmailAsync(Guid id, Guid securityStamp);
        public Task<int> ResetUserPasswordAsync(User userData, Guid newSecurityStamp);
        public Task<User?> GetDataForTokenAsync(string email);
        public Task<User?> GetDataForLoginCheckAsync(string email);
        public Task<bool> IsAdminAsync(Guid id);
        public Task<User?> GetAuthenticatedUserDataAsync(Guid id, Guid securityStamp);
        public Task<List<User>> GetAllUsersDataAsync();
        public Task<User?> GetUserDataByIdAsync(Guid id);
        public Task<int> UpdateUserDataAsync(User userData);
        public Task<User?> GetPasswordDataByIdAsync(Guid id);
        public Task<int> UpdatePasswordDataAsync(User userData, Guid newSecurityStamp);
        public Task<int> DeleteUserByIdAsync(Guid id);

        public Task<bool> AddTemplateAsync(Template templateData);
        public Task<List<Template>> GetAllTemplatesAsync();
        public Task<int> DeleteTemplateByIdAsync(int id);
        public Task<Template?> GetTemplateDataByIdAsync(int id);
        public Task<int> UpdateTemplateAsync(Template templateData);

        public Task<bool> AddCourtAsync(CourtModel courtData);
        public Task<List<CourtRowModel>> GetAllCourtsAsync();
        public Task<int> DeleteCourtByIdAsync(int id);
        public Task<CourtModel?> GetCourtDataByIdAsync(int id);
        public Task<int> UpdateCourtByIdAsync(int id, CourtModel courtData);
    }
}
