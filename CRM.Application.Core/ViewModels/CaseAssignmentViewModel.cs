using CRM.Application.Core.Resources.Administration;
using CRM.Application.Core.Resources.Customers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace CRM.Application.Core.ViewModels
{
    public class CaseAssignmentViewModel
    {
        public int Id { get; set; }
        public bool Urgent { get; set; }
        public int CustomerCaseId { get; set; }
        public CustomerCase CustomerCase { get; set; }
        public int? CustomerId { get; set; }
        public string Title { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
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
