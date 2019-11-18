using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.Application.Core.ViewModels
{
    public class UserRolesViewModel
    {
        public UserRolesViewModel()
        {
            AssignedRoles = new List<Role>();
            UnAssignedRoles = new List<Role>();
            User = new UserViewModel();
        }
        public Guid Id { get; set; }
        public UserViewModel User { get; set; }
        public List<Role> AssignedRoles { get; set; }
        public List<Role> UnAssignedRoles { get; set; }
    }
}