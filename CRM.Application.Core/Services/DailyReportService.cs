using CRM.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using CRM.Models;
using Microsoft.AspNet.Identity;
using System.Threading;

namespace CRM.Application.Core.Services
{
    public class DailyReportService
    {
        UnitofWork _uow = new UnitofWork();
        public bool IsDailyReportExistsToday()
        {
            var currentUser = Thread.CurrentPrincipal.Identity.GetUserId();
            var isTodayReportExist = _uow.DailyReportsRepo
                .Search(x => DbFunctions.TruncateTime(x.Date) == DbFunctions.TruncateTime(DateTime.Now) &&
                 x.UserId == currentUser)
                .SingleOrDefault();
            return isTodayReportExist == null ? false : true;
        }

        public void CreateDailyReport()
        {
            var currentUser = Thread.CurrentPrincipal.Identity.GetUserId();
            if (!IsDailyReportExistsToday())
            {
                _uow.DailyReportsRepo.Add(new DailyReport { Date = DateTime.Now, UserId = currentUser });
                _uow.SaveChanges();
            }
        }
    }
}
