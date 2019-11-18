using CRM.Application.Core.Enums;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using X.PagedList;
using CRM.Application.Core.Services;
using AutoMapper;
using CRM.Web.Helpers;
using CRM.Web.App_Helpers;
using Crayon.Api.Sdk;
using System.Net.Mail;
using System.Web.Configuration;
using System.Linq.Expressions;
using Newtonsoft.Json;
using NLog;
using CRM.Web;
using CacheManager = CRM.Web.CacheManager;

namespace SmartAdminMvc.Controllers
{
    public class CustomersController : Controller
    {
        public string CompanyMailAddress => WebConfigurationManager.AppSettings["CompanyMail:EmailAddress"].ToString();
        public string CompanyMailPassword => WebConfigurationManager.AppSettings["CompanyMail:EmailPasword"].ToString();
        public string CompanyMailServer => WebConfigurationManager.AppSettings["CompanyMail:EmailServer"].ToString();
        public int CompanyMailPort => int.Parse(WebConfigurationManager.AppSettings["CompanyMail:EmailPort"].ToString());

        private UserManager _userManager;
        private RoleManager _roleManager;
        private CustomerViewModel _customerViewModel = new CustomerViewModel();
        private UnitofWork _uow = new UnitofWork();
        CustomerNotesViewModel _customerNotesViewModel = new CustomerNotesViewModel();
        const int maxFileSizeInBytes = 104857600;
        DailyReportService dailyReportService = new DailyReportService();
        public CustomersController()
        {
        }
        public CustomersController(UserManager userManager, RoleManager roleManager)
        {
            UserManager = userManager;
            _roleManager = roleManager;
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public RoleManager RoleManager
        {
            get
            {
                return _roleManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<RoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        AppointmentService _appointmentService = new AppointmentService();
        public Logger logger = LogManager.GetCurrentClassLogger();
        TableCache _tableCache = new TableCache();

        // GET: Customers

        [CRMAuthorize]
        public ActionResult Index(bool isCustomers)
        {
            var tableCache = GetCustomerTableCache(isCustomers);
            int pageNumber = 0;
            if (tableCache != null && tableCache.PageNumber != 0)
            {
                pageNumber = tableCache.PageNumber;
                _customerViewModel.LastEditedCustomer = tableCache.LastEditedCustomer;
            }
            else
            {
                pageNumber = _customerViewModel.PageNumber;
            }

            _customerViewModel.DefaultOrderBy = "CompanyName";
            //if (customerLeadTableFilterParametersCookie != null &&
            //    customerLeadTableFilterParametersCookie.Values["parameters"] != null
            //    && !string.IsNullOrEmpty(customerLeadTableFilterParametersCookie.Values["parameters"].ToString()))
            //{

            //    var paramsString = Base64Decode(customerLeadTableFilterParametersCookie.Values["parameters"]);
            //    parameters = JsonConvert.DeserializeObject<List<ExpressionBuilderParameters>>(paramsString);
            //}

            TableFilterSortPagingService<Customer, CustomerViewModel> tableFilter = new TableFilterSortPagingService<Customer, CustomerViewModel>();
            GenericViewModel<CustomerViewModel, Customer> genericViewModel = new GenericViewModel<CustomerViewModel, Customer>();
            if (tableCache != null && tableCache.SearchParameters.Count > 0)
            {
                genericViewModel.QueryParameters = tableCache.SearchParameters;
                foreach (var parameter in tableCache.SearchParameters)
                {
                    if (parameter.SearchKey.ToLower() == "CompanyName".ToLower())
                        _customerViewModel.CompanyName = parameter.Value;
                    if (parameter.SearchKey.ToLower() == "Phone".ToLower())
                        _customerViewModel.Phone = parameter.Value;
                    if (parameter.SearchKey.ToLower() == "Address".ToLower())
                        _customerViewModel.Address = parameter.Value;
                    if (parameter.SearchKey.ToLower() == "PostalCode".ToLower())
                        _customerViewModel.PostalCode = parameter.Value;
                    if (parameter.SearchKey.ToLower() == "Town".ToLower())
                        _customerViewModel.Town = parameter.Value;
                }
            }
            genericViewModel.PageNumber = pageNumber;
            genericViewModel.PageSize = _customerViewModel.PageSize;
            genericViewModel.DefaultOrderBy = _customerViewModel.DefaultOrderBy;
            genericViewModel.DefaultDirection = _customerViewModel.DefaultDirection;
            genericViewModel.QueryOperatorComparer = _customerViewModel.QueryOperatorComparer;
            genericViewModel.SpecialConditionQueryOperatorComparer = ExpressionType.And.ToString();
            if (isCustomers)
            {
                genericViewModel.QueryParameters.Add(new ExpressionBuilderParameters { SearchKey = "CustomerStatusId", Operator = "Equals", Value = "1",HasSpecialConditionQueryOperatorComparer=true });
                _customerViewModel.CustomerStatusId = 1;
            }
            else
            {
                genericViewModel.QueryParameters.Add(new ExpressionBuilderParameters { SearchKey = "CustomerStatusId", Operator = "Equals", Value = "2", HasSpecialConditionQueryOperatorComparer = true });
                _customerViewModel.CustomerStatusId = 2;
            }

            genericViewModel.QueryParameters.Add(new ExpressionBuilderParameters { SearchKey = "IsActive", Operator = "Equals", Value = "true", HasSpecialConditionQueryOperatorComparer = true });
            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _customerViewModel);

            var customersIPagedList = new StaticPagedList<Customer>(
                tableFilterResult.ResultList,
                pageNumber,
                _customerViewModel.PageSize,
                tableFilterResult.QueryCount);


            _customerViewModel.CustomersList = customersIPagedList;
            _customerViewModel.TableCount = tableFilterResult.TableCount;
            _customerViewModel.QueryCount = tableFilterResult.QueryCount;
            _customerViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();
            _customerViewModel.IsCustomers = isCustomers;

            SaveCustomerTableCache(isCustomers, tableCache, genericViewModel.QueryParameters, tableFilterResult.PageNumber);
            return View(_customerViewModel);
        }

        private TableCache GetCustomerTableCache(bool isCustomer)
        {
            TableCache tableCache = null;
            if (isCustomer)
                tableCache = CacheManager.GetEntry<TableCache>(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()));
            else
                tableCache = CacheManager.GetEntry<TableCache>(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.LeadsTable, User.Identity.GetUserId()));
            return tableCache;
        }
        private void SaveCustomerTableCache(bool isCustomer, TableCache tableCache, List<ExpressionBuilderParameters> queryParameters, int pageNumber)
        {
            if (tableCache != null)
            {
                tableCache.SearchParameters.Clear();
                if (queryParameters != null && queryParameters.Count > 0)
                {
                    tableCache.SearchParameters = queryParameters.Where(x => x.SearchKey != "IsActive" && x.SearchKey != "CustomerStatusId").ToList();
                    tableCache.PageNumber = pageNumber;
                }
                else
                {
                    tableCache.PageNumber = _customerViewModel.PageNumber;
                }
                if (isCustomer)
                    CacheManager.AddEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()), tableCache);
                else
                    CacheManager.AddEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.LeadsTable, User.Identity.GetUserId()), tableCache);
            }
            else
            {

                if (queryParameters != null && queryParameters.Count > 0)
                    _tableCache.SearchParameters = queryParameters.Where(x => x.SearchKey != "IsActive" && x.SearchKey != "CustomerStatusId").ToList();
                if (isCustomer)
                    CacheManager.AddEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()), _tableCache);
                else
                    CacheManager.AddEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.LeadsTable, User.Identity.GetUserId()), _tableCache);
            }
        }

       public ActionResult RefreshTable(bool isCustomers)
        {
            if (isCustomers)
                CacheManager.RemoveEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()));
            else
                CacheManager.RemoveEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.LeadsTable, User.Identity.GetUserId()));

            return new EmptyResult();
        }

        [CRMAuthorize]
        public ActionResult Create()
        {
            _customerViewModel.CustomerTypesList.AddRange(GetCustomerTypes());
            _customerViewModel.SalesList.AddRange(GetSalesUsers());
            _customerViewModel.CustomerStatusList.AddRange(GetCustomerStatus());
            _customerViewModel.CustomerStatusId = 1;
            return View(_customerViewModel);
        }

        private IEnumerable<CustomerStatus> GetCustomerStatus()
        {
            var CustomerStatus = new List<CustomerStatus>
            {
                new CustomerStatus {Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.TypeCustomer},
                new CustomerStatus { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.TypeLead },
            };

            return CustomerStatus;
        }

        [HttpPost]
        public ActionResult Create(CustomerViewModel customerViewModel)
        {
            Customer savedCustomer = null;
            //if (ModelState.IsValid)

            //{
            if (customerViewModel.Id == 0)
            {
                savedCustomer = CreateCustomer(customerViewModel);
                ReponseViewModel.ResponseMessage = CRM.Application.Core.Resources.Customers.Customer.CustomerSaved;
                ReponseViewModel.TransactionType = TransactionType.Create.ToString();
            }
            else
            {
                savedCustomer = UpdateCustomer(customerViewModel);
                ReponseViewModel.ResponseMessage = CRM.Application.Core.Resources.Customers.Customer.CustomerUpdated;
                ReponseViewModel.TransactionType = TransactionType.Update.ToString();
            }
            try
            {
                _uow.SaveChanges();
                return Json(new { savedCustomerId = savedCustomer.Id, response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
            }

            catch (DbEntityValidationException e)

            {
                throw;
            }
            //}
            //return View();
        }
        private Customer CreateCustomer(CustomerViewModel customerViewModel)
        {
            Customer customer = Mapper.Map<Customer>(customerViewModel);
            _uow.CustomersRepo.Add(customer);
            return customer;
        }
        public Customer UpdateCustomer(CustomerViewModel customerViewModel)
        {
            Customer customer = Mapper.Map<Customer>(customerViewModel);
            _uow.CustomersRepo.Update(customer);
            return customer;

        }
        [CRMAuthorize]
        public ActionResult Edit(int id, int latestCustomersPageNumber, bool? navigateToNotes, bool isCustomer)
        {
            var customer = _uow.CustomersRepo.Find(id);
            _customerViewModel = Mapper.Map<CustomerViewModel>(customer);
            _customerViewModel.CustomerTypesList.AddRange(GetCustomerTypes());
            _customerViewModel.SalesList.AddRange(GetSalesUsers());
            _customerViewModel.CustomerStatusList.AddRange(GetCustomerStatus());
            _customerViewModel.NavigateToNotes = navigateToNotes == null ? false : navigateToNotes.Value;
            //SetCustomersTableCurrentPageCurrentCustomer(id, null, isCustomer);
            var cacheObj = CacheManager.GetEntry<TableCache>(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()));
            if (cacheObj != null)
            {
                cacheObj.LastEditedCustomer = id;
                CacheManager.AddEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()), cacheObj);
            }
            else
            {
                _tableCache.LastEditedCustomer = id;
                CacheManager.AddEntry(CacheKeyManager.GetCustomersTableInfo(CacheKeyManager.CustomersTable, User.Identity.GetUserId()), _tableCache);
            }
            return View("Create", _customerViewModel);
        }
        private void SetCustomersTableCurrentPageCurrentCustomer(int? customerId, int? currentPageNumber, bool isCustomer)
        {
            if (isCustomer)
            {
                CreateCustomerTableCookie(customerId, currentPageNumber);
            }
            else
            {
                CreateLeadsTableCookie(customerId, currentPageNumber);
            }

        }
        private void CreateCustomerTableCookie(int? customerId, int? currentPageNumber)
        {
            HttpCookie customersTableCookie = Request.Cookies["CustomersTable"];
            if (customersTableCookie == null)
            {
                HttpCookie cookie = new HttpCookie("CustomersTable");
                DateTime now = DateTime.Now;
                cookie.Expires = now.AddYears(50);
                cookie.Values.Add("customerId", customerId?.ToString() ?? null);
                if (currentPageNumber != null)
                    cookie.Values.Add("pageNumber", currentPageNumber.ToString());
                Response.Cookies.Add(cookie);
            }
            else
            {
                if (customerId != null)
                    customersTableCookie.Values["customerId"] = customerId.ToString();
                customersTableCookie.Values["pageNumber"] = currentPageNumber?.ToString() ?? null;
                Response.Cookies.Add(customersTableCookie);
            }
        }
        private void CreateLeadsTableCookie(int? customerId, int? currentPageNumber)
        {
            HttpCookie customersTableCookie = Request.Cookies["LeadsTable"];
            if (customersTableCookie == null)
            {
                HttpCookie cookie = new HttpCookie("LeadsTable");
                DateTime now = DateTime.Now;
                cookie.Expires = now.AddYears(50);
                if (customerId != null)
                    cookie.Values.Add("customerId", customerId.Value.ToString());
                if (currentPageNumber != null)
                    cookie.Values.Add("pageNumber", currentPageNumber.ToString());
                Response.Cookies.Add(cookie);
            }
            else
            {
                customersTableCookie.Values["customerId"] = customerId?.ToString() ?? null;
                customersTableCookie.Values["pageNumber"] = currentPageNumber?.ToString() ?? null;
                Response.Cookies.Add(customersTableCookie);
            }
        }

        /////////////
        private void SetCustomersTableFilterParameters(List<ExpressionBuilderParameters> queryParameters, bool isCustomer)
        {
            if (isCustomer)
            {
                CreateCustomerTableFilterParameters(queryParameters, isCustomer);
            }
            else
            {
                CreateLeadsTableFilterParameters(queryParameters, isCustomer);
            }

        }
        private void CreateCustomerTableFilterParameters(List<ExpressionBuilderParameters> queryParameters, bool isCustomer)
        {
            HttpCookie customersTableFilterParametersCookie = Request.Cookies["customersTableFilterParameters"];
            if (customersTableFilterParametersCookie == null)
            {
                HttpCookie cookie = new HttpCookie("customersTableFilterParameters");
                DateTime now = DateTime.Now;
                cookie.Expires = now.AddYears(50);
                cookie.Values.Add("parameters", Base64Encode(JsonConvert.SerializeObject(queryParameters, Formatting.None)));
                Response.Cookies.Add(cookie);
            }
            else
            {
                customersTableFilterParametersCookie.Values["parameters"] = null;
                if (queryParameters != null && queryParameters.Count > 0)
                    customersTableFilterParametersCookie.Values["parameters"] = Base64Encode(JsonConvert.SerializeObject(queryParameters, Formatting.None));
                Response.Cookies.Add(customersTableFilterParametersCookie);
            }
        }
        private void CreateLeadsTableFilterParameters(List<ExpressionBuilderParameters> queryParameters, bool isCustomer)
        {
            HttpCookie leadsTableFilterParametersCookie = Request.Cookies["leadsTableFilterParameters"];
            if (leadsTableFilterParametersCookie == null)
            {
                HttpCookie cookie = new HttpCookie("leadsTableFilterParameters");
                DateTime now = DateTime.Now;
                cookie.Expires = now.AddYears(50);
                cookie.Values.Add("parameters", Base64Encode(JsonConvert.SerializeObject(queryParameters, Formatting.None)));
                Response.Cookies.Add(cookie);
            }
            else
            {

                leadsTableFilterParametersCookie.Values["parameters"] = null;
                if (queryParameters != null && queryParameters.Count > 0)
                    leadsTableFilterParametersCookie.Values["parameters"] = Base64Encode(JsonConvert.SerializeObject(queryParameters, Formatting.None));
                Response.Cookies.Add(leadsTableFilterParametersCookie);
            }

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        ////////////
        [CRMAuthorize]
        public ActionResult CustomerContacts(int customerId)
        {
            CustomerContactsViewModel _customerContactsViewModel = new CustomerContactsViewModel();
            _customerContactsViewModel.CustomerId = customerId;

            return View("_CustomerContacts", _customerContactsViewModel);
        }
        public ActionResult CustomerContactsForm(int customerId)
        {
            CustomerContactsViewModel _customerContactsViewModel = new CustomerContactsViewModel();
            _customerContactsViewModel.CustomerId = customerId;
            return View("_CustomerContactsForm", _customerContactsViewModel);
        }
        [HttpPost]
        public ActionResult CustomerContacts(CustomerContactsViewModel customerContactsViewModel)
        {
            if (customerContactsViewModel.Id == 0)
            {
                SaveCustomerContact(customerContactsViewModel);
            }
            else
            {
                UpdateCustomerContact(customerContactsViewModel);
            }

            try
            {
                _uow.SaveChanges();
                CustomerContactsViewModel _customerContactsViewModel = new CustomerContactsViewModel();
                _customerContactsViewModel.CustomerContactsList = _uow.CustomerContactsRepo.Search(x => x.CustomerId == customerContactsViewModel.CustomerId).ToList();
                _customerContactsViewModel.CustomerId = customerContactsViewModel.CustomerId;
                return View("_CustomerContactList", _customerContactsViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void SaveCustomerContact(CustomerContactsViewModel customerContactsViewModel)
        {
            CustomerContact customerContact = Mapper.Map<CustomerContact>(customerContactsViewModel);
            _uow.CustomerContactsRepo.Add(customerContact);
        }
        private void UpdateCustomerContact(CustomerContactsViewModel customerContactsViewModel)
        {
            CustomerContact customerContact = Mapper.Map<CustomerContact>(customerContactsViewModel);
            _uow.CustomerContactsRepo.Update(customerContact);
        }
        public ActionResult CustomerContactsList(int customerId)
        {
            CustomerContactsViewModel _customerContactsViewModel = new CustomerContactsViewModel();
            _customerContactsViewModel.CustomerContactsList = _uow.CustomerContactsRepo.Search(x => x.CustomerId == customerId).ToList();
            return View("_CustomerContactList", _customerContactsViewModel);
        }
        private List<CustomerType> GetCustomerTypes()
        {
            var customerTypes = new List<CustomerType>
            {
                new CustomerType{Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.PrivatePerson},
                new CustomerType { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.Corporation },
                new CustomerType { Id = 3, Name = CRM.Application.Core.Resources.Customers.Customer.PublicCompany }
            };

            return customerTypes;
        }
        private List<UserViewModel> GetSalesUsers()
        {
            var users = UserManager.Users.Where(x => x.IsSuperAdmin == false);
            List<UserViewModel> userViewModel = new List<UserViewModel>();
            userViewModel.Add(new UserViewModel { Id = Guid.Empty, FirstName = CRM.Application.Core.Resources.Customers.Customer.NoVipSeller.ToString() });
            List<UserViewModel> userViewModelList = Mapper.Map<List<UserViewModel>>(users);
            userViewModel.AddRange(userViewModelList);
            return userViewModel;
        }

        private List<CustomerNoteReport> GetCustomerNoteReport()
        {
            var customerNoteReport = new List<CustomerNoteReport>
            {
                new CustomerNoteReport{Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.NotInReport},
                new CustomerNoteReport { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.InReport },
            };
            return customerNoteReport;
        }
        private List<CustomerNoteVisitType> GetCustomerNoteVisitType()
        {
            var customerNoteVisitType = new List<CustomerNoteVisitType>
            {
                new CustomerNoteVisitType{Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.NoType},
                new CustomerNoteVisitType { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.Appointment },
                new CustomerNoteVisitType { Id = 3, Name = CRM.Application.Core.Resources.Customers.Customer.Canvas },
                new CustomerNoteVisitType { Id = 4, Name = CRM.Application.Core.Resources.Customers.Customer.Telephone},
                new CustomerNoteVisitType { Id = 5, Name = CRM.Application.Core.Resources.Customers.Customer.CallCenter },
            };
            return customerNoteVisitType;
        }
        private List<CustomerNoteDemo> GetCustomerNoteDemo()
        {
            var customerNoteDemo = new List<CustomerNoteDemo>
            {
                new CustomerNoteDemo{Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.NoDemonstration},
                new CustomerNoteDemo { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.DemoCompleted },
            };
            return customerNoteDemo;
        }
        [CRMAuthorize]
        [HttpGet]
        public ActionResult CustomerNotes(int customerId)
        {
            _customerNotesViewModel.CustomerId = customerId;
            return View("_CustomerNotes", _customerNotesViewModel);
        }
        public ActionResult CustomerNotesForm(int customerId)
        {
            _customerNotesViewModel.CustomerId = customerId;
            _customerNotesViewModel.CustomerNotesReportList.AddRange(GetCustomerNoteReport());
            _customerNotesViewModel.CustomerNotesVisitTypeList.AddRange(GetCustomerNoteVisitType());
            _customerNotesViewModel.CustomerNotesDemoList.AddRange(GetCustomerNoteDemo());
            return View("_CustomerNotesForm", _customerNotesViewModel);
        }
        public ActionResult CustomerNotesList(int customerId)
        {
            _customerNotesViewModel.CustomerNotesList = _uow.CustomerNotesRepo.SearchInclude(x => x.CustomerId == customerId).OrderByDescending(x => x.CreationDate).ToList();
            return View("_CustomerNotesList", _customerNotesViewModel);
        }
        [HttpPost]
        public ActionResult CustomerNotes(CustomerNotesViewModel customerNotesViewModel)
        {
            var files = Request.Files.GetMultiple("customerNoteAttachment");
            if (files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var fileExtension = Path.GetExtension(files[i].FileName).Substring(1);
                    if (!CRM.Application.Core.Constants.Constants.FileSupportedTypes.Contains(fileExtension.ToLower()))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Customers.Customer.SupportedFileTypes.ToString());
                    }
                    else if (files[i].ContentLength > maxFileSizeInBytes)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Customers.Customer.FileMaxSize.ToString());
                    }
                }
            }

            var savedCustomerNote = new CustomerNote();
            if (customerNotesViewModel.Id == 0)
                savedCustomerNote = SaveCustomerNote(customerNotesViewModel);
            else
                UpdateCustomerNote(customerNotesViewModel);
            try
            {
                _uow.SaveChanges();
                dailyReportService.CreateDailyReport();
                if (customerNotesViewModel.Id != 0 && !string.IsNullOrEmpty(customerNotesViewModel.RemovedAttachments.First().ToString()))
                {
                    var customer = _uow.CustomersRepo.Find(customerNotesViewModel.CustomerId);
                    string attachmentsPath = Server.MapPath("~/Content/Attachments/");
                    var parsedAttachments = JArray.Parse(customerNotesViewModel.RemovedAttachments.First().ToString());
                    foreach (var attachment in parsedAttachments)
                    {

                        string fullPath = String.Format("{0}{1}-{2}\\{3}\\{4}", attachmentsPath, customer.CompanyName, customer.Id, customerNotesViewModel.Id, attachment);
                        System.IO.File.Delete(fullPath);
                    }
                }
                if (files.Count > 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        string path = string.Empty;
                        if (customerNotesViewModel.Id != 0)
                            path = CreateFilePath(files[i], customerNotesViewModel.CustomerId, customerNotesViewModel.Id);
                        else
                            path = CreateFilePath(files[i], customerNotesViewModel.CustomerId, savedCustomerNote.Id);
                        files[i].SaveAs(path);
                    }

                }


            }
            catch (DbEntityValidationException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
            _customerNotesViewModel.CustomerNotesList = _uow.CustomerNotesRepo.SearchInclude(x => x.CustomerId == customerNotesViewModel.CustomerId, "CustomerNoteDemo", "CustomerNoteReport", "CustomerNotesVisitType", "User").ToList();
            return View("_CustomerNotesList", _customerNotesViewModel);
        }
        public CustomerNote SaveCustomerNote(CustomerNotesViewModel customerNotesViewModel)
        {
            CustomerNote customerNote = Mapper.Map<CustomerNote>(customerNotesViewModel);
            customerNote.UserId = User.Identity.GetUserId();
            _uow.CustomerNotesRepo.Add(customerNote);

            return customerNote;
        }
        /// <summary>
        /// Create File path to save file
        /// Folder structure is: CompanyName-CustomerId/NoteId
        /// </summary>
        /// <param name="file"></param>
        /// <param name="customerId"></param>
        /// <param name="noteId"></param>
        /// <returns></returns>
        private string CreateFilePath(HttpPostedFileBase file, int customerId, int noteId)
        {
            var customer = _uow.CustomersRepo.Find(customerId);
            string fileName = String.Empty;
            string filePath = String.Empty;
            string attachmentsPath = Server.MapPath("~/Content/Attachments/");
            string fullPath = String.Format("{0}{1}-{2}\\{3}", attachmentsPath, customer.CompanyName, customer.Id, noteId);
            bool exists = Directory.Exists(fullPath);
            if (!exists)
                Directory.CreateDirectory(fullPath);
            fileName = Path.GetFileName(file.FileName);
            filePath = (Path.Combine(fullPath, fileName));

            return filePath;
        }
        private string GetFileNamesInFolderByNoteId(int customerId, int noteId)
        {
            var customer = _uow.CustomersRepo.Find(customerId);
            string fileName = String.Empty;
            string filePath = String.Empty;
            string attachmentsPath = Server.MapPath("~/Content/Attachments/");
            string fullPath = String.Format("{0}{1}-{2}\\{3}", attachmentsPath, customer.CompanyName, customer.Id, noteId);
            return fullPath;
        }
        public void UpdateCustomerNote(CustomerNotesViewModel customerNotesViewModel)
        {
            CustomerNote customerNote = Mapper.Map<CustomerNote>(customerNotesViewModel);
            customerNote.UpdateNoteDate = DateTime.Now;
            _uow.CustomerNotesRepo.Update(customerNote);
        }
        [HttpPost]
        public ActionResult DownloadCustomerNoteAttachment(int customerId, string attachmentName, int noteId)
        {
            var customer = _uow.CustomersRepo.Find(customerId);
            string filePath = String.Format("{0}{1}-{2}/{3}/{4}",
                "/Content/Attachments/",
                customer.CompanyName,
                customer.Id,
                noteId,
                attachmentName);
            return Json(filePath, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult EditCustomerContact(int customerContactId)
        {
            var customerContact = _uow.CustomerContactsRepo.Find(customerContactId);
            CustomerContactsViewModel customerContactsViewModel = Mapper.Map<CustomerContactsViewModel>(customerContact);
            return View("_CustomerContactsForm", customerContactsViewModel);
        }
        [HttpGet]
        public ActionResult EditCustomerNote(int customerNoteId)
        {
            var customerNote = _uow.CustomerNotesRepo.Find(customerNoteId);
            CustomerNotesViewModel customerNotesViewModel = Mapper.Map<CustomerNotesViewModel>(customerNote);
            customerNotesViewModel.CustomerNotesReportList.AddRange(GetCustomerNoteReport());
            customerNotesViewModel.CustomerNotesVisitTypeList.AddRange(GetCustomerNoteVisitType());
            customerNotesViewModel.CustomerNotesDemoList.AddRange(GetCustomerNoteDemo());
            string path = GetFileNamesInFolderByNoteId(customerNote.CustomerId, customerNoteId);
            if (Directory.Exists(path))
            {
                foreach (string fileName in Directory.GetFiles(path).Select(Path.GetFileName))
                    customerNotesViewModel.Attachments.Add(fileName);
            }
            return View("_CustomerNotesForm", customerNotesViewModel);
        }

        [HttpPost]
        public ActionResult DeleteCustomerContact(int customerId, int customerContactId)
        {
            _uow.CustomerContactsRepo.Remove(customerContactId);
            try
            {
                _uow.SaveChanges();
                CustomerContactsViewModel _customerContactsViewModel = new CustomerContactsViewModel();
                _customerContactsViewModel.CustomerContactsList = _uow.CustomerContactsRepo.Search(x => x.CustomerId == customerId).ToList();
                return View("_CustomerContactList", _customerContactsViewModel);
            }
            catch (Exception)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        public ActionResult DynamicTable(CustomerViewModel customerViewModel)
        {
            TableFilterSortPagingService<Customer, CustomerViewModel> tableFilter = new TableFilterSortPagingService<Customer, CustomerViewModel>();
            GenericViewModel<CustomerViewModel, Customer> genericViewModel = new GenericViewModel<CustomerViewModel, Customer>();
            genericViewModel.QueryParameters = customerViewModel.QueryParameters;
            genericViewModel.PageNumber = customerViewModel.PageNumber;
            genericViewModel.PageSize = customerViewModel.PageSize;
            genericViewModel.OrderBy = customerViewModel.OrderBy;
            genericViewModel.DefaultOrderBy = customerViewModel.DefaultOrderBy;
            genericViewModel.DefaultDirection = customerViewModel.DefaultDirection;
            genericViewModel.Direction = customerViewModel.Direction;
            genericViewModel.QueryOperatorComparer = customerViewModel.QueryOperatorComparer;
            genericViewModel.SpecialConditionQueryOperatorComparer = ExpressionType.And.ToString();

            genericViewModel.QueryParameters.Add(new ExpressionBuilderParameters { SearchKey = "IsActive", Operator = "Equals", Value = "true", HasSpecialConditionQueryOperatorComparer = true });
            if (customerViewModel.IsCustomers)
                genericViewModel.QueryParameters.Add(new ExpressionBuilderParameters { SearchKey = "CustomerStatusId", Operator = "Equals", Value = "1", HasSpecialConditionQueryOperatorComparer = true });
            else
                genericViewModel.QueryParameters.Add(new ExpressionBuilderParameters { SearchKey = "CustomerStatusId", Operator = "Equals", Value = "2", HasSpecialConditionQueryOperatorComparer = true });

            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _customerViewModel);
            _customerViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _customerViewModel.Direction = tableFilterResult.Direction;
            _customerViewModel.OrderBy = tableFilterResult.OrderBy;
            _customerViewModel.PageNumber = tableFilterResult.PageNumber;
            _customerViewModel.QueryCount = tableFilterResult.QueryCount;
            _customerViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _customerViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _customerViewModel.CustomersList = tableFilterResult.ResultList;
            _customerViewModel.TableCount = tableFilterResult.TableCount;

            TableCache tableCache = GetCustomerTableCache(_customerViewModel.IsCustomers);
            SaveCustomerTableCache(_customerViewModel.IsCustomers, tableCache, tableFilterResult.QueryParameters, tableFilterResult.PageNumber);
            var tablePartial = CRM.Web.Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerList", _customerViewModel);
            var pagingPartial = CRM.Web.Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerPagination", _customerViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = tableFilterResult.PageNumber,
                RowsFrom = tableFilterResult.ResultList.FirstItemOnPage,
                RowsTo = tableFilterResult.ResultList.LastItemOnPage,
                totalCount = customerViewModel.QueryParameters != null ? tableFilterResult.QueryCount : tableFilterResult.TableCount,
            }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetDashboardListsByUser(int customerId)
        {
            var currentUserId = User.Identity.GetUserId();
            var res = GetDashboardListsByUserFilteredByCustomer(currentUserId, customerId);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveCustomerInUserLists(int customerId, int[] userDashboardListIds)
        {
            foreach (var userdashboardListId in userDashboardListIds)
            {
                var dashboardListId = _uow.UserDashboardListsRepo.Search(x => x.Id == userdashboardListId).SingleOrDefault().DashboardListId;
                var columnId = _uow.DashboardListColumnsRepo.Search(x => x.DashboardListId == dashboardListId)
                    .OrderBy(x => x.ColumnOrder).First().Id;
                _uow.CustomerDashboardListsRepo.Add(new CustomerDashboardList
                {
                    UserDashboardListId = userdashboardListId,
                    CustomerId = customerId,
                    DashboardListColumnId = columnId
                });
            }
            try
            {
                _uow.SaveChanges();
                var currentUserId = User.Identity.GetUserId();
                var res = GetDashboardListsByUserFilteredByCustomer(currentUserId, customerId);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        private List<DashboardList> GetDashboardListsByUserFilteredByCustomer(string userId, int customerId)
        {
            var userLists = _uow.UserDashboardListsRepo.SearchInclude(x => x.UserId == userId, "DashboardList").ToList();
            var customerLists = _uow.CustomerDashboardListsRepo.SearchInclude(x => x.CustomerId == customerId).Select(x => x.UserDashboardListId).ToList();
            return userLists
                .Where(x => !customerLists.Contains(x.Id))
                .Select(x => new DashboardList { Id = x.Id, Name = x.DashboardList.Name })
                .ToList();
        }

        [CRMAuthorize]
        public ActionResult CustomerAppointments(int customerId)
        {
            CustomerAppointmentViewModel customerAppointmentViewModel = new CustomerAppointmentViewModel();
            customerAppointmentViewModel.CustomerId = customerId;
            customerAppointmentViewModel.DefaultOrderBy = "Date";
            customerAppointmentViewModel.Direction = "Desc";
            customerAppointmentViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();
            customerAppointmentViewModel.QueryParameters.Add(new ExpressionBuilderParameters
            {
                SearchKey = "CustomerId",
                Operator = OperatorComparer.Equals.ToString(),
                Value = customerAppointmentViewModel.CustomerId.ToString()
            });
            customerAppointmentViewModel = DynamicCustomerAppointmentsTable(customerAppointmentViewModel);
            customerAppointmentViewModel.Hours = CRM.Application.Core.Constants.Constants.Hours;
            customerAppointmentViewModel.Minutes = CRM.Application.Core.Constants.Constants.Minutes;
            var appointments = _uow.CustomerAppointmentsRepo.Search(x => x.CustomerId == customerId);
            foreach (var appointment in appointments)
            {
                customerAppointmentViewModel.AppointmentsList.Add(new Appointments
                {
                    id = appointment.Id,
                    title = appointment.Subject,
                    start = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.StartTime.Hours, appointment.StartTime.Minutes, 0),
                    description = appointment.AppointmentDescription,
                    className = new object[2] { "events", appointment.AppointmentColor },
                    icon = appointment.AppointmentIcon,
                    allDay = false,
                });
            }
            customerAppointmentViewModel.CustomerNotesReportList.AddRange(GetCustomerNoteReport());
            customerAppointmentViewModel.CustomerNotesVisitTypeList.AddRange(GetCustomerNoteVisitType());
            customerAppointmentViewModel.CustomerNotesDemoList.AddRange(GetCustomerNoteDemo());
            return View("_CustomerAppointments", customerAppointmentViewModel);
        }

        private CustomerAppointmentViewModel CustomerCalendarAppointments(CustomerAppointmentViewModel customerAppointmentViewModel)
        {

            return customerAppointmentViewModel;
        }


        [HttpPost]
        [JsonDateFilter]
        public ActionResult CreateAppointment(CustomerAppointmentViewModel customerAppointmentViewModel)
        {
            if (customerAppointmentViewModel.Id != 0)
            {
                var customerAppointment = _uow.CustomerAppointmentsRepo.Find(customerAppointmentViewModel.Id);
                customerAppointment.Date = customerAppointmentViewModel.Date;
                customerAppointment.Subject = customerAppointmentViewModel.Subject;
                customerAppointment.StartTime = new TimeSpan(customerAppointmentViewModel.StartTimeHour, customerAppointmentViewModel.StartTimeMinute, 0);
                customerAppointment.EndTime = new TimeSpan(customerAppointmentViewModel.EndTimeHour, customerAppointmentViewModel.EndTimeMinute, 0);
                customerAppointment.AppointmentDescription = customerAppointmentViewModel.AppointmentDescription;
                customerAppointment.AppointmentColor = customerAppointmentViewModel.AppointmentColor;
                customerAppointment.AppointmentIcon = customerAppointmentViewModel.AppointmentIcon;
                customerAppointment.UserId = User.Identity.GetUserId();
                customerAppointment.AppointmentNote = customerAppointmentViewModel.AppointmentNote;
                _uow.CustomerAppointmentsRepo.Update(customerAppointment);
                var appointmentNote = _uow.CustomerNotesRepo.Search(x => x.CustomerAppointmentId.Value == customerAppointment.Id).SingleOrDefault();

                if (appointmentNote != null)
                {
                    appointmentNote.Note = customerAppointmentViewModel.AppointmentNote;
                    appointmentNote.CustomerNoteDemoId = customerAppointmentViewModel.CustomerNotesDemoId;
                    appointmentNote.CustomerNoteReportId = customerAppointmentViewModel.CustomerNotesReportId;
                    appointmentNote.CustomerNoteVisitTypeId = customerAppointmentViewModel.CustomerNotesVisitTypeId;
                    appointmentNote.UpdateNoteUserId = User.Identity.GetUserId();
                    appointmentNote.UpdateNoteDate = DateTime.Now;
                    _uow.CustomerNotesRepo.Update(appointmentNote);
                }

                var appointmentEmails = _uow.AppointmentEmailsRepo.Search(x => x.CustomerAppointmentId == customerAppointmentViewModel.Id).ToList();
                _uow.AppointmentEmailsRepo.RemoveRange(appointmentEmails);

                if (customerAppointmentViewModel.AppointmentCustomerEmails != null && customerAppointmentViewModel.AppointmentCustomerEmails.Length > 0)
                {
                    foreach (var customerEmail in customerAppointmentViewModel.AppointmentCustomerEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail { Email = customerEmail, AppointmentEmailTypeId = 1, CustomerAppointmentId = customerAppointmentViewModel.Id });
                    }
                }
                if (customerAppointmentViewModel.AppointmentSalesPersonEmails != null && customerAppointmentViewModel.AppointmentSalesPersonEmails.Length > 0)
                {

                    foreach (var salespersonEmail in customerAppointmentViewModel.AppointmentSalesPersonEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail { Email = salespersonEmail, AppointmentEmailTypeId = 2, CustomerAppointmentId = customerAppointmentViewModel.Id });
                    }
                }
            }
            else
            {
                CustomerAppointment customerAppointment = new CustomerAppointment();
                customerAppointment.Date = customerAppointmentViewModel.Date;
                customerAppointment.Subject = customerAppointmentViewModel.Subject;
                customerAppointment.StartTime = new TimeSpan(customerAppointmentViewModel.StartTimeHour, customerAppointmentViewModel.StartTimeMinute, 0);
                customerAppointment.EndTime = new TimeSpan(customerAppointmentViewModel.EndTimeHour, customerAppointmentViewModel.EndTimeMinute, 0);
                customerAppointment.AppointmentDescription = customerAppointmentViewModel.AppointmentDescription;
                customerAppointment.AppointmentColor = customerAppointmentViewModel.AppointmentColor;
                customerAppointment.AppointmentIcon = customerAppointmentViewModel.AppointmentIcon;
                customerAppointment.UserId = User.Identity.GetUserId();
                customerAppointment.AppointmentNote = customerAppointmentViewModel.AppointmentNote;
                customerAppointment.IsSaveAppointNote = customerAppointmentViewModel.IsSaveAppointmentNote;
                customerAppointment.CustomerId = customerAppointmentViewModel.CustomerId;
                if (customerAppointmentViewModel.IsSaveAppointmentNote)
                {
                    _uow.CustomerNotesRepo.Add(new CustomerNote
                    {
                        Note = customerAppointmentViewModel.AppointmentNote,
                        UserId = User.Identity.GetUserId(),
                        CustomerAppointmentId = customerAppointmentViewModel.Id,
                        CustomerNoteReportId = customerAppointmentViewModel.CustomerNotesReportId,
                        CustomerNoteVisitTypeId = customerAppointmentViewModel.CustomerNotesVisitTypeId,
                        CustomerNoteDemoId = customerAppointmentViewModel.CustomerNotesDemoId,
                        CustomerId = customerAppointmentViewModel.SelectedCustomerId,
                        CreationDate = DateTime.Now
                    });
                }
                if (customerAppointmentViewModel.AppointmentCustomerEmails != null && customerAppointmentViewModel.AppointmentCustomerEmails.Length > 0)
                {
                    foreach (var customerEmail in customerAppointmentViewModel.AppointmentCustomerEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail
                        {
                            Email = customerEmail,
                            AppointmentEmailTypeId = 1,
                            CustomerAppointmentId = customerAppointment.Id
                        });
                    }
                }
                if (customerAppointmentViewModel.AppointmentSalesPersonEmails != null && customerAppointmentViewModel.AppointmentSalesPersonEmails.Length > 0)
                {

                    foreach (var salespersonEmail in customerAppointmentViewModel.AppointmentSalesPersonEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail { Email = salespersonEmail, AppointmentEmailTypeId = 2, CustomerAppointmentId = customerAppointment.Id });
                    }
                }
                _uow.CustomerAppointmentsRepo.Add(customerAppointment);
            }

            try
            {
                _uow.SaveChanges();
                if (customerAppointmentViewModel.IsSaveAppointmentNote)
                    dailyReportService.CreateDailyReport();
                SendAppointmentEmails(customerAppointmentViewModel, customerAppointmentViewModel.IsResendEmails);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Failed");
            }
            customerAppointmentViewModel.QueryParameters.Add(new ExpressionBuilderParameters
            {
                SearchKey = "CustomerId",
                Operator = OperatorComparer.Equals.ToString(),
                Value = customerAppointmentViewModel.CustomerId.ToString()
            });
            customerAppointmentViewModel.DefaultOrderBy = "Date";
            customerAppointmentViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();
            customerAppointmentViewModel.Direction = "Desc";
            var customerAppointmentViewModelResult = DynamicCustomerAppointmentsTable(customerAppointmentViewModel);
            var tablePartial = CRM.Web.Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerAppointmentsList", customerAppointmentViewModelResult);
            var pagingPartial = CRM.Web.Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerAppointmentPagination", customerAppointmentViewModelResult);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = customerAppointmentViewModelResult.PageNumber,
                RowsFrom = customerAppointmentViewModelResult.FirstItemOnPage,
                RowsTo = customerAppointmentViewModelResult.LastItemOnPage,
                totalCount = customerAppointmentViewModel.QueryParameters != null ? customerAppointmentViewModelResult.QueryCount : customerAppointmentViewModelResult.TableCount
            }, JsonRequestBehavior.AllowGet);
        }
        private void SendAppointmentEmails(CustomerAppointmentViewModel customerAppointmentViewModel, bool resendEmail)
        {
            //if (resendEmail)
            //{
            var client = new SmtpClient(CompanyMailServer, CompanyMailPort)
            {
                Credentials = new NetworkCredential(CompanyMailAddress, CompanyMailPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,

            };
            var TemplateFolderPath = Server.MapPath("~/Views/EmailTemplates");
            var customersMailBody = RazorEngineRender.PartialViewToString(TemplateFolderPath, "CustomersAppointmentTemplate.cshtml", new AppointmentEmailTemplateViewModel
            {
                AppointmentSubject = customerAppointmentViewModel.AppointmentDescription,
                AppointmentDate = new DateTime(customerAppointmentViewModel.Date.Year,
                customerAppointmentViewModel.Date.Month,
                customerAppointmentViewModel.Date.Day,
                customerAppointmentViewModel.StartTimeHour,
                customerAppointmentViewModel.StartTimeMinute,
                0).ToString(),

                AppointmentToTime = new DateTime(customerAppointmentViewModel.Date.Year,
                customerAppointmentViewModel.Date.Month,
                customerAppointmentViewModel.Date.Day,
                customerAppointmentViewModel.EndTimeHour,
                customerAppointmentViewModel.EndTimeMinute,
                0).ToShortTimeString(),

                IsSalesPerson = false
            });
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(CompanyMailAddress);
            mail.Subject = "Customer Appointment";
            mail.Body = customersMailBody;
            mail.IsBodyHtml = true;
            var ical = _appointmentService.GenerateAppointmentIcal(new DateTime(customerAppointmentViewModel.Date.Year,
              customerAppointmentViewModel.Date.Month,
              customerAppointmentViewModel.Date.Day,
              customerAppointmentViewModel.StartTimeHour,
              customerAppointmentViewModel.StartTimeMinute,
              0), new DateTime(customerAppointmentViewModel.Date.Year,
              customerAppointmentViewModel.Date.Month,
              customerAppointmentViewModel.Date.Day,
              customerAppointmentViewModel.EndTimeHour,
              customerAppointmentViewModel.EndTimeMinute,
              0),
            customerAppointmentViewModel.Subject,
            customerAppointmentViewModel.AppointmentDescription);
            Attachment attachment = new System.Net.Mail.Attachment(ical, "appointment.ics", "text/calendar");
            mail.Attachments.Add(attachment);
            if (customerAppointmentViewModel.AppointmentCustomerEmails != null && customerAppointmentViewModel.AppointmentCustomerEmails.Length > 0)
            {
                foreach (var mailToCustomer in customerAppointmentViewModel.AppointmentCustomerEmails)
                {
                    mail.To.Add(mailToCustomer);
                }
                client.Send(mail);

            }

            var salespersonMailBody = RazorEngineRender.PartialViewToString(TemplateFolderPath, "CustomersAppointmentTemplate.cshtml", new AppointmentEmailTemplateViewModel
            {
                AppointmentSubject = customerAppointmentViewModel.AppointmentDescription,
                AppointmentNote = customerAppointmentViewModel.AppointmentNote,
                AppointmentDate = new DateTime(customerAppointmentViewModel.Date.Year,
                customerAppointmentViewModel.Date.Month,
                customerAppointmentViewModel.Date.Day,
                customerAppointmentViewModel.StartTimeHour,
                customerAppointmentViewModel.StartTimeMinute,
                0).ToString(),

                AppointmentToTime = new DateTime(customerAppointmentViewModel.Date.Year,
                customerAppointmentViewModel.Date.Month,
                customerAppointmentViewModel.Date.Day,
                customerAppointmentViewModel.EndTimeHour,
                customerAppointmentViewModel.EndTimeMinute,
                0).ToShortTimeString(),

                IsSalesPerson = true
            });
            mail.Body = salespersonMailBody;
            mail.To.Clear();
            if (customerAppointmentViewModel.AppointmentSalesPersonEmails != null && customerAppointmentViewModel.AppointmentSalesPersonEmails.Length > 0)
            {
                foreach (var mailToSalesperson in customerAppointmentViewModel.AppointmentSalesPersonEmails)
                {
                    mail.To.Add(mailToSalesperson);
                }
                client.Send(mail);
            }
            //}
        }
        private void CreateCustomerAppointment(CustomerAppointmentViewModel customerAppointmentViewModel)
        {
            throw new NotImplementedException();
        }

        private void UpdateCustomerAppointment(CustomerAppointmentViewModel customerAppointmentViewModel)
        {
            throw new NotImplementedException();
        }


        public ActionResult RemoveCustomerAppointment(int appointmentId, int customerId)
        {
            RemoveAppointmentWithEmailsAndNotes(appointmentId);
            CustomerAppointmentViewModel customerAppointmentViewModel = new CustomerAppointmentViewModel();
            customerAppointmentViewModel.QueryParameters.Add(new ExpressionBuilderParameters
            {
                SearchKey = "CustomerId",
                Operator = OperatorComparer.Equals.ToString(),
                Value = customerId.ToString()
            });
            customerAppointmentViewModel.DefaultOrderBy = "Date";
            customerAppointmentViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();
            customerAppointmentViewModel.Direction = "Desc";
            var customerAppointmentViewModelResult = DynamicCustomerAppointmentsTable(customerAppointmentViewModel);
            var tablePartial = CRM.Web.Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerAppointmentsList", customerAppointmentViewModelResult);
            var pagingPartial = CRM.Web.Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerAppointmentPagination", customerAppointmentViewModelResult);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = customerAppointmentViewModelResult.PageNumber,
                RowsFrom = customerAppointmentViewModelResult.FirstItemOnPage,
                RowsTo = customerAppointmentViewModelResult.LastItemOnPage,
                totalCount = customerAppointmentViewModel.QueryParameters != null ? customerAppointmentViewModelResult.QueryCount : customerAppointmentViewModelResult.TableCount
            }, JsonRequestBehavior.AllowGet);
        }

        private void RemoveAppointmentWithEmailsAndNotes(int appointmentId)
        {
            var customerAppointment = _uow.CustomerAppointmentsRepo.Find(appointmentId);
            if (customerAppointment.AppointmentEmails != null && customerAppointment.AppointmentEmails.Count > 0)
                _uow.AppointmentEmailsRepo.RemoveRange(customerAppointment.AppointmentEmails);
            if (customerAppointment.IsSaveAppointNote)
            {
                var customerAppointmentNote = _uow.CustomerNotesRepo.Search(x => x.CustomerAppointmentId == customerAppointment.Id).SingleOrDefault();
                if (customerAppointmentNote != null)
                    _uow.CustomerNotesRepo.Remove(customerAppointmentNote.Id);
            }
            _uow.CustomerAppointmentsRepo.Remove(customerAppointment.Id);
            _uow.SaveChanges();
        }
        private CustomerAppointmentViewModel DynamicCustomerAppointmentsTable(CustomerAppointmentViewModel customerAppointmentViewModel)
        {

            TableFilterSortPagingService<CustomerAppointment, CustomerAppointmentViewModel> tableFilter = new TableFilterSortPagingService<CustomerAppointment, CustomerAppointmentViewModel>();
            GenericViewModel<CustomerAppointmentViewModel, CustomerAppointment> genericViewModel = new GenericViewModel<CustomerAppointmentViewModel, CustomerAppointment>();
            genericViewModel.QueryParameters = customerAppointmentViewModel.QueryParameters;
            genericViewModel.PageNumber = customerAppointmentViewModel.PageNumber;
            genericViewModel.PageSize = customerAppointmentViewModel.PageSize;
            genericViewModel.OrderBy = customerAppointmentViewModel.OrderBy;
            genericViewModel.DefaultOrderBy = customerAppointmentViewModel.DefaultOrderBy;
            genericViewModel.DefaultDirection = customerAppointmentViewModel.DefaultDirection;
            genericViewModel.Direction = customerAppointmentViewModel.Direction;
            genericViewModel.QueryOperatorComparer = customerAppointmentViewModel.QueryOperatorComparer;

            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _customerViewModel);
            CustomerAppointmentViewModel _customerAppointmentViewModel = new CustomerAppointmentViewModel();
            _customerAppointmentViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _customerAppointmentViewModel.Direction = tableFilterResult.Direction;
            _customerAppointmentViewModel.OrderBy = tableFilterResult.OrderBy;
            _customerAppointmentViewModel.PageNumber = tableFilterResult.PageNumber;
            _customerAppointmentViewModel.FirstItemOnPage = tableFilterResult.ResultList.FirstItemOnPage;
            _customerAppointmentViewModel.LastItemOnPage = tableFilterResult.ResultList.LastItemOnPage;
            _customerAppointmentViewModel.QueryCount = tableFilterResult.QueryCount;
            _customerAppointmentViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _customerAppointmentViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _customerAppointmentViewModel.CustomerAppointmentsList = tableFilterResult.ResultList;
            _customerAppointmentViewModel.TableCount = tableFilterResult.TableCount;
            return _customerAppointmentViewModel;
        }

        public ActionResult AddLicense(int customerId, int amount, int productId, bool newProduct)
        {

            var customer = _uow.CustomersRepo.Find(customerId);
            var product = _uow.ComasysProductsRepo.Find(productId);
            if (amount < 1)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            List<CustomerSubscription> customerSubscriptions = new List<CustomerSubscription>();
            for (int i = 0; i < amount; i++)
            {
                customerSubscriptions.Add(new CustomerSubscription
                { CustomerId = customerId, ProductId = productId, CrayonSupcriptionId = null });
            }

            _uow.CustomerSubscriptionRepo.AddRange(customerSubscriptions);

            try
            {
                _uow.SaveChanges();
                if (newProduct)
                {
                    var customerOrdersViewModel = GetCustomerOrders(customerId);
                    return View("_CustomerOrders", customerOrdersViewModel);
                }
                else
                    return Json(amount, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            #region crayonIntegration
            //var customer = _uow.CustomersRepo.SearchInclude(x => x.Id == customerOrdersViewModel.CustomerId, "User").FirstOrDefault();

            //var referenceType = _uow.ReferenceTypeRepo.Search(x => x.Name == "Crayon").FirstOrDefault();

            //var hasReference = customer.ReferenceList != null ? customer.ReferenceList.Any(x => x.Type == referenceType) : false;

            ////return CustomerOrders(customerOrdersViewModel.CustomerId);

            //if (hasReference)
            //{
            //    //Update
            //}
            //else
            //{
            //    //Create new
            //    int.TryParse(ConfigurationManager.AppSettings["Crayon:OrganizationId"], out var OrganizationId);

            //    var resultAgreements = crayonApi.Agreements.Get(token.Data.AccessToken);

            //    var invoiceFilter = new InvoiceProfileFilter
            //    {
            //        OrganizationId = OrganizationId
            //    };
            //    var resultInvoiceProfiles = crayonApi.InvoiceProfiles.Get(token.Data.AccessToken, invoiceFilter);

            //    var getCustomerTenantFilter = new CustomerTenantFilter
            //    {
            //        OrganizationId = OrganizationId,
            //        Page = 1,
            //        PageSize = 2,
            //        Search = "mycustomer"
            //    };

            //    var resultGetCustomerTenant = crayonApi.CustomerTenants.Get(token.Data.AccessToken, getCustomerTenantFilter);

            //    var customerTenantDetailed = new CustomerTenantDetailed
            //    {
            //        Tenant = new CustomerTenant
            //        {
            //            CustomerTenantType = CustomerTenantType.T2, //TODO: Jonas hvad gør vi her, hvilken type er det?
            //            DomainPrefix = "mycustomer",
            //            InvoiceProfile = new ObjectReference { Id = resultInvoiceProfiles.Data.Items.FirstOrDefault().Id }, //TODO: Hvad gør vi her? < invoice profile id > result.Data.Items[0].Id
            //            Name = customer.CompanyName,
            //            Organization = new Organization { Id = OrganizationId },
            //            Publisher = new ObjectReference { Id = resultAgreements.Data.Items[0].Publisher.Id } //TODO: Hvad gør vi her? lige nu tager vi bare den første i listen af items
            //            //de er alle 3 fra microsoft, vi kunne også ligge Microsoft's publisher id'et i appsettings? Hvis det ikke bliver skiftet
            //        },
            //        Profile = new CustomerTenantProfile
            //        {
            //            Address = new CustomerTenantAddress
            //            {
            //                FirstName = customer.User.FirstName,
            //                LastName = "asd", //TODO: Hardcoded lastname customer.User.LastName,
            //                AddressLine1 = customer.Address,
            //                City = customer.Town,
            //                Region = "Syd Danmark",//TODO: Jonas hvad gør vi her? Må ikke være tom, for så melder API'et fejl
            //                CountryCode = "DK", //TODO: Jonas hvad gør vi her?
            //                PostalCode = "5000" //TODO: hardcoded postnummer customer.PostalCode
            //            },
            //            Contact = new CustomerTenantContact
            //            {
            //                FirstName = customer.User.FirstName,
            //                LastName = "asd", //TODO: Hardcoded lastname - customer.User.LastName,
            //                Email = customer.User.Email,
            //                PhoneNumber = customer.User.PhoneNumber != null ? customer.User.PhoneNumber : "00 00 00 00"
            //                //TODO: Telefonnummer bør ikke kunne være null fordi så melder crayon api'et fejl
            //            }
            //        }
            //    };

            //    var resultCreateTenant = crayonApi.CustomerTenants.Create(token.Data.AccessToken, customerTenantDetailed);

            //    if (resultCreateTenant.IsSuccessStatusCode)
            //    {

            //        Referencelist refList = new Referencelist
            //        {
            //            CustomerId = customerOrdersViewModel.CustomerId,
            //            Customer = customer,
            //            ReferenceId = resultCreateTenant.Data.Tenant.Id.ToString(),
            //            AdditionalData = resultCreateTenant.Content,
            //            Type = referenceType
            //        };

            //        customer.ReferenceList.Add(refList);

            //        _uow.ReferencelistRepo.Add(refList);
            //    }
            //}

            #endregion

        }
        private ComasysOrdersViewModel GetCustomerOrders(int customerId)
        {
            var customerOrdersViewModel = new ComasysOrdersViewModel { CustomerId = customerId };
            var productList = _uow.ComasysProductsRepo.GetAll();
            customerOrdersViewModel.ProductList = _uow.ComasysProductsRepo.GetAll();

            var customerSubscriptions = _uow.CustomerSubscriptionRepo.SearchInclude(x => x.CustomerId == customerId).ToList();
            foreach (var sub in customerSubscriptions)
            {
                var product = productList.Find(i => i.Id == sub.ProductId);

                if (product != null)
                {
                    productList.Remove(product);
                    customerOrdersViewModel.OwnedProducts.Add(new OwnedProduct()
                    {
                        product = product,
                        licenseQuantity = customerSubscriptions.Count(x => x.ProductId == product.Id)

                    });
                }
            }
            customerOrdersViewModel.ProductList = productList;
            return customerOrdersViewModel;
        }
        public ActionResult RemoveLicense(int customerId, int amount, int productId)
        {
            var customer = _uow.CustomersRepo.Find(customerId);
            var product = _uow.ComasysProductsRepo.Find(productId);
            var licenseQuantity = _uow.CustomerSubscriptionRepo.SearchInclude(x => x.CustomerId == customerId)
                .Count(x => x.ProductId == product.Id);
            if (licenseQuantity < amount)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            List<CustomerSubscription> customerSubscriptions
                = _uow.CustomerSubscriptionRepo.Search(x => x.CustomerId == customerId && x.ProductId == productId)
                    .ToList().OrderByDescending(x => x.Id).ToList();
            List<CustomerSubscription> removeCustomerSubscriptions = new List<CustomerSubscription>();
            for (int i = 0; i < amount; i++)
            {
                var cusSub = customerSubscriptions.First();
                removeCustomerSubscriptions.Add(cusSub);
                customerSubscriptions.Remove(cusSub);
            }

            _uow.CustomerSubscriptionRepo.RemoveRange(removeCustomerSubscriptions);

            try
            {
                _uow.SaveChanges();

                if (licenseQuantity - amount != 0) return Json(amount, JsonRequestBehavior.AllowGet);
                var customerOrdersViewModel = GetCustomerOrders(customerId);
                return View("_CustomerOrders", customerOrdersViewModel);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        public JsonResult CustomerExists(int CustomerId)
        {
            bool exists = _uow.CustomersRepo.Find(CustomerId) != null;
            return Json(exists, JsonRequestBehavior.AllowGet);
        }

        [CRMAuthorize]
        public ActionResult SetCustomerInactive(int customerId)
        {
            var customer = _uow.CustomersRepo.Find(customerId);
            customer.IsActive = false;
            try
            {
                _uow.SaveChanges();
                return Json(customer.CustomerStatusId, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(JsonRequestBehavior.DenyGet);
            }

        }
    }
}