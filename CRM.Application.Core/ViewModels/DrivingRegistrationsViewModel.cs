using CRM.Application.Core.Resources.Customers;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class DrivingRegistrationViewModel
    {
        public string UserId { get; set; }
        public string StartKm { get; set; }
        public string EndKm { get; set; }
        public TimeSpan DateOfDriving { get; set; }
        public StaticPagedList<CustomerCase> CustomerCases { get; set; }
        public string CurrentUserID { get; set; }
        public int? CustomerId { get; set; }
        public List<Customer> CustomersList { get; set; }

        public string SearchKey { get; set; }
    }
}
