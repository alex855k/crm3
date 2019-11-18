using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class Customer
    {
        public Customer()
        {
            IsActive = true;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public int CustomerTypeId { get; set; }
        public CustomerType CustomerType{ get; set; }
        public string CVR { get; set; }
        public string EAN { get; set; }
        public string DELEAN { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Town { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string CompanyURL { get; set; }
        public string SalesPersonId { get; set; }
        public User SalesPerson { get; set; }
        public int CustomerStatusId { get; set; }
        public CustomerStatus CustomerStatus { get; set; }
        public string CustomerRowColor { get; set; }
        public string AdditionalInfo { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        public virtual ICollection<CustomerNote> CustomerNotes { get; set; }
        public virtual ICollection<CustomerDashboardList> CustomerDashboardLists { get; set; }
        public virtual ICollection<CustomerAppointment> CustomerAppointments { get; set; }

        public virtual ICollection<Referencelist> ReferenceList { get; set; }
    }
}
