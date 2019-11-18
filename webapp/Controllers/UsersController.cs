using CRM.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using CRM.Identity;
using CRM.Application.Core.ViewModels;
using CRM.Application.Core.Enums;

namespace CRM.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {

        private UserManager _userManager;
        private RoleManager _roleManager;

        public UsersController()
        {
        }

        public UsersController(UserManager userManager, RoleManager roleManager)
        {
            UserManager = userManager;
            _roleManager = roleManager;
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
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
        // GET: Users
        public ActionResult Index()
        {
            UserViewModel userViewModel = new UserViewModel();
            var users = UserManager.Users.ToList();
            foreach (var user in users)
            {
                userViewModel.UsersList.Add(user as User);
            }

            return View(userViewModel);
        }
        public ActionResult Create()
        {
            UserViewModel userViewModel = new UserViewModel();
            userViewModel.IsEnabled = true;
            return View(userViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> Create(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityResult createdUser = null;
                PasswordHasher passwordHasher = new PasswordHasher();
                string password = null;
                if (userViewModel.Password != null)
                {
                    password = passwordHasher.HashPassword(userViewModel.Password);
                }
                if (userViewModel.Id == Guid.Empty)
                {
                    createdUser = await UserManager.CreateAsync(new CRM.Models.User
                    {
                        UserName = userViewModel.UserName,
                        PasswordHash = password,
                        Email = userViewModel.Email,
                        PhoneNumber = userViewModel.Phone,
                        FirstName = userViewModel.FirstName,
                        LastName = userViewModel.LastName,
                        IsSuperAdmin = false,
                        IsEnabled = userViewModel.IsEnabled,
                        MonHours = userViewModel.MonHours,
                        TueHours = userViewModel.TueHours,
                        WedHours = userViewModel.WedHours,
                        ThursHours = userViewModel.ThursHours,
                        FriHours = userViewModel.FriHours,
                        SatHours = userViewModel.SatHours,
                        SunHours = userViewModel.SunHours
                    });
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.User.UserSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                }
                else
                {
                    User user = await UserManager.FindByIdAsync(userViewModel.Id.ToString());
                    user.UserName = userViewModel.UserName;
                    if (userViewModel.Password != null) { 
                        user.PasswordHash = password;
                    }
                    user.Email = userViewModel.Email;
                    user.PhoneNumber = userViewModel.Phone;
                    user.FirstName = userViewModel.FirstName;
                    user.LastName = userViewModel.LastName;
                    user.IsSuperAdmin = false;
                    user.IsEnabled = userViewModel.IsEnabled;
                    user.IsEnabled = userViewModel.IsEnabled;
                    user.MonHours = userViewModel.MonHours;
                    user.TueHours = userViewModel.TueHours;
                    user.WedHours = userViewModel.WedHours;
                    user.ThursHours = userViewModel.ThursHours;
                    user.FriHours = userViewModel.FriHours;
                    user.SatHours = userViewModel.SatHours;
                    user.SunHours = userViewModel.SunHours;
                    createdUser = await UserManager.UpdateAsync(user);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.User.UserUpdated;
                    ReponseViewModel.TransactionType = TransactionType.Update.ToString();
                }

                return createdUser.Succeeded ?
                     Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet) :
                     Json(JsonRequestBehavior.DenyGet);

            }

            return View();
        }
        public async Task<ActionResult> Edit(string userId)
        {
            User user = await UserManager.FindByIdAsync(userId);
            UserViewModel userViewModel = new UserViewModel()
            {
                Id = Guid.Parse(user.Id),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                UserName = user.UserName,
                IsEnabled = user.IsEnabled.Value,
                MonHours = user.MonHours,
                TueHours = user.TueHours,
                WedHours = user.WedHours,
                ThursHours = user.ThursHours,
                FriHours = user.FriHours,
                SatHours = user.SatHours,
                SunHours = user.SunHours
            };
            return View("Create", userViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> IsUserNameExist(string UserName, Guid Id)
        {
            User user = await UserManager.FindByNameAsync(UserName);
            if (Id == Guid.Empty)
            {
                return user != null ?
                        Json(false, JsonRequestBehavior.AllowGet) :
                        Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return user != null && (Guid.Parse(user.Id) != Id) ?
                        Json(false, JsonRequestBehavior.AllowGet) :
                        Json(true, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost]
        public async Task<ActionResult> IsEmailExist(string Email, Guid Id)
        {
            User user = await UserManager.FindByEmailAsync(Email);

            if (Id == Guid.Empty)
            {
                return user != null ?
                        Json(false, JsonRequestBehavior.AllowGet) :
                        Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return user != null && (Guid.Parse(user.Id) != Id) ?
                        Json(false, JsonRequestBehavior.AllowGet) :
                        Json(true, JsonRequestBehavior.AllowGet);

            }
        }

        public async Task<ActionResult> UserRoles(Guid userId)
        {
            UserRolesViewModel userRolesViewModel = new UserRolesViewModel();
            var roles = RoleManager.Roles.ToList();
            User user = await UserManager.FindByIdAsync(userId.ToString());
            var userRoles = user.Roles.ToList();
            foreach (var role in userRoles)
            {
                userRolesViewModel.AssignedRoles.Add(roles.Where(r => r.Id == role.RoleId).Single() as Role);
            }
            var unAssignedRoles = roles.Except(userRolesViewModel.AssignedRoles).ToList();
            foreach (var role in unAssignedRoles)
            {
                userRolesViewModel.UnAssignedRoles.Add(role as Role);
            }
            userRolesViewModel.User.Id = Guid.Parse(user.Id);
            userRolesViewModel.User.FirstName = user.FirstName;
            userRolesViewModel.User.LastName = user.LastName;
            userRolesViewModel.User.UserName = user.LastName;
            return View(userRolesViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> AssignUserRoles(string userId, string[] assignedRoles, string[] unassignedRoles)
        {
            try
            {
                if (assignedRoles.Length > 0)
                {
                    foreach (var role in assignedRoles)
                    {
                        if (!string.IsNullOrEmpty(role))
                            await UserManager.AddToRoleAsync(userId, role);
                    }
                }
                if (unassignedRoles.Length > 0)
                {
                    foreach (var role in unassignedRoles)
                    {
                        if (!string.IsNullOrEmpty(role) && UserManager.IsInRole(userId, role))
                            await UserManager.RemoveFromRoleAsync(userId, role);
                    }
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(JsonRequestBehavior.DenyGet);
            }



        }
    }
}