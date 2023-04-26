using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Models.Administration;

namespace WebTennisFieldReservation.Controllers
{
    [Route("/administration")]
    [Authorize(Policy = AuthorizationPoliciesNames.IsAdmin)]
    [AutoValidateAntiforgeryToken]
    public class AdministrationController : Controller
    {
        private readonly ICourtComplexRepository _repo;

        public AdministrationController(ICourtComplexRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users()
        {        
            List<UserPartialModel> users = await _repo.GetAllUsersData();
            return View(users);
        }

        [HttpGet("courts")]
        public IActionResult Courts()
        {
            return View();
        }

        [HttpGet("templates")]
        public IActionResult Templates()
        {
            return View();
        }
    }
}
