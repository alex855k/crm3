using System;
using System.Collections.Generic;

namespace CRM.Models
{
    public class EmailMessage
    {
        public int Id { get; set; }
        public string AltId { get; set; }
        public uint MailBox_Uid { get; set; }
        public string Sender { get; set; }
        public string Recipients { get; set; }
        public int CustomerId { get; set; }
        public string CustomerCompanyName { get; set; }
        public virtual Customer Customer { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime? DateSent { get; set; }
        public int EmailAccountId { get; set; }
        public virtual EmailAccount EmailAccount { get; set; }
    }
}