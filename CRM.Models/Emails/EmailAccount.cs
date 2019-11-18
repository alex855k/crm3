using System;

namespace CRM.Models
{
    public class EmailAccount
    {
        public int Id { get; set; }
        public string HostName { get; set; }
        public int HostPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime? LatestSync { get; set; }
        public int? newestUidSynced { get; set; }
        public string FullAddress { get; set; }//need to add to listview and create form
        
    }
}
