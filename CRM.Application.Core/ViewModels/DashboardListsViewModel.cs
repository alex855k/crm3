using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CRM.Application.Core.ViewModels
{
    public class DashboardListsViewModel
    {
        public DashboardListsViewModel()
        {
            DashboardLists = new List<DashboardListsViewModel>();
            UserDashboardLists = new List<DashboardListsViewModel>();
            DashboardListColumns = new List<DashboardListColumnsViewModel>();

        }
        public int Id { get; set; }
        [Required(ErrorMessageResourceName = "ListNameRequired", ErrorMessageResourceType = typeof(Resources.Administration.DashboardLists))]
        //[Remote("IsListNameExist", "DashboardLists", AdditionalFields = "Name,Id", ErrorMessageResourceName = "ListNameExist", ErrorMessageResourceType = typeof(Resources.Administration.DashboardLists), HttpMethod = "Post")]
        public string Name { get; set; }
        public String Description { get; set; }
        public List<DashboardListsViewModel> DashboardLists { get; set; }
        public List<DashboardListsViewModel> UserDashboardLists { get; set; }
        public List<DashboardListColumnsViewModel> DashboardListColumns { get; set; }
        public List<Customer> CustomersList { get; set; }


    }
}
