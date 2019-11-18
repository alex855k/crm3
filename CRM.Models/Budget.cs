using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public decimal BudgetAmount { get; set; }
        public virtual User SalesPerson { get; set; }
        public string SalesPersonId { get; set; }
        public DateTime BudgetDate { get; set; }
    }
}
