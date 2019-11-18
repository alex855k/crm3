using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class CompanyProfileRepo<T> : BaseRepository<T> where T : class
    {
        public CompanyProfileRepo(CRMContext context) : base(context)
        {
        }
    }
}
