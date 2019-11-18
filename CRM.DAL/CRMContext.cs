using CRM.DAL.Migrations;
using CRM.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace CRM.DAL
{
    public class CRMContext : IdentityDbContext<User>
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<ApplicationController> ApplicationControllers { get; set; }
        public DbSet<ControllerRole> ControllerRoles { get; set; }
        public DbSet<CustomerContact> CustomerContacts { get; set; }
        public DbSet<CustomerNote> CustomerNotes { get; set; }
        public DbSet<CustomerNoteReport> CustomerNotesReports { get; set; }
        public DbSet<CustomerNoteVisitType> CustomerNotesVisitTypes { get; set; }
        public DbSet<CustomerNoteDemo> CustomerNotesDemos { get; set; }
        public DbSet<DashboardList> DashboardLists { get; set; }
        public DbSet<DashboardListColumn> DashboardListColumns { get; set; }
        public DbSet<UserDashboardList> UserDashboardLists { get; set; }
        public DbSet<CustomerDashboardList> CustomerDashboardLists { get; set; }
        public DbSet<CustomerAppointment> CustomerAppointments { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<ApplicationControllerCategory> ApplicationControllerCategories { get; set; }
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<EmailProtocol> EmailProtocols { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ComasysOrder> CustomerOrders { get; set; }
        public DbSet<OrderItem> OrderedProducts { get; set; }
        public DbSet<ComasysProduct> ComasysProducts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Referencelist> Referencelists { get; set; }
        public DbSet<ReferenceType> ReferenceTypes { get; set; }

        //public DbSet<ComasysSubscription> CustomerSubscriptions { get; set; }
        public DbSet<OrderStatus> OrdersStatuses { get; set; }
        public DbSet<Procedure> Procedures { get; set; }
        public DbSet<ProcedureItem> ProcedureItems { get; set; }
        public DbSet<AppointmentEmail> AppointmentEmails { get; set; }
        public DbSet<AppointmentEmailType> AppointmentEmailTypes { get; set; }
        public DbSet<CustomerStatus> CustomerStatus { get; set; }
        public DbSet<CustomerCase> CustomerCases { get; set; }
        public DbSet<CaseAssignment> CaseAssignments { get; set; }
        public DbSet<CustomerCaseType> CustomerCaseTypes { get; set; }
        public DbSet<TimeRegistration> TimeRegistrations { get; set; }
        public DbSet<DrivingRegistration> DrivingRegistrations { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<CompanyProfile> CompanyProfile { get; set; }
        public CRMContext() : base("CRMContext")
        {

        }
        public static CRMContext Create()
        {
            return new CRMContext();
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
