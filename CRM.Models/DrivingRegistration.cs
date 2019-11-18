using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Xml;

namespace CRM.Models
{
    public class DrivingRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime DateOfDrive { get; set; }
        public string CaseId { get; set; }
        public virtual CustomerCase Case { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }

        public decimal StartMileage { get; set; }
        public decimal EndMileage { get; set; }
        [NotMapped]
        public decimal Distance {
            get
            {
                return EndMileage - StartMileage;
            }
        }
    }
}