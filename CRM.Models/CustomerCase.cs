using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace CRM.Models
{
    public enum CaseStatus {IkkePlanlagt, Igang, Afsluttet, Afventer, AfventerKunde, Afventer3part, Pause }
    
    public class CustomerCase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Titel { get; set; }
        public int CustomerCaseTypeId { get; set; }
        public CustomerCaseType CustomerCaseType { get; set; }
        public int CustomerId { get; set; }
        public CaseStatus Status { get; set; }
        public Customer Customer { get; set; }
        public string ProjectResponsibleId { get; set; }
        public User ProjectResponsible { get; set; }
        public bool Pinned { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? Deadline { get; set; }
        public string UserId { get; set; }//case responsible
        public User User { get; set; }//case responsible
        public int? CustomerContactId { get; set; }
        public CustomerContact Contact { get; set; }
        public TimeSpan EstimatedTimeSpan
        {
            get
            {
                if (!string.IsNullOrEmpty(EstimatedTimeSpanIsoString))
                {
                    return XmlConvert.ToTimeSpan(EstimatedTimeSpanIsoString);
                }

                return TimeSpan.Zero;
            }
            set => EstimatedTimeSpanIsoString = XmlConvert.ToString(value);
        }
        public string EstimatedTimeSpanIsoString { get; set; }

        [NotMapped]
        public TimeSpan TotalTimeUsed
        {
            get
            {
                if (!string.IsNullOrEmpty(TotalTimeUsedIsoString))
                {
                    return XmlConvert.ToTimeSpan(TotalTimeUsedIsoString);
                }

                return TimeSpan.Zero;
            }
            set => TotalTimeUsedIsoString = XmlConvert.ToString(value);
        }
        public string TotalTimeUsedIsoString { get; set; }
        public string Description { get; set; }
        public int? PercentDone { get; set; }
        public bool Done { get; set; }
        public DateTime? EndDateTime { get; set; }
        public double? ImportanceRank { get; set; }
        public string Week { get; set; }
    }
}