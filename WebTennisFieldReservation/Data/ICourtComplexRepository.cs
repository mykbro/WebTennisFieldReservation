using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.Users;

namespace WebTennisFieldReservation.Data
{
    public interface ICourtComplexRepository
    {
        public Task<bool> AddUserAsync(User u);
        public Task<IEnumerable<User>> GetUsersHavingMail(string email);

        public Task<int> ConfirmUserEmailAsync(Guid id, Guid securityStamp);
        public Task<int> ResetUserPasswordAsync(Guid id, Guid oldSecurityStamp, byte[] pwdHash, byte[] salt, int iters, Guid newSecurityStamp);
        public Task<(Guid Id, Guid SecurityStamp)> GetDataForTokenAsync(string email);
        public Task<(Guid Id, Guid SecurityStamp, byte[] pwdHash, byte[] salt, int iters)> GetDataForLoginCheckAsync(string email);
        public Task<bool> IsAdminAsync(Guid id);
        public Task<(string Firstname, string Lastname, string Email)> GetAuthenticatedUserDataAsync(Guid id, Guid securityStamp);

        public Task<List<UserPartialModel>> GetAllUsersData();
        public Task<EditUserDataModel?> GetUserDataById(Guid id);
        public Task<int> UpdateUserDataById(Guid id, EditUserDataModel userData);
    }
}
