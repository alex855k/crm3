using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace CRM.Models
{
    public class CustomerCaseType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public bool? Repeats { get; set; }

        [NotMapped]
        public TimeSpan RepeatsSpan
        {
            get
            {
                if (!string.IsNullOrEmpty(RepeatsSpanIsoString))
                {
                    return XmlConvert.ToTimeSpan(RepeatsSpanIsoString);
                }

                return TimeSpan.Zero;
            }
            set => RepeatsSpanIsoString = XmlConvert.ToString(value);
        }
        public string RepeatsSpanIsoString { get; set; }
        public bool? Invoiced { get; set; }
        //Who plans the repeat        
        public string UserId { get; set; }
        public User User { get; set; }
        [NotMapped]
        public TimeSpan? DaysBefore
        {
            get
            {
                if (!string.IsNullOrEmpty(DaysBeforeIsoString))
                {
                    return XmlConvert.ToTimeSpan(DaysBeforeIsoString);
                }

                return TimeSpan.Zero;
            }
            set => DaysBeforeIsoString = XmlConvert.ToString((TimeSpan) value);
        }
        public string DaysBeforeIsoString { get; set; }
    }
}