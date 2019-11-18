using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Core.ViewModels
{
    public class CustomerDashboardListViewModel
    {
        public int Id { get; set; }
        public CustomerViewModel Customer { get; set; }
        public int CustomerId { get; set; }
    }
}
