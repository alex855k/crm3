using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CRM.Web.Controllers
{
    public class LanguageController : Controller
    {
        // GET: Language
        public JsonResult ChangeCulture(string culture,string uiCulture)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(uiCulture);
                HttpCookie cookie = new HttpCookie("Language");
                cookie.Values.Add("culture", culture);
                cookie.Values.Add("uiCulture", uiCulture);
                Response.Cookies.Add(cookie);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(JsonRequestBehavior.DenyGet);
            }
            
            
        }
    }
}