using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebTennisFieldReservation.Constants.Names;

namespace WebTennisFieldReservation.AuthorizationPolicies.SameUser
{
    public class SameUserAuthZHandler : AuthorizationHandler<SameUserRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameUserRequirement requirement, Guid resource)
        {            
            string idAsString = resource.ToString();

            if(context.User.HasClaim(ClaimsNames.Id, idAsString))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
