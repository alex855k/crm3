using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class AppointmentEmail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Email { get; set; }
        public int CustomerAppointmentId { get; set; }
        public CustomerAppointment CustomerAppointment { get; set; }
        public int AppointmentEmailTypeId { get; set; }
        public AppointmentEmailType AppointmentEmailType { get; set; }
    }
}
