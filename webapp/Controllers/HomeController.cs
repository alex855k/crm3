#region Using
using AutoMapper;
using CRM.Application.Core.Enums;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using DateTime = System.DateTime;

#endregion

namespace CRM.Web.Controllers
{
    [System.Web.Mvc.Authorize]
    public class HomeController : Controller
    {
        // GET: home/index
        UnitofWork _uow = new UnitofWork();
        public ActionResult Index()
        {
            if (!User.IsInRole("Admin"))
            {
                var dashboardLists = _uow.DashboardListsRepo.GetAll();
                DashboardListsViewModel dashboardListsViewModel = new DashboardListsViewModel();
                dashboardListsViewModel.DashboardLists = Mapper.Map<List<DashboardListsViewModel>>(dashboardLists);
                return View(dashboardListsViewModel);
            }
            else
            {
                return View(new DashboardListsViewModel());
            }
                
        }

        public ActionResult ProductModal()
        {
            return View("_ProductModal");
        }

        [CRMAuthorize]
        public ActionResult Widget()
        {
            if (!User.IsInRole("Admin"))
            {
                var currentUserId = User.Identity.GetUserId();
                var dashboardViewModel = GetDashboardList(currentUserId);

                return View("_Widget", dashboardViewModel);
            }
            else
                return View("_Widget", new List<DashboardList>());
        }

