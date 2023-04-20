using WebTennisFieldReservation.Entities;

namespace WebTennisFieldReservation.Data
{
    public interface ICourtComplexRepository
    {
        public Task<bool> AddUserAsync(User u);
        public Task<IEnumerable<User>> GetUsersHavingMail(string email);

        public Task<int> UpdateUserEmailConfirmationByIdAndSecurityStampAsync(Guid id, Guid securityStamp);
    }
}
