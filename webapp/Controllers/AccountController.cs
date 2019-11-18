using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CRM.Identity;
using CRM.Application.Core.ViewModels;
using CRM.Web.Extensions;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System;

namespace CRM.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private static string Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
        private UserManager _userManager;
        private RoleManager _roleManager;
        private SignInManager _signInManager;
        public AccountController()
        {
        }

        public AccountController(UserManager userManager, RoleManager roleManager, SignInManager signInManager)
        {
            UserManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public RoleManager RoleManager
        {
            get
            {
                return _roleManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<RoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        public SignInManager SignInManager
        {
            get
            {
                return _signInManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<SignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        // GET: /account/login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {

            // We do not want to use any existing identity information

            // Store the originating URL so we can attach it to a form field
            var loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                RememberMe = true,
            };
            return View(loginViewModel);
        }

        // POST: /account/login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [PreventDuplicateRequest]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {

            SignInStatus signInResult;
            var user = UserManager.Find(loginViewModel.UserName, loginViewModel.Password);
            if (user == null || !ModelState.IsValid)
            {
                ModelState.AddModelError("LoginErrors", CRM.Application.Core.Resources.Administration.Login.UserNamePasswordIncorrect);
                return View(loginViewModel);
            }

            //signInResult = await SignInManager.PasswordSignInAsync(
            //    loginViewModel.UserName,
            //    loginViewModel.Password,
            //    loginViewModel.RememberMe,
            //    shouldLockout: false);

            AuthenticationManager.SignOut(new[] { DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.ExternalBearer });

           var claimsIdentity = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

            if (claimsIdentity == null)
            {
                ModelState.AddModelError("LoginErrors", CRM.Application.Core.Resources.Administration.Login.UserNamePasswordIncorrect.ToString());
                signInResult = SignInStatus.Failure;
            }
            
            else
            {
                AuthenticationProperties authenticationProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(90)
                };

                switch (Request.UserHostAddress)
                {
                    case "185.210.176.8":
                    case "::1":
                    case "127.0.0.1":
                    case "localhost":

                        authenticationProperties.AllowRefresh = true;
                         AuthenticationManager.SignIn(authenticationProperties, claimsIdentity);
                        break;
                    default:

                        AuthenticationManager.SignIn(authenticationProperties, claimsIdentity);
                        break;
                }

            }

            if (!claimsIdentity.IsAuthenticated)
                signInResult = SignInStatus.Failure;
            else
                signInResult = SignInStatus.Success;

            

            //if (loginViewModel.RememberMe)
            //{
            //    HttpCookie cookie = FormsAuthentication.GetAuthCookie(loginViewModel.UserName, loginViewModel.RememberMe);
            //    cookie.Expires = DateTime.Now.AddDays(30);
            //    //var authCookieData = Hash(loginViewModel.UserName + HttpContext.Session.SessionID);
            //    Response.Cookies.Add(cookie);
            //}
            //else
            //{
            //    FormsAuthentication.SetAuthCookie(loginViewModel.UserName, loginViewModel.RememberMe);
            //}

            switch (signInResult)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(loginViewModel.ReturnUrl);
                case SignInStatus.LockedOut:
                    ModelState.AddModelError("LoginErrors", CRM.Application.Core.Resources.Administration.Login.UserLockedOut.ToString());
                    return View(loginViewModel);
                case SignInStatus.Failure:
                    ModelState.AddModelError("LoginErrors", CRM.Application.Core.Resources.Administration.Login.UserNamePasswordIncorrect.ToString());
                    return View(loginViewModel);
                default:
                    return View(loginViewModel);
            }
        }

        private void EnsureLoggedOut()
        {
            // If the request is (still) marked as authenticated we send the user to the logout action
            if (Request.IsAuthenticated)
                Logout();
        }
        // POST: /account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }
        private ActionResult RedirectToLocal(string returnUrl = "")
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Unauthorized()
        {
            return View("~/Views/Shared/UnauthorizedFeature.cshtml");
        }
    }
}