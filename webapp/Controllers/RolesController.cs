using CRM.Application.Core.Enums;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CRM.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class RolesController : Controller
    {
        private RoleManager _roleManager;

        public RolesController()
        {
        }

        public RolesController(RoleManager roleManager)
        {
            _roleManager = roleManager;
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

        public ActionResult Index()
        {
            
            RoleViewModel roleViewModel = new RoleViewModel();
            var roles = RoleManager.Roles.ToList();
            foreach (var role in roles)
            {
                roleViewModel.rolesList.Add(role as Role);
            }
            return View(roleViewModel);
        }
        public ActionResult Create()
        {
            return View("_Form", new RoleViewModel());
        }
        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel roleViewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityResult createdRole = null;
                if (roleViewModel.Id == Guid.Empty)
                {
                     createdRole = await RoleManager.CreateAsync(new Role
                    {
                        Name = roleViewModel.Name,
                        Description = roleViewModel.Description
                    });
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.Role.RoleSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                }
                else
                {
                    Role role = await RoleManager.FindByIdAsync(roleViewModel.Id.ToString());
                    role.Name = roleViewModel.Name;
                    role.Description = roleViewModel.Description;
                    createdRole = await RoleManager.UpdateAsync(role);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.Role.RoleUpdated;
                    ReponseViewModel.TransactionType = TransactionType.Update.ToString();
                }

                return createdRole.Succeeded ?
                 Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet) :
                 Json(JsonRequestBehavior.DenyGet);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public async Task<ActionResult> Edit(string roleId)
        {
            var role = await RoleManager.FindByIdAsync(roleId);
            RoleViewModel roleViewModel = new RoleViewModel()
            {
                Id = Guid.Parse(role.Id),
                Name = role.Name,
                Description = role.Description
            };
            return View("_Form", roleViewModel);
        }
        public async Task<ActionResult> IsRoleExist(string Name,Guid Id)
        {
            var role = await RoleManager.FindByNameAsync(Name);
            if (Id == Guid.Empty)
            {
                return role != null ?
                        Json(false, JsonRequestBehavior.AllowGet) :
                        Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return role != null && (Guid.Parse(role.Id) != Id) ?
                        Json(false, JsonRequestBehavior.AllowGet) :
                        Json(true, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult ControllerRoles()
        {
            UnitofWork uow = new UnitofWork();
            var categories = uow.ApplicationControllerCategoriesRepo.GetAll();
            return View(categories);
        }
        public ActionResult AssignControllerRole(int controllerId)
        {
            var roles = RoleManager.Roles.ToList();
            UnitofWork uow = new UnitofWork();
            ControllerRolesViewModel controllerRolesViewModel = new ControllerRolesViewModel();
            List<string> assignedRolesId = uow.ControllerRolesRepo.GetAssignedRolesByControllerId(controllerId);
            foreach (var roleId in assignedRolesId)
            {
                controllerRolesViewModel.AssignedRoles.Add(roles.Where(r => r.Id == roleId).Single() as Role);
            }
            controllerRolesViewModel.UnAssignedRoles = roles.Except(controllerRolesViewModel.AssignedRoles).ToList();
            controllerRolesViewModel.Id = controllerId;
            var controller = uow.ApplicationControllersRepo.Find(controllerId);
            controllerRolesViewModel.PageName = controller.ControllerName;
            controllerRolesViewModel.ActionName = controller.ActionName;
            return View(controllerRolesViewModel);
        }
        [HttpPost]
        public ActionResult AssignControllerRole(int controllerId, string[] assignedRolesId)
        {
            UnitofWork uow = new UnitofWork();
            ControllerRolesViewModel controllerRolesViewModel = new ControllerRolesViewModel();
            List<ControllerRole> controllerRole = new List<ControllerRole>();

            try
            {

                List< ControllerRole> controllerRoleResult = uow.ControllerRolesRepo.Search(x => x.ControllerId == controllerId).ToList();
                if(controllerRoleResult.Count > 0)
                {
                    uow.ControllerRolesRepo.RemoveRange(controllerRoleResult);
                    uow.SaveChanges();
                }
                if (assignedRolesId.Length > 0)
                {
                    foreach (var roleId in assignedRolesId)
                    {
                        if (!string.IsNullOrEmpty(roleId))
                            controllerRole.Add(new ControllerRole
                            {
                                ControllerId = controllerId,
                                RoleId = roleId
                            });
                    }
                }
                uow.ControllerRolesRepo.AddRange(controllerRole);
                uow.SaveChanges();
                return Json(JsonRequestBehavior.AllowGet);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (DbUpdateException ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {

                return Json(JsonRequestBehavior.DenyGet);
            }
        }

    }
}