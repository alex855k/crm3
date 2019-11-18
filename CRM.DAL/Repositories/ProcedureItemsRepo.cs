using CRM.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL
{
    public class ProcedureItemsRepo<T> : BaseRepository<T> where T : class
    {
        public ProcedureItemsRepo(CRMContext context) : base(context)
        {
        }
    }
}
