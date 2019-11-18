using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class ControllerRolesRepo<T> : BaseRepository<T> where T : class
    {
        private CRMContext _dbContext;
        public ControllerRolesRepo(CRMContext context) : base(context)
        {
            _dbContext = context;
        }
        public List<String> GetRolesByControllerName(string controllerName)
        {
            return _dbContext.ControllerRoles
                 .Where(x => x.Controller.ControllerName == controllerName)
                 .Select(x => x.Role.Name)
                 .ToList();
        }
        public List<ControllerRole> GetControllerRolesByRoles(List<string> roleIds)
        {
            return _dbContext.ControllerRoles
                 .Where(x => roleIds.Contains(x.RoleId))
                 .ToList();
        }
        public List<string> GetAssignedRolesByControllerId(int controllerId)
        {
            return _dbContext.ControllerRoles
                 .Where(x => x.ControllerId == controllerId)
                 .Select(x => x.RoleId)
                 .ToList();
        }
    }
}
