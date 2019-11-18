using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace CRM.Models
{
    public class CaseAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool Urgent { get; set; }
        public int CustomerCaseId { get; set; }
        public CustomerCase CustomerCase { get; set; }
        public string Title { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        [Required(ErrorMessage = "this field is required ahaha")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "this field is required ahaha")]
        public DateTime Deadline { get; set; }
        public int? LinkedCaseAssignmentId { get; set; }
        [NotMapped]
        public TimeSpan EstimatedTimeSpan
        {
            get
            {
                if (!string.IsNullOrEmpty(EstimatedTimeIsoString))
                {
                    return XmlConvert.ToTimeSpan(EstimatedTimeIsoString);
                }

                return TimeSpan.Zero;
            }
            set => EstimatedTimeIsoString = XmlConvert.ToString(value);
        }
        public string EstimatedTimeIsoString { get; set; }
        public string Description { get; set; }
        public bool? Done { get; set; }
        public bool AddToCaseEstimate { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}


