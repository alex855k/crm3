using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class DashboardListsRepo<T> : BaseRepository<T> where T : class
    {
        public DashboardListsRepo(CRMContext context) : base(context)
        {
        }
    }
}
