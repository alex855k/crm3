using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM.Application.Core.ViewModels
{
    public class RoleViewModel
    {
        public RoleViewModel()
        {
            rolesList = new List<Role>();
        }
        public Guid Id { get; set; }
        [Required(ErrorMessageResourceName = "RoleNameRequired", ErrorMessageResourceType = typeof(Resources.Administration.Role))]
        [Remote("IsRoleExist", "Roles", AdditionalFields = "Name,Id", ErrorMessageResourceName = "RoleExist", ErrorMessageResourceType = typeof(Resources.Administration.Role), HttpMethod = "Post")]
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Role> rolesList { get; set; }
    }
}