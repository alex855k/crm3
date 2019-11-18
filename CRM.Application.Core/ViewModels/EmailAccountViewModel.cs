using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class EmailAccountViewModel : DynamicTableViewModel<EmailAccount>
    {
        public int Id { get; set; } = 0;
        [Required]
        //[Remote("ValidAccount", AdditionalFields = "HostPort,UserName,PassWord", ErrorMessageResourceName = "InvalidAccoount", ErrorMessageResourceType = typeof(Resources.Email.Email))]
        public string HostName { get; set; }
        public int Port { get; set; } = 993;//IMAP over SSL
        public IEnumerable<EmailProtocol> ProtocolOptions { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string PassWord { get; set; }
        public DateTime? LatestSync { get; set; }
        public int? newestUidSynced { get; set; }
        [Required, EmailAddress]
        public string FullAddress { get; set; }
        public StaticPagedList<EmailAccount> EmailAccountsPL { get; set; }
    }
}
