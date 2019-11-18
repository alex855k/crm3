using CRM.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Hangfire;

namespace CRM.Web.Controllers
{
    public class EnableDisableSystemFeaturesController : Controller
    {
        // GET: EnableDisableSystemFeatures
        UnitofWork _uow = new UnitofWork();
        public ActionResult Index()
        {
            var categories = _uow.ApplicationControllerCategoriesRepo.GetAll();
            return View(categories);
        }

        public ActionResult EnableDisableFeature(int id, bool isDisabled)
        {
            var feature = _uow.ApplicationControllersRepo.Find(id);
            feature.IsDisabled = isDisabled;
            _uow.ApplicationControllersRepo.Update(feature);
            try
            {
                _uow.SaveChanges();

                if (feature.ActionName == "SendSms")
                {
                    if (!isDisabled)
                    {
                        string monThurCron = WebConfigurationManager.AppSettings["SmsMonThuMin"] +" "+
                                             WebConfigurationManager.AppSettings["SmsMonThuHour"] +" "+ "* * 1-4";
                        string fridayCron = WebConfigurationManager.AppSettings["SmsFridayMin"] + " " +
                                            WebConfigurationManager.AppSettings["SmsFridayHour"] + " " + "* * 5";

                        RecurringJob.AddOrUpdate<TimeregistrationController>(x => x.SendCheckoutReminder(), monThurCron, TimeZoneInfo.Local);

                        RecurringJob.AddOrUpdate<TimeregistrationController>(x => x.SendCheckoutReminderFriday(), fridayCron, TimeZoneInfo.Local);
                    }
                    else
                    {
                        RecurringJob.RemoveIfExists("TimeregistrationController.SendCheckoutReminder");
                        RecurringJob.RemoveIfExists("TimeregistrationController.SendCheckoutReminderFriday");
                    }
                }

            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return isDisabled ?
            Json(CRM.Application.Core.Resources.Administration.EnableDisableFeature.SystemFeatureDisabled, JsonRequestBehavior.AllowGet) :
            Json(CRM.Application.Core.Resources.Administration.EnableDisableFeature.SystemFeatureEnabled, JsonRequestBehavior.AllowGet);
        }
    }
}