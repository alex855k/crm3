using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity.Owin;

namespace CRM.Web.Controllers
{
    
    public class StatisticsController : Controller
    {
        UnitofWork _uow = new UnitofWork();
        public UserManager UserManager => System.Web.HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();

        [CRMAuthorize]
        public ActionResult Index()
        {
            //why does this method exist?
            return View("StatisticsIndex");
        }
        [Authorize]
        public ActionResult GetTimeRegStatistics(string from, string to )
        {
            DateTime fromDate = (DateTime.Parse(from)).Date; 
            DateTime toDate = (DateTime.Parse(to)).Date;
            List<Stat> stats = new List<Stat>();
            try
            {

            
                var periodEnumerable = _uow.TimeRegistrationRepo.Search(x=> System.Data.Entity.DbFunctions.TruncateTime(x.StartDateTime) >= fromDate && System.Data.Entity.DbFunctions.TruncateTime(x.StartDateTime) <= toDate);
                var periodList = periodEnumerable.ToList();
                var userslist = UserManager.Users.Where(x=> x.IsSuperAdmin != null && (bool) !x.IsSuperAdmin).ToList();
                foreach (var i in userslist)
                {
                    Stat stat = new Stat()
                    {
                        User = new User() // don't send passwordhash
                        {
                            Id = i.Id,
                            FirstName = i.FirstName,
                            LastName = i.LastName,
                            MonHours = i.MonHours,
                            TueHours = i.TueHours,
                            WedHours = i.WedHours,
                            ThursHours = i.ThursHours,
                            FriHours = i.FriHours,
                            SatHours = i.SatHours,
                            SunHours = i.SunHours
                        }

                    };

                    var userEnumerable = periodEnumerable.Where(x => x.UserId == i.Id);
                    if (userEnumerable.Any())
                    {
                        for (DateTime date = fromDate; date.Date <= toDate.Date; date = date.AddDays(1))
                        {
                            var timeRegistrations = userEnumerable.Where(x => x.StartDateTime.Date == date);
                            var list = timeRegistrations.ToList();
                            if (timeRegistrations.Any())
                            {
                                Day day = new Day()
                                {
                                    Date = date,
                                    TimeRegistrations = timeRegistrations.ToList(),
                                };
                                day.calcDayTotalTime();

                                stat.Days.Add(day);
                            }
                        }

                        if (stat.Days.Count > 0)
                        {
                            stat.calcTotalTime();
                        }
                    }

                    stats.Add(stat);
                }

                return Json(new{success = true, Stats = stats, responseText = "success" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new {error = true,stack = e, responseText = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

    }

    internal class Stat
    {
        public User User { get; set; }
        public string TotalTimeIso { get; set; }
        public TimeSpan TotalTime {
            get => !string.IsNullOrEmpty(TotalTimeIso) ? XmlConvert.ToTimeSpan(TotalTimeIso) : TimeSpan.Zero;
            set => TotalTimeIso = XmlConvert.ToString((TimeSpan)value);
        }
        public List<Day> Days = new List<Day>();

        public void calcTotalTime()
        {
            TimeSpan time = new TimeSpan();
            foreach (var i in Days)
            {
                time = time.Add(i.DayTotalTime);
            }
            TotalTime = time;
        }
    }

    internal class Day
    {
        public DateTime Date { get; set; }
        public string DayTotalTimeIso { get; set; }
        public TimeSpan DayTotalTime {
            get => !string.IsNullOrEmpty(DayTotalTimeIso) ? XmlConvert.ToTimeSpan(DayTotalTimeIso) : TimeSpan.Zero;
            set => DayTotalTimeIso = XmlConvert.ToString((TimeSpan)value);
        }
        public List<TimeRegistration> TimeRegistrations = new List<TimeRegistration>();


        public void calcDayTotalTime()
        {
            TimeSpan time = new TimeSpan();
            foreach (var i in TimeRegistrations)
            {
                if (!i.IsActive)
                {
                    if (i.Interval != null) time = time.Add((TimeSpan) i.Interval);
                }
            }
            DayTotalTime = time;
        }
    }
}