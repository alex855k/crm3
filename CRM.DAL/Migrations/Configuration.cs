using CRM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using static MoreLinq.Extensions.DistinctByExtension;



namespace CRM.DAL.Migrations
{
    public class Configuration : DbMigrationsConfiguration<CRMContext>
    {
        private readonly bool _pendingMigrations;
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;

            var migrator = new DbMigrator(this);
            _pendingMigrations = migrator.GetPendingMigrations().Any();
            if (_pendingMigrations)
            {
                migrator.Update();
                Seed(new CRMContext());
            }
        }
        protected override void Seed(CRMContext context)
        {
            context.OrdersStatuses.AddOrUpdate(
                c => c.Name,
                new OrderStatus { Name = "InProcessing" },
                new OrderStatus { Name = "InTransit" },
                new OrderStatus { Name = "Completed" },
                new OrderStatus { Name = "Withdrawn" },
                new OrderStatus { Name = "Returned" } //warranty etc
                );

            context.CustomerTypes.AddOrUpdate(
                c => c.Name,
                new CustomerType { Name = "PrivatePerson" },
                new CustomerType { Name = "Corporation" },
                new CustomerType { Name = "PublicCompany" }
                );

            context.ApplicationControllerCategories.AddOrUpdate(
               c => c.Id,
                 new ApplicationControllerCategory { Id = 1, Name = "Customers" },
                 new ApplicationControllerCategory { Id = 2, Name = "Calendar" },
                 new ApplicationControllerCategory { Id = 3, Name = "Budget" },
                 new ApplicationControllerCategory { Id = 4, Name = "Daily Report" },
                 new ApplicationControllerCategory { Id = 5, Name = "Dashboard" },
                 new ApplicationControllerCategory { Id = 6, Name = "CRM Features" },
                 new ApplicationControllerCategory { Id = 7, Name = "Company Profile" },
                 new ApplicationControllerCategory { Id = 8, Name = "Time Registration" },
                 new ApplicationControllerCategory { Id = 9, Name = "Cases" }
                 );



            context.CustomerNotesReports.AddOrUpdate(
                c => c.Name,
                new CustomerNoteReport { Name = "NotInReport" },
                new CustomerNoteReport { Name = "InReport" }
                );

            context.CustomerNotesVisitTypes.AddOrUpdate(
                c => c.Name,
                new CustomerNoteVisitType { Name = "NoType" },
                new CustomerNoteVisitType { Name = "Appointment" },
                new CustomerNoteVisitType { Name = "Canvas" },
                new CustomerNoteVisitType { Name = "Telephone" },
                new CustomerNoteVisitType { Name = "CallCenter" }
                );

            context.CustomerNotesDemos.AddOrUpdate(
                c => c.Name,
                new CustomerNoteDemo { Name = "NoDemonstration" },
                new CustomerNoteDemo { Name = "DemoCompleted" }
                );
            context.AppointmentEmailTypes.AddOrUpdate(
                c => c.Name,
                new AppointmentEmailType { Name = "Customer" },
                new AppointmentEmailType { Name = "SalesPerson" }
                );

            context.CustomerStatus.AddOrUpdate(
                c => c.Name,
                new CustomerStatus { Name = "Customer" },
                new CustomerStatus { Name = "Lead" }
                );

            var customerNotes = context.CustomerNotes
                .DistinctBy(x => x.CreationDate.Date + "." + x.UserId)
                .ToList();

            if (customerNotes.Any())
            {
                foreach (var note in customerNotes)
                {
                    var dailyReport = context.DailyReports
                        .Where(x => DbFunctions.TruncateTime(x.Date) == DbFunctions.TruncateTime(note.CreationDate) && x.UserId == note.UserId)
                        .SingleOrDefault();
                    if (dailyReport == null)
                    {
                        context.DailyReports.Add(new DailyReport { Date = note.CreationDate, UserId = note.UserId });
                    }
                }
            }

            context.CompanyProfile.AddOrUpdate(
               c => c.Id,
               new CompanyProfile { Id = 1, Name = "Company Name", Phone = "451234567890", Email = "email@company.com", Address = "Company Address Here....", URL = "www.CompanyURL.com", Logo = null }
               );

            context.ProductTypes.AddOrUpdate(
                c => c.Type,
                new ProductType { Type = "CoMaSys Produkt" }
            );
            //context.Products.AddOrUpdate(
            //    c => c.Name,

            //    new ComasysProduct { Name = "Office 365 Business", ProductType = context.ProductTypes.FirstOrDefault(), AdditionalData = "" }
            //);
            context.ReferenceTypes.AddOrUpdate(
                c => c.Name,
                new ReferenceType { Name = "Crayon" }
            );

            var rstore = new RoleStore<Role>(context);
            var rmanager = new RoleManager<Role>(rstore);
            Role admin;
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                admin = new Role { Name = "Admin", Description = "Admin Role For Administration Area" };
                rmanager.Create(admin);
            }
            else
                admin = rmanager.FindByName("Admin");



            if (!context.Users.Any(u => u.UserName == "comasysadmin"))
            {
                var ustore = new UserStore<User>(context);
                var umanager = new UserManager<User>(ustore);
                var user = new User
                {
                    FirstName = "Comasys Admin",
                    Email = "admin@comasys.dk",
                    UserName = "comasysadmin",
                    IsSuperAdmin = true,
                    IsEnabled = true

                };

                umanager.Create(user, "P@ssw0rd");
                umanager.AddToRole(user.Id, "Admin");
            }

            /**
             * Tester just for local
             **/
            if (!context.Roles.Any(r => r.Name == "Tester"))
            {
                var rolestore = new RoleStore<Role>(context);
                var rolemanager = new RoleManager<Role>(rolestore);
                var role = new Role
                {
                    Name = "Tester", Description ="just for testing"
                    
                };

                rolemanager.Create(role);
            }


