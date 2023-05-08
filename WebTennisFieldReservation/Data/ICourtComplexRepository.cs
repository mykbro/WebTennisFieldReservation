using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Models.Administration;
using WebTennisFieldReservation.Models.Users;
using WebTennisFieldReservation.Models.Api;

namespace WebTennisFieldReservation.Data
{
    public interface ICourtComplexRepository
    {
        public Task<bool> AddUserAsync(User u); 
        public Task<int> ConfirmUserEmailAsync(Guid id, Guid securityStamp);
        public Task<int> ResetUserPasswordAsync(Guid id, PasswordResetModel pwdResetData);
        public Task<DataForTokenModel?> GetDataForTokenAsync(string email);
        public Task<DataForLoginCheckModel?> GetDataForLoginCheckAsync(string email);
        public Task<bool> IsAdminAsync(Guid id);
        public Task<AuthenticatedUserDataModel?> GetAuthenticatedUserDataAsync(Guid id, Guid securityStamp);
        public Task<List<UserRowModel>> GetAllUsersDataAsync();
        public Task<UserModel?> GetUserDataByIdAsync(Guid id);
        public Task<int> UpdateUserDataByIdAsync(Guid id, UserModel userData);
        public Task<PasswordDataModel?> GetPasswordDataByIdAsync(Guid id);
        public Task<int> UpdatePasswordDataByIdAsync(Guid id, PasswordUpdateModel pwdUpdateData);
        public Task<int> DeleteUserByIdAsync(Guid id);

        public Task<bool> AddTemplateAsync(TemplateModel templateData);
        public Task<List<TemplateRowModel>> GetAllTemplatesAsync();
        public Task<List<TemplateSelectionModel>> GetAllTemplatesForDropdownAsync();
        public Task<int> DeleteTemplateByIdAsync(int id);
        public Task<TemplateModel?> GetTemplateDataByIdAsync(int id);
        public Task<int> UpdateTemplateByIdAsync(int id, TemplateModel templateData);

        public Task<bool> AddCourtAsync(CourtModel courtData);
        public Task<List<CourtRowModel>> GetAllCourtsAsync();
        public Task<List<CourtSelectionModel>> GetAllCourtsForDropdownAsync();
        public Task<int> DeleteCourtByIdAsync(int id);
        public Task<CourtModel?> GetCourtDataByIdAsync(int id);
        public Task<int> UpdateCourtByIdAsync(int id, CourtModel courtData);

        public Task<bool> AddReservationSlotsAsync(PostedReservationSlotsModel reservationSlotsData);
		public Task<List<ReservationSlotModel>> GetReservationSlotsForCourtBetweenDatesAsync(int courtId, DateTime from, DateTime to);

		public Task<List<ReservationSlotModel>> GetReservatonSlotsFromTemplateAsync(int templateId);
	}
}
