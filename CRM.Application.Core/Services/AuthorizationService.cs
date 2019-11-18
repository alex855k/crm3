using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Web;

namespace CRM.Application.Core.Services
{
    public class AuthorizationService
    {
      
        public static bool AuthorizeRenderHTML(string currentActionName, string currentControllerName)
        {
            bool isAuthorized = false;
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return false;
            }
            UnitofWork _uow = new UnitofWork();
            if (currentControllerName == "Home" && currentActionName == "Index")
                isAuthorized = true;
            else
            {
                if (string.IsNullOrEmpty(currentControllerName))
                    isAuthorized = false;

                if (IsSystemFeatureAvailable(currentControllerName.ToUpper(), currentActionName.ToUpper()) && IsUserAuthorized(currentControllerName.ToUpper(), currentActionName.ToUpper(), _uow))
                    isAuthorized = true;
            }
            return isAuthorized;
        }
        public static bool IsSystemFeatureAvailable(string controller, string action)
        {
            UnitofWork _uow = new UnitofWork();
            bool isSystemFeatureAvailable = false;
            var result = _uow.ApplicationControllersRepo
                 .Search(x => x.ControllerName.ToUpper() == controller && x.ActionName.ToUpper() == action).SingleOrDefault();
            if (result == null || result.IsDisabled)
                isSystemFeatureAvailable = false;
            else
                isSystemFeatureAvailable = true;

            return isSystemFeatureAvailable;
        }
        private static bool IsUserAuthorized(string controller, string action, UnitofWork _uow)
        {
            bool isUserAuthorized = false;

            UserManager userManager = new UserManager(new UserStore<User>(new CRMContext()));

            var user = userManager.FindByNameAsync(HttpContext.Current.User.Identity.Name).Result;
            if (user == null)
                return false;
            var userControllerRoles = _uow.ControllerRolesRepo.GetControllerRolesByRoles(user.Roles.Select(x => x.RoleId).ToList());

            if (userControllerRoles == null || userControllerRoles.Count == 0)
                return false;

            var currentControllerAction = _uow.ApplicationControllersRepo
                .Search(x => x.ControllerName.ToUpper() == controller && x.ActionName.ToUpper() == action).SingleOrDefault();

            if (currentControllerAction == null)
            {
                return false;
            }

            bool isRoleExist = userControllerRoles.Any(role => role.ControllerId == currentControllerAction.Id);

            if (isRoleExist)
                isUserAuthorized = true;

            return isUserAuthorized;
        }
    }
}
