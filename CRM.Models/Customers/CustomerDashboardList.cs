using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class CustomerDashboardList
    {
        public int Id { get; set; }
        public virtual Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public UserDashboardList UserDashboardList { get; set; }
        public int UserDashboardListId { get; set; }
        public DashboardListColumn DashboardListColumn { get; set; }
        public int DashboardListColumnId { get; set; }
        public string CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User User { get; set; }
    }
}
