using CRM.Models;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class UserDashboardListsRepo<T> : BaseRepository<T> where T : class
    {
        private CRMContext _dbContext;
        public UserDashboardListsRepo(CRMContext context) : base(context)
        {
            _dbContext = context;
        }

        public Tuple<List<DashboardList>,List<CustomerDashboardList>> GetDashboardListsByUserId(string userId)
        {
            //_dbContext.Configuration.LazyLoadingEnabled = false;
            var userdashboardIds = _dbContext.UserDashboardLists.Where(x => x.UserId.Equals(userId)).Select(x => x.DashboardListId).Distinct().ToList();
            var userDashboardLists = _dbContext.DashboardLists.Where(x => userdashboardIds.Contains(x.Id))
                .Include("DashboardListColumns").ToList();
        

            var userDashboardListCustomers = _dbContext.CustomerDashboardLists.Where(x => x.CreatedByUserId.Equals(userId))
                .Include(x => x.Customer).ToList();

            return Tuple.Create(userDashboardLists, userDashboardListCustomers);
        }
    }
}
