using CRM.Models;
using System;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class EmailMessageViewModel : DynamicTableViewModel<EmailMessage>
    {
        public int Id { get; set; }
        public int MailBox_Uid { get; set; }
        public string Sender { get; set; }
        public string[] Recipients { get; set; }
        public int CustomerId { get; set; }
        public string CustomerCompanyName { get; set; }
        public string Subject { get; set; }
        public byte[] Content { get; set; }
        public string TextMessage { get; set; }
        public DateTime DateSent { get; set; }
        public StaticPagedList<EmailMessage> EmailMessagesPL { get; set; }
    }
}
