
using CRM.Models;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class DailyReportViewModel : DynamicTableViewModel<DailyReport>
    {
        public DailyReportViewModel()
        {
            CustomerNotesList = new List<CustomerNote>();
            UsersList = new List<User>();
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int KmFrom { get; set; }
        public int KmTo { get; set; }
        public string SearchKey { get; set; }
        public string SelectedUserId{ get; set; }
        public StaticPagedList<DailyReport> DailyReportList { get; set; }
        public List<CustomerNote> CustomerNotesList { get; set; }
        public IEnumerable<User> UsersList { get; set; }
    }
}

