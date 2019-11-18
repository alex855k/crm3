using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class DynamicTableQueryResult<T> where T : class
    {
        public IList<T> QueryResultList { get; set; }
        public IList<T> QueryResultListAllResults { get; set; }

        public int QueryCount { get; set; }
        public int TableCount { get; set; }
    }
}