            if (!context.Users.Any(u => u.UserName == "Tester"))
            {
                var ustore = new UserStore<User>(context);
                var umanager = new UserManager<User>(ustore);
                var user = new User
                {
                    FirstName = "Tester",
                    Email = "tester@comasys.dk",
                    UserName = "tester",
                    IsSuperAdmin = false,
                    IsEnabled = true

                };
                umanager.Create(user, "P@ssw0rd");
                umanager.AddToRole(user.Id, "Tester");
            }



            context.CustomerCaseTypes.AddOrUpdate(ct => ct.TypeName,
                new CustomerCaseType { TypeName = "Software Dev", Invoiced = true },
                new CustomerCaseType { TypeName = "IT", Invoiced = true }
            );



            //            var applicationControllers = new ApplicationController[]
            //{
            //                //[ControllerName],[ActionName],[DisplayName],[IsDisabled],[IsPartialPage],[ApplicationControllerCategoryId]
            //                new ApplicationController("Customers",         "Index", "Customers Main Page", false, false, 1),
            //                new ApplicationController("Customers",         "Create", "Create Customer", false, false, 1),
            //                new ApplicationController("Customers",         "Edit", "Edit Customer", false, false, 1),
            //                new ApplicationController("Customers",         "CustomerContacts", "Customers Contacts", false, true, 1),
            //                new ApplicationController("Customers",         "CustomerNotes", "Customers Notes", false, true, 1),
            //                new ApplicationController("Customers",         "CustomerAppointments", "Customers Appointments", false, true, 1),
            //                new ApplicationController("Calendar",          "Index", "Calendar Main Page", false, false, 2),
            //                new ApplicationController("Budget",            "Index", "Create Budget for Salesman", false, false, 3),
            //                new ApplicationController("DailyReport",       "Index", "Daily Report", false, false, 4),
            //                new ApplicationController("Customers",        "CustomerOrders", "Customers Orders", false, true, 1),
            //                new ApplicationController("DailyReport",      "DailyReportUserSelection", "Daily Report User Selection", false, false, 4),
            //                new ApplicationController("Customers",        "SetCustomerInactive", "Delete Customer", false, false, 1),
            //                new ApplicationController("UploadCustomers",  "Index", "Upload Customers", false, false, 1),
            //                new ApplicationController("DashboardLists",   "Index", "Dashboard Lists", false, true, 5),
            //                new ApplicationController("Customers",        "CustomerTableFilterComparison", "Customers Main Table Filter Comparison Operators", false, false, 1),
            //                new ApplicationController("CRMFeatures",      "TopMenuNotification", "Top Menu Notification Center", false, false, 6),
            //                new ApplicationController("CRMFeatures",      "TopMenuSearch", "Top Menu Search", false, false, 6),
            //                new ApplicationController("CRMFeatures",      "TopMenuLanguage", "Top Menu Language", false, false, 6),
            //                new ApplicationController("CompanyProfile",   "Index", "Company Profile", false, false, 7),
            //                new ApplicationController("Orders",           "CustomerOrderHistory", "Orders", false, true, 1),
            //                new ApplicationController("EmailMessages",    "CustomerEmailCorrespondenceIndex", "Email Messages", false, true, 1),
            //                new ApplicationController("EmailAccounts",    "Index", "Email Account Management", false, true, 1),
            //                new ApplicationController("Orders",           "Create", "Create New Order", false, true, 1),
            //                new ApplicationController("Customers",        "CustomerNoteOptions", "Customer Notes {Report,type,demo} options", false, false, 1),
            //                new ApplicationController("Customers",        "CustomerCase", "Customer Case", false, true, 1),
            //                new ApplicationController("Customers",        "CustomerTableAndOr", "Customers Table AndOr", false, false, 1),
            //                new ApplicationController("",                 "", "Load Dashboard Customers by User", false, false, 6),
            //                new ApplicationController("CustomerCases",    "Index", "Cases", false, false, 1),
            //                new ApplicationController("TimeRegistration", "Home", "Time Registration Widget", false, false, 8),
            //                new ApplicationController("", "SendSms",      "Sms Send end of day sms", false, false, 8),
            //                new ApplicationController("CustomerCases",    "", "CustomerCases AllAccess", false, false, 9),
            //	            //new ApplicationController(33, "CustomerCases", "CasesForWeek", "For a particluar week get all cases", false, false, 9)
            //	            //new ApplicationController(34, "CustomerCases", "CurrentWeek", "For the current week get all cases", false, false, 9)
            //	            new ApplicationController("TimeRegistration", "TimeRegsForWeek", "For the current week get summarized time-registrations for every user", false, false, 9)

            //};
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string searchDir = AppDomain.CurrentDomain.RelativeSearchPath;
            var folder = Path.Combine(baseDir, searchDir ?? "");
            var filepath = Path.Combine(folder, @"Migrations\ApplicationPagesMigration.sql");
            var apppagesMigration = File.ReadAllText(filepath);
            context.Database.ExecuteSqlCommand(apppagesMigration);

            
            //grant all the access to admin
            

            if(!context.ControllerRoles.Any())
            {
                var apIds = context.ApplicationControllers.Select(x => x.Id).ToArray();
                var adminRoles = apIds
                    .Select(apid => new ControllerRole
                    {
                        RoleId = admin.Id,
                        ControllerId = apid
                    })
                    .ToArray();


                context.ControllerRoles.AddRange(adminRoles);
                context.SaveChanges();
            }
            base.Seed(context);
        }
    }
}
