using CRM.DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class CustomersRepo<T> : BaseRepository<T> where T : class
    {
        public CustomersRepo(CRMContext context) : base(context)
        {
           
        }
    }
}
