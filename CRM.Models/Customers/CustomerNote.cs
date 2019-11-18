using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class CustomerNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Note { get; set; }
        public int? CustomerNoteReportId { get; set; }
        public virtual CustomerNoteReport CustomerNoteReport { get; set; }
        public int? CustomerNoteVisitTypeId { get; set; }
        public virtual CustomerNoteVisitType CustomerNotesVisitType { get; set; }
        public int? CustomerNoteDemoId { get; set; }
        public virtual CustomerNoteDemo CustomerNoteDemo { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        //[Column("CreatedBy")]
        public string UserId { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdateNoteUserId { get; set; }
        public DateTime? UpdateNoteDate { get; set; }
        public virtual User User { get; set; }
        [ForeignKey("UpdateNoteUserId")]
        public virtual User UpdateNoteUser { get; set; }
        public CustomerAppointment CustomerAppointment { get; set; }
        public int? CustomerAppointmentId { get; set; }




    }
}
