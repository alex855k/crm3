using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class DashboardListColumn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DashboardList DashboardList { get; set; }
        public int DashboardListId { get; set; }
        public int ColumnOrder { get; set; }
        public virtual ICollection<CustomerDashboardList> CustomerDashboardLists { get; set; }
    }
}
