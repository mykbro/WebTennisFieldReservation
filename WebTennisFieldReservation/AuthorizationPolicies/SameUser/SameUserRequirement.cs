using Microsoft.AspNetCore.Authorization;

namespace WebTennisFieldReservation.AuthorizationPolicies.SameUser
{
    public class SameUserRequirement : IAuthorizationRequirement
    {
    }
}
