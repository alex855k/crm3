using CRM.DAL.Repositories;
using CRM.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CRM.DAL
{
    public class UnitofWork
    {
        public CRMContext DbContext;
        public CustomersRepo<Customer> CustomersRepo { get; set; }
        public CustomerTypesRepo<CustomerType> CustomerTypesRepo { get; set; }
        public ApplicationControllersRepo<ApplicationController> ApplicationControllersRepo { get; set; }
        public ControllerRolesRepo<ControllerRole> ControllerRolesRepo { get; set; }
        public CustomerContactsRepo<CustomerContact> CustomerContactsRepo { get; set; }
        public CustomerNotesRepo<CustomerNote> CustomerNotesRepo { get; set; }
        public DashboardListsRepo<DashboardList> DashboardListsRepo { get; set; }
        public DashboardListColumnsRepo<DashboardListColumn> DashboardListColumnsRepo { get; set; }
        public UserDashboardListsRepo<UserDashboardList> UserDashboardListsRepo { get; set; }
        public CustomerDashboardListsRepo<CustomerDashboardList> CustomerDashboardListsRepo { get; set; }
        public CustomerAppointmentsRepo<CustomerAppointment> CustomerAppointmentsRepo { get; set; }
        public BudgetRepo<Budget> BudgetRepo { get; set; }
        public ApplicationControllerCategoriesRepo<ApplicationControllerCategory> ApplicationControllerCategoriesRepo { get; set; }

        public ReferenceTypeRepo<ReferenceType> ReferenceTypeRepo { get; set; }
        public ReferencelistRepo<Referencelist> ReferencelistRepo { get; set; }
        public CustomerSubscriptionRepo<CustomerSubscription> CustomerSubscriptionRepo { get; set; }
        

        public ProcedureRepo<Procedure> ProcedureRepo { get; set; }
        public ProcedureItemsRepo<ProcedureItem> ProcedureItemsRepo { get; set; }
        public AppointmentEmailsRepo<AppointmentEmail> AppointmentEmailsRepo { get; set; }
        public DailyReportRepo<DailyReport> DailyReportsRepo { get; set; }
        public CompanyProfileRepo<CompanyProfile> CompanyProfileRepo { get; set; }

        public OrdersRepo<Order> OrdersRepo { get; set; }
        public OrderStatusRepo<OrderStatus> OrderStatusRepo { get; set; }
        public OrderItemRepo<OrderItem> OrderItemRepo { get; set; }

        public ComasysProductRepo<ComasysProduct> ComasysProductsRepo { get; set; }
        public ProductTypeRepo<ProductType> ProductsTypeRepo { get; set; }
        public ProductRepo<Product> ProductRepo { get; set; }

        public EmailAccountsRepo<EmailAccount> EmailAccountsRepo { get; set; }
        public EmailMessagesRepo<EmailMessage> EmailMessagesRepo { get; set; }
        public EmailProtocolsRepo<EmailProtocol> EmailProtocolsRepo { get; set; }

        public CustomerCaseRepo<CustomerCase> CustomerCaseRepo { get; set; }
        public CaseAssignmentRepo<CaseAssignment> CaseAssignmentRepo { get; set; }
        public CustomerCaseTypeRepo<CustomerCaseType> CustomerCaseTypeRepo { get; set; }
        public TimeRegistrationRepo<TimeRegistration> TimeRegistrationRepo { get; set; }
        public DrivingRegistrationRepo<DrivingRegistration> DrivingRegistrationRepo { get; set; }
        public UnitofWork()
        {
            DbContext = CRMContext.Create();
            CustomersRepo = new CustomersRepo<Customer>(DbContext);
            CustomerTypesRepo = new CustomerTypesRepo<CustomerType>(DbContext);
            ApplicationControllersRepo = new ApplicationControllersRepo<ApplicationController>(DbContext);
            ControllerRolesRepo = new ControllerRolesRepo<ControllerRole>(DbContext);
            CustomerContactsRepo = new CustomerContactsRepo<CustomerContact>(DbContext);
            CustomerNotesRepo = new CustomerNotesRepo<CustomerNote>(DbContext);
            DashboardListsRepo = new DashboardListsRepo<DashboardList>(DbContext);
            DashboardListColumnsRepo = new DashboardListColumnsRepo<DashboardListColumn>(DbContext);
            UserDashboardListsRepo = new UserDashboardListsRepo<UserDashboardList>(DbContext);
            CustomerDashboardListsRepo = new CustomerDashboardListsRepo<CustomerDashboardList>(DbContext);
            CustomerAppointmentsRepo = new CustomerAppointmentsRepo<CustomerAppointment>(DbContext);
            BudgetRepo = new BudgetRepo<Budget>(DbContext);
            ApplicationControllerCategoriesRepo = new ApplicationControllerCategoriesRepo<ApplicationControllerCategory>(DbContext);
            ReferenceTypeRepo = new ReferenceTypeRepo<ReferenceType>(DbContext);
            ReferencelistRepo = new ReferencelistRepo<Referencelist>(DbContext);
            CustomerSubscriptionRepo = new CustomerSubscriptionRepo<CustomerSubscription>(DbContext);
            ProcedureRepo = new ProcedureRepo<Procedure>(DbContext);
            ProcedureItemsRepo = new ProcedureItemsRepo<ProcedureItem>(DbContext);
            DailyReportsRepo = new DailyReportRepo<DailyReport>(DbContext);
            CompanyProfileRepo = new CompanyProfileRepo<CompanyProfile>(DbContext);
            AppointmentEmailsRepo = new AppointmentEmailsRepo<AppointmentEmail>(DbContext);

            OrdersRepo = new OrdersRepo<Order>(DbContext);
            OrderStatusRepo = new OrderStatusRepo<OrderStatus>(DbContext);
            OrderItemRepo = new OrderItemRepo<OrderItem>(DbContext);

            ProductRepo = new ProductRepo<Product>(DbContext);
            ComasysProductsRepo = new ComasysProductRepo<ComasysProduct>(DbContext);
            ProductsTypeRepo = new ProductTypeRepo<ProductType>(DbContext);

            EmailAccountsRepo = new EmailAccountsRepo<EmailAccount>(DbContext);
            EmailMessagesRepo = new EmailMessagesRepo<EmailMessage>(DbContext);
            EmailProtocolsRepo = new EmailProtocolsRepo<EmailProtocol>(DbContext);

            CustomerCaseRepo = new CustomerCaseRepo<CustomerCase>(DbContext);
            CaseAssignmentRepo = new CaseAssignmentRepo<CaseAssignment>(DbContext);
            CustomerCaseTypeRepo = new CustomerCaseTypeRepo<CustomerCaseType>(DbContext);
            TimeRegistrationRepo = new TimeRegistrationRepo<TimeRegistration>(DbContext);
            CustomerCaseRepo = new CustomerCaseRepo<CustomerCase>(DbContext);
            CaseAssignmentRepo = new CaseAssignmentRepo<CaseAssignment>(DbContext);
            CustomerCaseTypeRepo = new CustomerCaseTypeRepo<CustomerCaseType>(DbContext);
            TimeRegistrationRepo = new TimeRegistrationRepo<TimeRegistration>(DbContext);
            DrivingRegistrationRepo = new DrivingRegistrationRepo<DrivingRegistration>(DbContext);
        }
        public async Task SaveChangesAsync()
        {
            await DbContext.SaveChangesAsync();
           
        }
        public void SaveChanges()
        {

            try
            {
                DbContext.SaveChanges();
            }
            catch(InvalidOperationException e)
            {
                Debug.WriteLine(e.Message + "\n" + e.InnerException);
                throw;
            }
            catch (DbEntityValidationException e)
            {
                DbEntityValidationException ed = e;
                foreach (var eve in e.EntityValidationErrors)
                {
                    Debug.WriteLine(@"Entity of type ""{0}"" in state ""{1}"" 
                   has the following validation errors:",
                        eve.Entry.Entity.GetType().Name,
                        eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Debug.WriteLine(@"- Property: ""{0}"", Error: ""{1}""",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (DbUpdateException e)
            {
                DbUpdateException em = e;
                Debug.WriteLine(e.Message + "\n" + e.InnerException);
                //Add your code to inspect the inner exception and/or
                //e.Entries here.
                //Or just use the debugger.
                //Added this catch (after the comments below) to make it more obvious 
                //how this code might help this specific problem
                throw;
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message + "\n" + e.InnerException);
                throw;
            }
        }
    }

}
