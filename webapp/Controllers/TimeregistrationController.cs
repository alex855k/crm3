using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Data.Entity;
using CRM.DAL;
using Hangfire;
using CRM.Web.Extensions;
using CRM.Application.Core.ViewModels;
using User = CRM.Models.User;

namespace CRM.Web.Controllers
{
   public class TimeregistrationController : Controller
    {
        private UnitofWork _uow = new UnitofWork();
        //private UserManager _userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
        public ActionResult TimeRegistrationModal()
        {
            return View("_TimeRegistrationModal");
        }
        public ActionResult TimeRegistrationList()
        {
            return View("_TimeRegistrationList");
        }

        public ActionResult TimeRegistrationForm()
        {
            return View("_TimeRegistrationForm");
        }

        public ActionResult CreateTimeReg(TimeRegistration timeReg, string startTime, string endTime)
        {
            timeReg.StartDateTime = DateTime.Parse(startTime);
            timeReg.EndDateTime = DateTime.Parse(endTime);
            timeReg.Interval = timeReg.EndDateTime - timeReg.StartDateTime;
            try
            {
                _uow.TimeRegistrationRepo.Add(timeReg);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    responseText = "failed."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CreateCaseTimeReg(TimeRegistration timeReg, int customerId, string startTime, string endTime)
        {
            timeReg.StartDateTime = DateTime.Parse(startTime);
            timeReg.EndDateTime = DateTime.Parse(endTime);
            timeReg.Interval = timeReg.EndDateTime - timeReg.StartDateTime;
            if (timeReg.CustomerCaseId == null)
            {
                var gen = _uow.CustomerCaseRepo.Search(x => x.CustomerId == customerId && x.Titel == "Generel").ToList();
                if (gen.Any())
                {
                    timeReg.CustomerCaseId = (int) gen.FirstOrDefault()?.Id;
                }
                else
                {
                    var CustomerCaseType = _uow.CustomerCaseTypeRepo.Search(x => x.TypeName == "Generel").ToList();

                    CustomerCase genCase = new CustomerCase()
                    {
                        UserId = timeReg.UserId,
                        CustomerId = customerId,
                        Titel = "Generel",
                        Deadline = null,
                        StartDateTime = DateTime.Now,
                        CustomerCaseTypeId = CustomerCaseType.FirstOrDefault().Id
                    };
                    _uow.CustomerCaseRepo.Add(genCase);
                    _uow.SaveChanges();
                    timeReg.CustomerCaseId = genCase.Id;
                }
            }
            try
            {
                _uow.TimeRegistrationRepo.Add(timeReg);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    responseText = "failed."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteTimeReg(int timeRegId)
        {
            try
            {
                var timeReg = _uow.TimeRegistrationRepo.Find(timeRegId);

                _uow.TimeRegistrationRepo.Remove(timeRegId);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Deleting Failed" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult AddJobs()
        {
            RecurringJob.AddOrUpdate<TimeregistrationController>(x => x.SendCheckoutReminder(), "0 16 * * 1-4");

            RecurringJob.AddOrUpdate<TimeregistrationController>(x => x.SendCheckoutReminderFriday(), "0 15 * * 5");
            return Json(new { success = true, responseText = "good good" }, JsonRequestBehavior.AllowGet);

        }

        public bool SendCheckoutReminderFriday()
        {
            return SendCheckoutReminder();

        }
        public  bool SendCheckoutReminder()
        {
            var activeTimeRegistrations = _uow.TimeRegistrationRepo.SearchInclude(x => x.IsActive == true, "User").ToList();
            foreach (var a in activeTimeRegistrations)
            {
                if (a.User.PhoneNumber != null)
                {
                    Sms sms = new Sms()
                    {
                        message = WebConfigurationManager.AppSettings["SmsTemplate"],
                        recipient = "45" + a.User.PhoneNumber,
                        sender = WebConfigurationManager.AppSettings["SmsStandardSender"]
                    };
                    _ = sms.Send();
                }
            }


            return true;
        }

        public ActionResult GetCaseTimeRegs(string UserId, string dateTime)
        {
            try
            {
                List<TimeRegistration> TimeRegs;

                if (dateTime != null)
                {
                    var searchDate = DateTime.Parse(dateTime).Date;
                    TimeRegs = _uow.TimeRegistrationRepo.SearchInclude(x => UserId == x.UserId && DbFunctions.TruncateTime(x.StartDateTime) == searchDate, "User").OrderByDescending(x => x.StartDateTime).ToList();

                    foreach (var t in TimeRegs)
                    {
                        var user = new User()
                        {
                            FirstName = t.User.FirstName,
                            LastName = t.User.LastName,
                            Id = t.User.Id,
                            MonHours = t.User.MonHours,
                            TueHours = t.User.TueHours,
                            WedHours = t.User.WedHours,
                            ThursHours = t.User.ThursHours,
                            FriHours = t.User.FriHours,
                            SatHours = t.User.SatHours,
                            SunHours = t.User.SunHours
                        };
                        t.User = user;
                        if (t.GetType() == typeof(TimeRegistration))
                        {
                            t.CustomerCase = _uow.CustomerCaseRepo.Find(t.CustomerCaseId);
                            t.CaseAssignment = _uow.CaseAssignmentRepo.Find(t.CaseAssignmentId);
                        }
                    }
                }
                else
                {
                    TimeRegs = _uow.TimeRegistrationRepo.SearchInclude(x => UserId == x.UserId, "User").OrderByDescending(x => x.StartDateTime).Take(15).ToList();

                    foreach (var t in TimeRegs)
                    {
                        var user = new User()
                        {
                            FirstName = t.User.FirstName,
                            LastName = t.User.LastName,
                            Id = t.User.Id,
                            MonHours = t.User.MonHours,
                            TueHours = t.User.TueHours,
                            WedHours = t.User.WedHours,
                            ThursHours = t.User.ThursHours,
                            FriHours = t.User.FriHours,
                            SatHours = t.User.SatHours,
                            SunHours = t.User.SunHours
                        };
                        t.User = user;
                        if (t.GetType() == typeof(TimeRegistration))
                        {
                            t.CustomerCase = _uow.CustomerCaseRepo.Find(t.CustomerCaseId);
                            t.CaseAssignment = _uow.CaseAssignmentRepo.Find(t.CaseAssignmentId);
                            
                        }
                    }
                }

                return Json(new { success = true, TimeRegsList = TimeRegs, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting TimeRegs Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TimeRegEdit(TimeRegistration timeReg)
        {
            try
            {
                var oldTimeReg = _uow.TimeRegistrationRepo.Find(timeReg.Id);



                oldTimeReg.Interval = timeReg.EndDateTime - timeReg.StartDateTime;
                oldTimeReg.StartDateTime = timeReg.StartDateTime;
                oldTimeReg.EndDateTime = timeReg.EndDateTime;

                _uow.TimeRegistrationRepo.Update(oldTimeReg);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "Success." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTimeReg(int TimeRegId)
        {
            try
            {
                var TimeReg = _uow.TimeRegistrationRepo.Find(TimeRegId);

                return Json(new { success = true, TimeReg = TimeReg, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting Time Registration Failed" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult TimeRegsForWeek(int offset)
        {
            //for every user get sum of:
            // - invoicable time registered
            // - non invoicable time registered

            //within the timeframe of a given week (offset is relative to current week number)

            //(optionally) with a given case casetype

            //return as list of (Firstname + hh MM)



            //var testlist1 = _uow.CaseTimeRegistrationRepo.SearchInclude(x => x.CustomerCase.Titel.StartsWith("T"), "CustomerCase")
            //    .ToList();


            try
            {
                var listResult = new List<TimeRegistrationViewModel>();

                DateTime dt = DateTime.Now.AddDays(offset * 7);
                DateTime firstOfWeek = dt.GetStartOfWeekDate(DayOfWeek.Monday);
                DateTime lastOfWeek = firstOfWeek.AddDays(7).AddTicks(-1);

                var userCaseTimeRegsQuery = 
                    from tr in _uow.DbContext.TimeRegistrations.Include(x => x.User).Include(x => x.CustomerCase)
                    where
                        tr.EndDateTime.HasValue &&
                        tr.StartDateTime > firstOfWeek &&
                        tr.EndDateTime < lastOfWeek
                    select tr;


                if (!userCaseTimeRegsQuery.Any())
                {
                    //return empty viewmodel
                    return PartialView("_TimeRegsForWeek", listResult);
                }

                var TRByUser = userCaseTimeRegsQuery
                    .Include(x => x.CustomerCase)
                        .GroupBy(x => x.User)
                        .Select(x => new
                        {
                            TimeRegs = x.Select(y => new { 
                                RegId = y.Id,
                                CaseTypeId = y.CustomerCase.CustomerCaseTypeId,
                                TimeReg = y
                                //help I'm nested in a nested nest of nests
                            }),
                            UserId = x.Key.Id,
                            UserName = x.Key.FirstName
                        })
                    .ToList();




                int[] invoicable_caseTypes = _uow.CustomerCaseTypeRepo

                        .Search(x => x.Invoiced.HasValue && x.Invoiced.Value == true)
                        .Select(x => x.Id)
                        .ToArray();


                foreach (var user in TRByUser)
                {

                    var invoiceable_Cases = user.TimeRegs
                        .Where(x => invoicable_caseTypes.Contains(x.CaseTypeId))
                        .ToArray();

                    var nonInvoiceable_Cases = user.TimeRegs
                        .Where(x => !invoiceable_Cases.Select(y => y.RegId).Contains(x.RegId))
                        .ToArray();

                    var invoiceable_ticks = invoiceable_Cases.Sum(x => x.TimeReg.Interval.Value.Ticks);
                    var nonInvoiceable_ticks = nonInvoiceable_Cases.Sum(x => x.TimeReg.Interval.Value.Ticks);

                    var viewModel = new TimeRegistrationViewModel
                    {
                        UserId = user.UserId,
                        FirstName = user.UserName,
                        //UserInitials = userTimeReg.User.FirstName?.Substring(0, 1) + userTimeReg.User.LastName?.Substring(0, 1),
                        InvoiceableTime = new TimeSpan(invoiceable_ticks),
                        NonInvoiceableTime = new TimeSpan(nonInvoiceable_ticks)
                        
                    };

                    listResult.Add(viewModel);
                }


                //JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                //var data = JsonConvert.SerializeObject(new { data = user_regs, success = true, responseText = "success" }, Formatting.Indented, jss);


                return PartialView("_TimeRegsForWeek", listResult);

                //return Content(data, "application/json");
            }
            catch (Exception e)
            {
                throw;
                //return Json(new { success = false, responseText = "Something went wrong" }, JsonRequestBehavior.AllowGet);
            }

            
        }

    }
}