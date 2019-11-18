using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class CustomerAppointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Subject { get; set; }
        public string AppointmentColor { get; set; }
        public string AppointmentIcon { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }
        public string AppointmentDescription { get; set; }
        public string AppointmentNote { get; set; }
        public bool IsSaveAppointNote{ get; set; }
        public virtual List<AppointmentEmail> AppointmentEmails { get; set; }
    }
}
