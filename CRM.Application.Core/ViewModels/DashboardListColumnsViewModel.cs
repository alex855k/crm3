using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CRM.Application.Core.ViewModels
{
    public class DashboardListColumnsViewModel
    {
        public DashboardListColumnsViewModel()
        {
            CustomerDashboardListViewModel = new List<CustomerDashboardListViewModel>();
        }
        public int Id { get; set; }
        [Required(ErrorMessageResourceName = "ColumnNameRequired", ErrorMessageResourceType = typeof(Resources.Administration.DashboardLists))]
        //[Remote("IsListColumnNameExist", "DashboardLists", AdditionalFields = "Name,Id,DashboardListId", ErrorMessageResourceName = "ColumnNameExist", ErrorMessageResourceType = typeof(Resources.Administration.DashboardLists), HttpMethod = "Post")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int DashboardListId { get; set; }
        public List<DashboardListColumnsViewModel> DashboardListColumnsList { get; set; }
        public List<CustomerDashboardListViewModel> CustomerDashboardListViewModel { get; set; }
        public List<CustomerViewModel> Customers { get; set; }
    }
}
