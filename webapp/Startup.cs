using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using CRM.Web.Controllers;
using System.Data.Entity;

[assembly: OwinStartup(typeof(CRM.Web.Startup))]

namespace CRM.Web
{
    public class Startup
    {
        private IEnumerable<IDisposable> GetHangfireServers()
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(ConfigurationManager.ConnectionStrings["CRMContext"].ConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });

            yield return new BackgroundJobServer();
        }
        public object ApplicationSignInManager { get; private set; }
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<CRMContext, DAL.Migrations.Configuration>());
            app.CreatePerOwinContext(CRMContext.Create);
            app.CreatePerOwinContext<UserManager>(UserManager.Create);
            app.CreatePerOwinContext<RoleManager>(RoleManager.Create);
            app.CreatePerOwinContext<SignInManager>(SignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie

            var auth = new CookieAuthenticationOptions
            {
                CookieName = "CRMLogin",
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<UserManager, User>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
                    OnApplyRedirect = ctx =>
                    {
                        if ( !IsAjaxRequest(ctx.Request))
                        {
                            // Patching by the way the absolute uri using http
                            // instead of https, when we are behind a lb
                            // terminating the https: returning only
                            // PathAndQuery
                            ctx.Response.Redirect(new Uri(ctx.RedirectUri).PathAndQuery);
                        }
                    }
                },
                ExpireTimeSpan = new TimeSpan(90, 0, 0, 0, 0),
                SlidingExpiration = true,

            };
            app.UseCookieAuthentication(auth);
            app.UseHangfireAspNet(GetHangfireServers);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });
            if (CRM.Application.Core.Services.AuthorizationService.IsSystemFeatureAvailable("", "SendSms"))
            {
                
                string monThurCron = WebConfigurationManager.AppSettings["SmsMonThuMin"] + " " +
                                     WebConfigurationManager.AppSettings["SmsMonThuHour"] + " " + "* * 1-4";
                string fridayCron = WebConfigurationManager.AppSettings["SmsFridayMin"] + " " +
                                    WebConfigurationManager.AppSettings["SmsFridayHour"] + " " + "* * 5";

                RecurringJob.AddOrUpdate<TimeregistrationController>(x => x.SendCheckoutReminder(), monThurCron, TimeZoneInfo.Local);

                RecurringJob.AddOrUpdate<TimeregistrationController>(x => x.SendCheckoutReminderFriday(), fridayCron, TimeZoneInfo.Local);
            }
        }

        public static CookieAuthenticationOptions GetCookieOptions()
        {
            var options = new CookieAuthenticationOptions
            {
                AuthenticationType =
                    DefaultAuthenticationTypes.ApplicationCookie,
                SlidingExpiration = true,
                // On ajax calls, better have a 401 rather than a redirect
                // to an HTML login page.
                // Taken from http://brockallen.com/2013/10/27/using-cookie-authentication-middleware-with-web-api-and-401-response-codes/
                Provider = new CookieAuthenticationProvider
                {
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request))
                        {
                            // Patching by the way the absolute uri using http
                            // instead of https, when we are behind a lb
                            // terminating the https: returning only
                            // PathAndQuery
                            ctx.Response.Redirect(new Uri(ctx.RedirectUri)
                                .PathAndQuery);
                        }
                    }
                }
            };

            return options;
        }

        // Taken from http://brockallen.com/2013/10/27/using-cookie-authentication-middleware-with-web-api-and-401-response-codes/
        private static bool IsAjaxRequest(IOwinRequest request)
        {
            var query = request.Query;
            if (query != null && StringComparer.OrdinalIgnoreCase.Equals(
                query["X-Requested-With"], "XMLHttpRequest"))
                return true;
            var headers = request.Headers;
            return headers != null && StringComparer.OrdinalIgnoreCase.Equals(
                headers["X-Requested-With"], "XMLHttpRequest");
        }


    }
    internal class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(context.GetOwinEnvironment());
            
            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return owinContext.Authentication.User.Identity.IsAuthenticated &&
                   HttpContext.Current.User.IsInRole("Admin");

        }
    }
}
