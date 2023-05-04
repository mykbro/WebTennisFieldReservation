using Microsoft.AspNetCore.Authentication;

namespace WebTennisFieldReservation.Constants
{
    public static class AuthenticationPropertiesConsts
    {
        public static readonly AuthenticationProperties RememberMe = new AuthenticationProperties() { IsPersistent = true };
        public static readonly AuthenticationProperties DoNotRememberMe = new AuthenticationProperties() { IsPersistent = false };
    }
}
