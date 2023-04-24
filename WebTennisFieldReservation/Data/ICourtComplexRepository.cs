using WebTennisFieldReservation.Entities;

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
    }
}
