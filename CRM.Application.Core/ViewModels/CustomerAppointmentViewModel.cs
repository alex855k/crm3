
using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class CustomerAppointmentViewModel : DynamicTableViewModel<CustomerAppointmentViewModel>
    {
        public CustomerAppointmentViewModel()
        {
            AppointmentsList = new List<Appointments>();
            Hours = new List<int>();
            Minutes = new List<int>();
            CustomersList = new List<CustomerViewModel>();
            CustomerNotesReportList = new List<CustomerNoteReport>();
            CustomerNotesVisitTypeList = new List<CustomerNoteVisitType>();
            CustomerNotesDemoList = new List<CustomerNoteDemo>();
        }
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public int StartTimeHour { get; set; }
        public int StartTimeMinute { get; set; }
        public int EndTimeHour { get; set; }
        public int EndTimeMinute { get; set; }
        public string Subject { get; set; }
        public string AppointmentDescription { get; set; }
        [Required(ErrorMessageResourceName = "CompanyNameRequired", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public int CustomerId { get; set; }
        public int SelectedCustomerId { get; set; }
        public string UserId { get; set; }
        public List<int> Hours { get; set; }
        public List<int> Minutes { get; set; }
        public string AppointmentColor { get; set; }
        public string AppointmentIcon { get; set; }
        public List<Appointments> AppointmentsList { get; set; }
        public List<CustomerViewModel> CustomersList { get; set; }
        public string[] AppointmentCustomerEmails { get; set; }
        public string[] AppointmentSalesPersonEmails { get; set; }
        public string AppointmentNote { get; set; }
        public bool IsSaveAppointmentNote { get; set; }
        public int? CustomerNotesReportId { get; set; }
        public List<CustomerNoteReport> CustomerNotesReportList { get; set; }
        public int? CustomerNotesVisitTypeId { get; set; }
        public List<CustomerNoteVisitType> CustomerNotesVisitTypeList { get; set; }
        public int? CustomerNotesDemoId { get; set; }
        public List<CustomerNoteDemo> CustomerNotesDemoList { get; set; }
        public bool IsResendEmails { get; set; }
        public int CustomerStatusId { get; set; }
        public StaticPagedList<CustomerAppointment> CustomerAppointmentsList { get; set; }
    }

    public class Appointments
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime start { get; set; }
        public bool allDay { get; set; }
        public object className { get; set; }
        public string icon { get; set; }
    }
}
