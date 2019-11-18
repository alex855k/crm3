using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class BudgetViewModel :DynamicTableViewModel<BudgetViewModel>
    {
        public BudgetViewModel()
        {
            SalesPersonList = new List<UserViewModel>();
        }
        public int Id { get; set; }
        public decimal BudgetAmount { get; set; }
        public DateTime BudgetDate { get; set; }
        public string SalesPersonId { get; set; }
        public List<UserViewModel> SalesPersonList { get; set; }
        public User SalesPerson { get; set; }
        public StaticPagedList<Budget> BudgetList { get; set; }
    }
}
