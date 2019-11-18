#region Using

using NLog;
using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using CRM.API.App_Start;
using CRM.Application.Core.AutoMapperMappings;

#endregion

namespace CRM.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
           
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // API Routes
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // MVC Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfiguration.ConfigureMappings();

        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["Language"];
            if (cookie != null && cookie.Value != null && cookie.Values.Count > 1)
            {
                CultureInfo.CurrentCulture = new CultureInfo(cookie.Values[0]);
                CultureInfo.CurrentUICulture = new CultureInfo(cookie.Values[1]);
            }
            else
            {
                CultureInfo.CurrentCulture = new CultureInfo("da-DK");
                CultureInfo.CurrentUICulture = new CultureInfo("da-DK");
            }
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "Logs.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;

        }
    }
}