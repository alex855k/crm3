using System;

namespace CRM.Application.Core.ViewModels
{
    public class TimeRegistrationViewModel
    {
        public string FirstName { get; set; }
        public string UserInitials { get; set; }
        public string UserId { get; set; }
        public TimeSpan InvoiceableTime { get; set; }
        public TimeSpan NonInvoiceableTime { get; set; }

    }
}
