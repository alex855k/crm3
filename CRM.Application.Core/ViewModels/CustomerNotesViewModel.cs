using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM.Application.Core.ViewModels
{
    public class CustomerNotesViewModel
    {
        public CustomerNotesViewModel()
        {
            CustomerNotesReportList = new List<CustomerNoteReport>();
            CustomerNotesVisitTypeList = new List<CustomerNoteVisitType>();
            CustomerNotesDemoList = new List<CustomerNoteDemo>();
            CustomerNotesList = new List<CustomerNote>();
            Attachments = new List<string>();
        }
        public int Id { get; set; }
        [Required(ErrorMessageResourceName = "NoteRequired",ErrorMessageResourceType =typeof(Resources.Customers.Customer))]
        public string Note { get; set; }
        public int? CustomerNotesReportId { get; set; }
        public List<CustomerNoteReport> CustomerNotesReportList { get; set; }
        public int? CustomerNotesVisitTypeId { get; set; }
        public List<CustomerNoteVisitType> CustomerNotesVisitTypeList { get; set; }
        public int? CustomerNotesDemoId { get; set; }
        public List<CustomerNoteDemo> CustomerNotesDemoList { get; set; }
        public int CustomerId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public List<string> Attachments { get; set; }
        public List<string> RemovedAttachments { get; set; }
        public List<CustomerNote> CustomerNotesList { get; set; }

    }
}