        public ActionResult GetActiveTimeReg(string UserId)
        {
            var list = _uow.TimeRegistrationRepo.Search(x => x.IsActive).Where(x => x.UserId == UserId);

            var registrations = list.ToList();
            var timeRegistrations = registrations.ToList();
            if (timeRegistrations.Any())
            {
                var cCase = _uow.CustomerCaseRepo.Find((registrations.FirstOrDefault())?.CustomerCaseId);

                return Json(new
                {
                    success = true,
                    ActiveCase = true,
                    activeTimereg = registrations.FirstOrDefault(),
                    Start = timeRegistrations.FirstOrDefault()?.StartDateTime.ToUniversalTime()
                        .ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    customerCase = cCase,
                    responseText = "Success."
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new {success = true, ActiveCase = false, responseText = "Success."},
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetWebShopUrl(int customerId, int caseId)
        {
            var webshopUrl = CreateViaWebShop(customerId, caseId);
            return Json(new { success = true, webshopUrl, ActiveCase = false, responseText = "Success." },
                JsonRequestBehavior.AllowGet);
        }

        public string CreateViaWebShop(int customerId, int caseId)
        {
            Customer c = _uow.CustomersRepo.Find(customerId);
            if (c == null)
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            CustomerCase cc = _uow.CustomerCaseRepo.Find(caseId);

            string base_url = System.Web.Configuration.WebConfigurationManager
                .AppSettings["WebShopUrl"].ToString();

            var encodedProps = new List<string>();
            var supportedTypes = new List<Type>
            {
                typeof(string),
                typeof(int),
                typeof(DateTime)
            };
            supportedTypes.AddRange(
                supportedTypes.Where(x => x.IsValueType)
                    .Select(x => typeof(Nullable<>).MakeGenericType(x)).ToList());

            foreach (var prop in c.GetType().GetProperties())
            {
                Type type = prop.PropertyType;
                if (supportedTypes.Contains(type))
                {
                    object val = prop.GetValue(c);
                    if (val != null)
                        encodedProps.Add(prop.Name + "=" + Server.UrlEncode(val.ToString()));
                }
            }
            foreach (var prop in cc.GetType().GetProperties())
            {
                Type type = prop.PropertyType;
                if (supportedTypes.Contains(type))
                {
                    object val = prop.GetValue(cc);
                    if (val != null)
                        encodedProps.Add("Case"+prop.Name + "=" + Server.UrlEncode(val.ToString()));
                }
            }
            string query = string.Join("&", encodedProps);
            var webShopUrl = base_url +"&"+ query;
            return webShopUrl;
        }

        public ActionResult StartTimeReg(string UserId, int customerId, int? caseId)
        {
            var list = _uow.TimeRegistrationRepo.Search(x => x.IsActive).Where(x => x.UserId == UserId);

            var timeRegistrations = list.ToList();

            if (timeRegistrations.Any())
            {
                return Json(new { success = true, AlreadyActiveCase = true, responseText = "Success." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (caseId == null)
                {
                    var gen = _uow.CustomerCaseRepo.Search(x => x.CustomerId == customerId && x.Titel == "Generel").ToList();
                    if (gen.Any())
                    {
                        caseId = gen.FirstOrDefault()?.Id;
                    }
                    else
                    {
                        var CustomerCaseType = _uow.CustomerCaseTypeRepo.Search(x => x.TypeName == "Generel").ToList();

                        CustomerCase genCase = new CustomerCase()
                        {
                            UserId = UserId,
                            CustomerId = customerId,
                            Titel = "Generel",
                            Deadline = null,
                            StartDateTime = DateTime.Now,
                            CustomerCaseTypeId = CustomerCaseType.FirstOrDefault().Id
                        };
                        _uow.CustomerCaseRepo.Add(genCase);
                        _uow.SaveChanges();
                        caseId = genCase.Id;
                    }
                }
                
                TimeRegistration timereg = new TimeRegistration();
                    timereg.Start(UserId, caseId.Value);

                    _uow.TimeRegistrationRepo.Add(timereg);

                    _uow.SaveChanges();
                    Customer c = _uow.CustomersRepo.Find(customerId);
                    return Json(
                        new
                        {
                            success = true, AlreadyActiveCase = false,
                            Started = timereg.StartDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            webshopUrl = CreateViaWebShop(customerId, caseId.Value),
                            responseText = "Success."
                        }, JsonRequestBehavior.AllowGet);
                
            }

        }

        public ActionResult StopTimeReg(string UserId, string description)
        {
            var list = _uow.TimeRegistrationRepo.Search(x => x.IsActive).Where(x => x.UserId == UserId);

            var timeRegistrations = list.ToList();

            if (!timeRegistrations.Any())
            {
                return Json(new { success = true, AlreadyActiveCase = false, responseText = "Success." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TimeRegistration timereg = timeRegistrations.FirstOrDefault();

                if (timereg != null)
                {
                    timereg.CustomerCase = _uow.CustomerCaseRepo.SearchInclude(x=> x.Id == timereg.CustomerCaseId,"Customer").FirstOrDefault();
                    timereg.Done();
                    timereg.Description = description;

                    _uow.TimeRegistrationRepo.Update(timereg);

                    _uow.SaveChanges();

                    return Json(new {
                            success = true, AlreadyActiveCase = true, interval = timereg.IntervalIsoString,
                            Stopped = timereg.EndDateTime.ToString(),
                            responseText = "Success."
                        }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, AlreadyActiveCase = false, responseText = "Null Error." }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        public ActionResult TimeRegWidget()
        {
            var res = new DashboardListsViewModel();
            res.CustomersList = _uow.CustomersRepo.GetAll().OrderBy(x=> x.CompanyName).ToList();
            return View("_TimeregWidget", res);
        }

        [System.Web.Http.HttpPost]
        public ActionResult GetCustomerCaseList(int id)
        {
            var customerCases = _uow.CustomerCaseRepo.Search(x => x.CustomerId == id).ToList();
            return Json(new { success = true, customerCasesList = customerCases, responseText = "Null Error." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomerList()
        {
            var customers = _uow.CustomersRepo.GetAll().OrderBy(x=> x.CompanyName).ToList();
            List<Customer> customerShort = new List<Customer>();
            foreach (var v in customers)
            {
                customerShort.Add(new Customer()
                {
                    CompanyName = v.CompanyName,
                    Id = v.Id
                });
            }
            return Json(new { success = true, customersList = customerShort, responseText = "Success" }, JsonRequestBehavior.AllowGet);
        }

        private List<DashboardListsViewModel> GetDashboardList(string currentUserId)
        {
            List<DashboardListsViewModel> dashboardViewModel = new List<DashboardListsViewModel>();
            bool isLoadCustomersByUser = _uow.ApplicationControllersRepo.Find((int)SystemFeatures.LoadDashboardCustomersbyUser).IsDisabled;
            if (isLoadCustomersByUser)
            {
                var res = _uow.UserDashboardListsRepo.GetDashboardListsByUserId(currentUserId);
                dashboardViewModel = DashboardListToDashboardListViewModelByUser(res);
            }
            else
            {
                var dashboardList = _uow.UserDashboardListsRepo
                    .SearchQueryable(t => t.UserId == currentUserId, "Customer").Select(t => t.DashboardList).ToList();
                dashboardViewModel = DashboardListToDashboardListViewModel(dashboardList);

            }
            return dashboardViewModel;
        }
        private List<DashboardListsViewModel> DashboardListToDashboardListViewModel(List<DashboardList> dashboardLists)
        {
            var dashboardListViewModel = Mapper.Map<List<DashboardListsViewModel>>(dashboardLists);
            return dashboardListViewModel;
        }
        private List<DashboardListsViewModel> DashboardListToDashboardListViewModelByUser(Tuple<List<DashboardList>, List<CustomerDashboardList>> dashboardLists)
        {
            var dashboardListViewModel = Mapper.Map<List<DashboardListsViewModel>>(dashboardLists.Item1);
            return dashboardListViewModel;
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult AddWidget(int dashboardListId)
        {
            var currentUserId = User.Identity.GetUserId();
            var isDashboardExist = _uow.UserDashboardListsRepo.Search(x => x.UserId == currentUserId && x.DashboardListId == dashboardListId).SingleOrDefault();
            if (isDashboardExist == null)
            {


                _uow.UserDashboardListsRepo.Add(new UserDashboardList
                {
                    UserId = currentUserId,
                    DashboardListId = dashboardListId
                });
                try
                {
                    _uow.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Administration.DashboardLists.ErrorAddListToDashboard);
                }
                var dashboardViewModel = GetDashboardList(currentUserId);
                return View("_Widget", dashboardViewModel);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Conflict, CRM.Application.Core.Resources.Administration.DashboardLists.DashboardlistDuplicate);
            }
        }
        [System.Web.Mvc.HttpPost]
        public ActionResult DeleteWidget(int dashboardlistId)
        {
            var currentUserId = User.Identity.GetUserId();
            var userDashboardList = _uow.UserDashboardListsRepo.Search(x => x.UserId == currentUserId && x.DashboardListId == dashboardlistId).SingleOrDefault();
            if (userDashboardList.CustomerDashboardList.Count() > 0)
                _uow.CustomerDashboardListsRepo.RemoveRange(userDashboardList.CustomerDashboardList.ToList());
            _uow.UserDashboardListsRepo.RemoveObject(userDashboardList);
            try
            {
                _uow.SaveChanges();
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Administration.DashboardLists.ErrorRemoveWidget);
            }
            var userDashboardLists = _uow.UserDashboardListsRepo.SearchInclude(x => x.UserId == currentUserId, "DashboardList").Select(x => x.DashboardList).ToList();
            return View("_Widget", userDashboardLists);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult RemoveCustomerFromList(int customerDashboardList)
        {
            var currentUserId = User.Identity.GetUserId();
            _uow.CustomerDashboardListsRepo.Remove(customerDashboardList);
            try
            {
                _uow.SaveChanges();
            }
            catch (Exception)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Administration.DashboardLists.ErrorRemoveCustomerFromList);
            }
            var userDashboardLists = _uow.UserDashboardListsRepo.SearchInclude(x => x.UserId == currentUserId, "DashboardList").Select(x => x.DashboardList).ToList();
            return View("_Widget", userDashboardLists);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult AssignCustomerToList(int dashboardlistId, int columnId, int customerId, int customerDashboardListId)
        {
            var currentUserId = User.Identity.GetUserId();

            var userDashboardList = _uow.UserDashboardListsRepo.Search(x => x.UserId == currentUserId && x.DashboardListId == dashboardlistId).SingleOrDefault();
            var customerDashboardList = _uow.CustomerDashboardListsRepo.Search(x => x.Id == customerDashboardListId).SingleOrDefault();
            customerDashboardList.DashboardListColumnId = columnId;
            customerDashboardList.UserDashboardListId = userDashboardList.Id;
            _uow.CustomerDashboardListsRepo.Update(customerDashboardList);
            try
            {
                _uow.SaveChanges();
            }
            catch (Exception)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Administration.DashboardLists.ErrorAssignCustomerToList);
            }

            var userDashboardLists = _uow.UserDashboardListsRepo.SearchInclude(x => x.UserId == currentUserId, "DashboardList").Select(x => x.DashboardList).ToList();
            return View("_Widget", userDashboardLists);
        }

        public ActionResult BudgetBarchartWidget()
        {
            return View("_BudgetBarchartWidget");
        }
    }
}