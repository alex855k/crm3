using CRM.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Core.ViewModels
{
    public class CustomerContactsViewModel
    {
        public CustomerContactsViewModel()
        {
            CustomerContactsList = new List<CustomerContact>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        [Required(ErrorMessageResourceName = "NameRequierd", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public string Name { get; set; }
        [EmailAddress(ErrorMessageResourceName = "ValidEmail", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public int CustomerId { get; set; }
        public List<CustomerContact> CustomerContactsList { get; set; }
    }
}