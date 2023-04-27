using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;

namespace WebTennisFieldReservation.Controllers
{
    [Route("/administration/users")]
    [Authorize(Policy = AuthorizationPoliciesNames.IsAdmin)]
    [AutoValidateAntiforgeryToken]
    public class Administration_UsersController : Controller
    {
        private readonly ICourtComplexRepository _repo;

        public Administration_UsersController(ICourtComplexRepository repo)
        {
            _repo = repo;
        }

        
    }
}
