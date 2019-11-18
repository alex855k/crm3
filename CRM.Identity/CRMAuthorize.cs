using CRM.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace CRM.Identity
{
    public class CRMAuthorize : AuthorizeAttribute
    {
        private bool UnAuthorizedFeature { get; set; }
        private bool UnAuthorizedUser { get; set; }
        private bool IsPartialPage { get; set; }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string redirectUrl = "";
            if (this.AuthorizeCore(filterContext.HttpContext))
            {

                base.OnAuthorization(filterContext);
            }
            else
            {
                if (UnAuthorizedFeature && IsPartialPage)
                    redirectUrl = "~/Views/Shared/UnauthorizedFeaturePartial.cshtml";
                else if (UnAuthorizedUser && IsPartialPage)
                    redirectUrl = "~/Views/Shared/UnauthorizedUserPartial.cshtml";
                else if (UnAuthorizedFeature)
                    redirectUrl = "~/Views/Shared/UnauthorizedFeature.cshtml";
                else if (UnAuthorizedUser)
                    redirectUrl = "~/Views/Shared/UnauthorizedUser.cshtml";

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = redirectUrl
                    };
                }
                else
                {
                   
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.Result = new HttpStatusCodeResult(403);
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(
                                 new RouteValueDictionary
                                 {
                                       { "action", "Unauthorized" },
                                       { "controller", "Account" }
                                 });
                    }
                        
                }
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool isAuthorized = false;

            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var routeData = httpContext.Request.RequestContext.RouteData;
            string currentControllerName = routeData.GetRequiredString("controller");
            string currentActionName = routeData.GetRequiredString("action");

            if (string.IsNullOrEmpty(currentControllerName))
                return false;

            UnitofWork _uow = new UnitofWork();

            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
            var currentUser = httpContext.User.Identity.Name;
            if (currentControllerName == "Home" && currentActionName == "Index")
                isAuthorized = true;
            else
            {
                if (string.IsNullOrEmpty(currentControllerName))
                    isAuthorized = false;

                else if (IsSystemFeatureAvailable(currentControllerName.ToUpper(), currentActionName.ToUpper(), _uow) && IsUserAuthorized(currentControllerName.ToUpper(), currentActionName.ToUpper(), _uow, userManager, currentUser))
                    isAuthorized = true;
            }
            return isAuthorized;
        }

        private bool IsSystemFeatureAvailable(string controller, string action, UnitofWork _uow)
        {
            var currentControllerAction = _uow.ApplicationControllersRepo
                .Search(x => x.ControllerName.ToUpper() == controller
                    && (x.ActionName.ToUpper() == action //ActionName matches the requested action
                    || x.ActionName == ""))              //or empty, meaning the currentUser has full access to the controller
                .AsEnumerable();

            if (currentControllerAction != null || currentControllerAction.Any())
                return true;

            return false;
        }
        private bool IsUserAuthorized(string controller, string action, UnitofWork _uow, UserManager userManager, string currentUser)
        {
            //Checks if user has role which is authorized for the action.

            bool isUserAuthorized = false;
            var user = userManager.FindByName(currentUser);

            if (user == null)
                return false;

            var userControllerRoles = _uow.ControllerRolesRepo.GetControllerRolesByRoles(user.Roles.Select(x => x.RoleId).ToList());
            if (userControllerRoles == null || !userControllerRoles.Any())
                return false;

            var currentControllerAction = _uow.ApplicationControllersRepo
                .Search(x => x.ControllerName.ToUpper() == controller 
                    && (x.ActionName.ToUpper() == action //ActionName matches the requested action
                    || x.ActionName == ""))              //or empty, meaning the currentUser has full access to the controller
                .ToList();

            if (currentControllerAction == null)
            {
                return false;
            }

            bool isRoleExist = userControllerRoles.Any(role => role.ControllerId == currentControllerAction.First().Id);

            if (isRoleExist)
                isUserAuthorized = true;


            UnAuthorizedUser = !isUserAuthorized;
            IsPartialPage = currentControllerAction.First().IsPartialPage;
            return isUserAuthorized;
        }
    }
}
