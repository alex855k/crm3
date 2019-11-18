using CRM.Models;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class CustomerCaseDatatableViewModel : DynamicTableViewModel<CustomerCaseDatatableViewModel>
    {
        public StaticPagedList<CustomerCase> CustomerCases { get; set; }
        public string CurrentUserID { get; set; }
        public int? CustomerId { get; set; }
        public List<Customer> CustomersList { get; set; }

        public string Titel { get; set; }
        public bool Pinned { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? Deadline { get; set; }
        public string EstimatedTimeSpanIsoString { get; private set; }
        public int? PercentDone { get; set; }
        public double? ImportanceRank { get; set; }
        public string SearchKey { get; set; }
    }
}