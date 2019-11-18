using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class OrdersRepo<T> : BaseRepository<T> where T : class
    {
        public OrdersRepo(CRMContext context) : base(context)
        {
        }
    }
}