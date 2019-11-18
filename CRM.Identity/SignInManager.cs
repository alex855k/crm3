using CRM.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CRM.Identity
{

    public class SignInManager : SignInManager<User, string>
    {
        public SignInManager(UserManager userManager, IAuthenticationManager authenticationManager)
        : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((UserManager)UserManager);
        }

        public static SignInManager Create(IdentityFactoryOptions<SignInManager> options, IOwinContext context)
        {
            return new SignInManager(context.GetUserManager<UserManager>(), context.Authentication);
        }

        public override Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool rememberMe, bool shouldLockout)
        {
            var user = UserManager.FindByNameAsync(userName).Result;
            if (user != null && user.IsEnabled.HasValue && !user.IsEnabled.Value)
            {
                return Task.FromResult<SignInStatus>(SignInStatus.LockedOut);
            }
            return base.PasswordSignInAsync(userName, password, rememberMe, shouldLockout);
        }
    }
}
