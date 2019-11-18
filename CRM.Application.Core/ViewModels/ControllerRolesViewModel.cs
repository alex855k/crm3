using CRM.Models;
using System.Collections.Generic;

namespace CRM.Application.Core.ViewModels
{
    public class ControllerRolesViewModel
    {
        public ControllerRolesViewModel()
        {
            AssignedRoles = new List<Role>();
            UnAssignedRoles = new List<Role>();
        }
        public int Id { get; set; }
        public string PageName { get; set; }
        public string ActionName { get; set; }
        
        public List<Role> AssignedRoles { get; set; }
        public List<Role> UnAssignedRoles { get; set; }
    }
}