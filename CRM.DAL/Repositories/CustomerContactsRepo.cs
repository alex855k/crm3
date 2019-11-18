using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class CustomerContactsRepo<T> : BaseRepository<T> where T : class
    {
        public CustomerContactsRepo(CRMContext context) : base(context)
        {
        }
    }
}
