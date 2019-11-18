using CRM.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class APICustomerViewModel : DynamicTableViewModel<Customer>
    {
        public APICustomerViewModel()
        {
            CustomerTypesList = new List<CustomerType>();
            SalesList = new List<UserViewModel>();
            CustomerStatusList = new List<CustomerStatus>();
        }
        public int Id { get; set; }
        [Required(ErrorMessageResourceName = "CompanyNameRequired", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public string CompanyName { get; set; }
        public int CustomerTypeId { get; set; }
        public List<CustomerType> CustomerTypesList { get; set; }
        public string CVR { get; set; }
        public string EAN { get; set; }
        public string DELEAN { get; set; }
        [EmailAddress(ErrorMessageResourceName = "ValidEmail", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Town { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        [Url(ErrorMessageResourceName = "ValidateURL", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public string CompanyURL { get; set; }
        public List<UserViewModel> SalesList { get; set; }
        [Required(ErrorMessageResourceName = "SalesPersonRequierd", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public string SalesPersonId { get; set; }
        [Required(ErrorMessageResourceName = "CustomerStatusRequierd", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public int CustomerStatusId { get; set; }
        public List<CustomerStatus> CustomerStatusList { get; set; }
        public string AdditionalInfo { get; set; }
        public string CustomerRowColor { get; set; }
        public string HiddenCustomerRowColor { get; set; }
        public bool IsCustomerHasNotes { get; set; }
        public StaticPagedList<Customer> CustomersList { get; set; }
        public int? LatestEditedCustomerId { get; set; }
        public int LatestCustomersPageNumber { get; set; }
        public bool NavigateToNotes { get; set; }
        public bool IsCustomers { get; set; }
        public int LastEditedCustomer { get; set; }
    }
}