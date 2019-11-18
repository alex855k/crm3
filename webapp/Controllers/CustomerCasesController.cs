using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRM.Application.Core.Services;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using CRM.Web.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using X.PagedList;
using Formatting = Newtonsoft.Json.Formatting;
//using PdfSharp.Pdf;
//using Remotion.Data.Linq.Clauses;
//using Customer = CRM.Application.Core.Resources.Customers.Customer;

namespace CRM.Web.Controllers
{
    [CRMAuthorize]
    public class CustomerCasesController : Controller
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private CustomerViewModel _customerViewModel = new CustomerViewModel();
        private UnitofWork _uow = new UnitofWork();
        private UserManager _userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
        
        public ActionResult CustomerCaseIndex(int customerId)
        {
            var customerCaseViewModel = GetCustomerCaseViewModel(customerId);
            return View("_CustomerCase", customerCaseViewModel);
        }
        public ActionResult CurrentWeek()
        {
            return View("WeekView/_Current");
        }
        public ActionResult CaseWeekIndex()
        {
            return View("WeekView/_Index", null);
        }

        public ActionResult CreateModal(int customerId)
        {
            var res = GetCustomerCaseViewModel(customerId);
            return View("_CaseFormModal", res);
        }

        public ActionResult CaseFormModal()
        {
            CustomerCaseDatatableViewModel viewModel = new CustomerCaseDatatableViewModel();
            viewModel.CurrentUserID = User.Identity.GetUserId();
            viewModel.CustomersList = _uow.CustomersRepo.GetAll().OrderBy(x => x.CompanyName).ToList();
            return View("_CaseFormModal", viewModel);
        }

        public ActionResult CaseTimeRegistrationModal(int customerId)
        {
            var res = GetCustomerCaseViewModel(customerId);
            return View("_CaseTimeRegistrationModal", res);
        }

        public ActionResult CaseDrivingRegistrationModal(int customerId)
        {
            var res = GetCustomerCaseViewModel(customerId);
            return View("_CaseDrivingRegistrationModal", res);
        }

        public ActionResult AssignmentModal(int customerId)
        {
            return View("_AssignmentModal");
        }

        public ActionResult CaseTypeModal()
        {
            return View("_CaseTypeModal");
        }

        private CustomerCaseDatatableViewModel GetCustomerCaseViewModel(int customerId)
        {
            var customerCasesQuery = _uow.CustomerCaseRepo.GetAllPagination(x => x.CustomerId == customerId, 0, 10, "Pinned", "Desc", "User");

            CustomerCaseDatatableViewModel customerCaseViewModel = new CustomerCaseDatatableViewModel()
            {
                CustomerCases = new StaticPagedList<CustomerCase>(customerCasesQuery.ToList(), 1, 10, customerCasesQuery.Capacity),
                CurrentUserID = User.Identity.GetUserId(),
                CustomerId = customerId,
                QueryCount = customerCasesQuery.Capacity,
                TableCount = customerCasesQuery.Capacity,
                DefaultOrderBy = "Pinned",
                OrderBy = "Pinned",
                Direction = "Desc"
            };
            return customerCaseViewModel;
        }

        [HttpPost]
        public ActionResult Create(CustomerCase customerCase)
        {
            try
            {
                /*if (!AuthorizationService.AuthorizeRenderHTML("Edit", "CustomerCases")) {
                    customerCase.Week = null;
                    }*/
                if(customerCase.UserId == null) customerCase.UserId = User.Identity.GetUserId();
                customerCase.TotalTimeUsed = TimeSpan.Zero;
                _uow.CustomerCaseRepo.Add(customerCase);
                _uow.SaveChanges();
                if (Request.UrlReferrer.AbsolutePath == "/customercases/caseweekindex")
                {
                    return Json(new { success = true, responseText = "Successfully saved changes to case." }, JsonRequestBehavior.AllowGet);
                }
                var res = GetCustomerCaseViewModel(customerCase.CustomerId);
                return View("_CustomerCasesList", res);
              
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateCaseType(CustomerCaseType customerCaseType)
        {
            try
            {
                _uow.CustomerCaseTypeRepo.Add(customerCaseType);
                _uow.SaveChanges();
                return Json(new { success = true, responseText = "Success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetCaseTypeTable()
        {
            try
            {
                var CaseTypes = _uow.CustomerCaseTypeRepo.GetAll("User");

                return Json(new { success = true, CaseTypeList = CaseTypes, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting CaseTypes Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EndCase(int CaseId, DateTime EndDateTime)
        {
            try
            {
                var @case = _uow.CustomerCaseRepo.Find(CaseId);
                @case.EndDateTime = EndDateTime;
                @case.Done = true;
                @case.Status = CaseStatus.Afsluttet;

                _uow.CustomerCaseRepo.Update(@case);
                _uow.SaveChanges();
                if (Request.UrlReferrer.AbsolutePath == "/customercases/caseweekindex")
                {
                    return Json(new { success = true, responseText = "Successfully ended the case." }, JsonRequestBehavior.AllowGet);
                }
                var res = GetCustomerCaseViewModel(@case.CustomerId);

                return View("_CustomerCasesList", res);
            }
            catch (Exception)
            {
                return Json(new { success = false, responseText = "Ending Failed" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RestartCase(int CaseId)
        {
            try
            {
                var Case = _uow.CustomerCaseRepo.Find(CaseId);
                Case.EndDateTime = null;
                Case.Done = false;

                _uow.CustomerCaseRepo.Update(Case);
                _uow.SaveChanges();
                if (Request.UrlReferrer.AbsolutePath == "/customercases/caseweekindex")
                {
                    return Json(new { success = true, responseText = "Successfully saved changes to case." }, JsonRequestBehavior.AllowGet);
                }
                var res = GetCustomerCaseViewModel(Case.CustomerId);

                return View("_CustomerCasesList", res);
            }
            catch (Exception)
            {
                return Json(new { success = false, responseText = "Restarting Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateTimeReg(TimeRegistration timeReg, string startTime, string endTime)
        {
            timeReg.StartDateTime = DateTime.Parse(startTime);
            timeReg.EndDateTime = DateTime.Parse(endTime);
            timeReg.Interval = timeReg.EndDateTime - timeReg.StartDateTime;

            try
            {
                var customerCase = _uow.CustomerCaseRepo.Find(timeReg.CustomerCaseId);

                customerCase.TotalTimeUsed += (TimeSpan) timeReg.Interval;
                _uow.CustomerCaseRepo.Update(customerCase);
                _uow.TimeRegistrationRepo.Add(timeReg);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetCaseTimeRegs(int caseId)
        {
            try
            {
                var TimeRegs = _uow.TimeRegistrationRepo.SearchInclude(x => caseId == x.CustomerCaseId, "User", "CaseAssignment").ToList();
                foreach (var t in TimeRegs)
                {
                     var user = new User()
                    {
                         FirstName = t.User.FirstName,
                         LastName = t.User.LastName,
                         Id = t.User.Id
                    };
                    t.User = user;
                }
                return Json(new { success = true, TimeRegsList = TimeRegs, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting TimeRegs Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteTimeReg(int timeRegId)
        {
            try
            {
                var timeReg = _uow.TimeRegistrationRepo.Find(timeRegId);
                var customerCase = _uow.CustomerCaseRepo.Find(timeReg.CustomerCaseId);

                customerCase.TotalTimeUsed -= (TimeSpan) timeReg.Interval;
                _uow.CustomerCaseRepo.Update(customerCase);
                _uow.TimeRegistrationRepo.Remove(timeRegId);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Deleting Failed" }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult CreateAssignment(CaseAssignment assignment, string EstimatedTimeSpan)
        {
            var oldassignment = assignment;
            try
            {
                if (assignment.AddToCaseEstimate)
                {
                    var customerCase = _uow.CustomerCaseRepo.Find(assignment.CustomerCaseId);
                    customerCase.EstimatedTimeSpan += assignment.EstimatedTimeSpan;
                    _uow.CustomerCaseRepo.Update(customerCase);
                }
                
                _uow.CaseAssignmentRepo.Add(assignment);
                _uow.SaveChanges();
                return Json(new { success = true, responseText = "Opgave blev oprettet." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failure" }, JsonRequestBehavior.AllowGet);
            }

        }
        //[HttpPost]
        //public ActionResult CreateAssignment(CaseAssignmentViewModel viewModel, string EstimatedTimeSpan)
        //{
        //    CaseAssignment assignment = AutoMapper.Mapper.Map<CaseAssignment>(viewModel);
            
        //    if (!ModelState.IsValid)
        //       {
        //           return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
        //       }


        //       try
        //       {
        //           if(!ModelState.IsValid)
        //           {
        //               IEnumerable<string> errorMessages = ViewData.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
        //               return Json(new { success = false, responseText = string.Join(", ", errorMessages) }, JsonRequestBehavior.AllowGet);
        //           }
        //           if (assignment.AddToCaseEstimate)
        //           {
        //               var customerCase = _uow.CustomerCaseRepo.Find(assignment.CustomerCaseId);
        //               customerCase.EstimatedTimeSpan += assignment.EstimatedTimeSpan;
        //               _uow.CustomerCaseRepo.Update(customerCase);
        //           }

        //           _uow.CaseAssignmentRepo.Add(assignment);
        //           _uow.SaveChanges();

        //           return Json(new { success = true, responseText = "success." }, JsonRequestBehavior.AllowGet);
        //       }
        //       catch (Exception e)
        //       {
        //           return Json(new { success = false, responseText = "failure" }, JsonRequestBehavior.AllowGet);
        //       }

        //}

        [HttpGet]
        public ActionResult GetCaseAssignments(int caseId)
        {
            try
            {
                var assignments = _uow.CaseAssignmentRepo.SearchInclude(x => caseId == x.CustomerCaseId, "User").ToList();
                string s = "s";
                return Json(new { success = true, AssignmentsList = assignments, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting Assignments Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteCaseAssignment(int assignmentId)
        {
            try
            {
                var assignment = _uow.CaseAssignmentRepo.Find(assignmentId);
                if (assignment.AddToCaseEstimate)
                {
                    var customerCase = _uow.CustomerCaseRepo.Find(assignment.CustomerCaseId);
                    customerCase.EstimatedTimeSpan -= assignment.EstimatedTimeSpan;
                    _uow.CustomerCaseRepo.Update(customerCase);
                }
                _uow.CaseAssignmentRepo.Remove(assignmentId);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Deleting Failed" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult EndCaseAssignment(int AssignmentId, DateTime EndDateTime)
        {
            try
            {
                var assignment = _uow.CaseAssignmentRepo.Find(AssignmentId);
                assignment.EndDateTime = EndDateTime;
                assignment.Done = true;

                _uow.CaseAssignmentRepo.Update(assignment);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception )
            {
                return Json(new { success = false, responseText = "Ending Failed" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RestartCaseAssignment(int AssignmentId)
        {
            try
            {
                var assignment = _uow.CaseAssignmentRepo.Find(AssignmentId);
                assignment.EndDateTime = null;
                assignment.Done = false;

                _uow.CaseAssignmentRepo.Update(assignment);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, responseText = "Ending Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        //The two parameters are CaseId so we can find the case in the DB and PercentDone which is the percent
        public ActionResult UpdatePercentDone(int caseId, int PercentDone)
        {
            //The customer case is searched for be Entity Framework using .Find(PrimaryKey)
            var customerCase = _uow.CustomerCaseRepo.Find(caseId);
            {
                //Percent is updated
                customerCase.PercentDone = PercentDone;
                try
                {
                    //We try to update the Case using .Update(Object)
                    _uow.CustomerCaseRepo.Update(customerCase);
                    //And save the changes
                    _uow.SaveChanges();
                    //Lastly we return a JSON file where success is true, here we can also put in whole objects
                    // or Lists if we want. The JSON parser will take care of that for us.
                    return Json(new { success = true, responseText = "Updated" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    // incase of error in back end this is sent.
                    return Json(new { success = false, responseText = "failed" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpPost]
        public ActionResult Edit(CustomerCase customerCase)
        {
            try
            {
                if(customerCase.UserId == null) customerCase.UserId = User.Identity.GetUserId();
                //if (!AuthorizationService.AuthorizeRenderHTML("Edit", "CustomerCases"))
                //{
                //    customerCase.Week = null;
                //}
                string s = "";
                _uow.CustomerCaseRepo.Update(customerCase);
                _uow.SaveChanges();
                if (Request.UrlReferrer.AbsolutePath == "/customercases/caseweekindex")
                {
                    return Json(new { success = true, responseText = "Successfully saved changes to case." }, JsonRequestBehavior.AllowGet);
                }

                var res = GetCustomerCaseViewModel(customerCase.CustomerId);
                return View("_CustomerCasesList", res);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TimeRegEdit(TimeRegistration timeReg)
        {
            try
            {
                var oldTimeReg = _uow.TimeRegistrationRepo.Find(timeReg.Id);

                var customerCase = _uow.CustomerCaseRepo.Find(oldTimeReg.CustomerCaseId);
                customerCase.TotalTimeUsed = (TimeSpan) (customerCase.TotalTimeUsed - oldTimeReg.Interval);


                oldTimeReg.Interval = timeReg.EndDateTime - timeReg.StartDateTime;
                oldTimeReg.Description = timeReg.Description;
                oldTimeReg.StartDateTime = timeReg.StartDateTime;
                oldTimeReg.EndDateTime = timeReg.EndDateTime;
                oldTimeReg.CaseAssignmentId = timeReg.CaseAssignmentId;

                customerCase.TotalTimeUsed = (TimeSpan) (customerCase.TotalTimeUsed + oldTimeReg.Interval);


                _uow.CustomerCaseRepo.Update(customerCase);
                _uow.TimeRegistrationRepo.Update(oldTimeReg);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "Success." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AssignmentEdit(CaseAssignment Assignment)
        {
            CaseAssignment old = Assignment;
            try
            {
                var oldAssignment = _uow.CaseAssignmentRepo.Find(Assignment.Id);
                string newCaseEstimate = null;
                if (oldAssignment.AddToCaseEstimate)
                {
                    var customerCase = _uow.CustomerCaseRepo.Find(oldAssignment.CustomerCaseId);
                    customerCase.EstimatedTimeSpan = customerCase.EstimatedTimeSpan - oldAssignment.EstimatedTimeSpan;
                    customerCase.EstimatedTimeSpan = customerCase.EstimatedTimeSpan + Assignment.EstimatedTimeSpan;
                    _uow.CustomerCaseRepo.Update(customerCase);
                    newCaseEstimate = customerCase.EstimatedTimeSpanIsoString;
                }

                oldAssignment.Title = Assignment.Title;
                oldAssignment.UserId = Assignment.UserId;
                oldAssignment.StartDateTime = Assignment.StartDateTime;
                oldAssignment.Deadline = Assignment.Deadline;//Deadline
                oldAssignment.LinkedCaseAssignmentId = Assignment.LinkedCaseAssignmentId;//LinkedCaseAssignmentId
                oldAssignment.EstimatedTimeSpan = Assignment.EstimatedTimeSpan;//EstimatedTimeIsoString
                oldAssignment.Description = Assignment.Description;
                oldAssignment.EndDateTime = Assignment.EndDateTime;


                _uow.CaseAssignmentRepo.Update(oldAssignment);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "Success.", newCaseEstimate }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCasePeople(int customerId)
        {
            var contacts = _uow.CustomerContactsRepo.Search(x => x.CustomerId == customerId).Select(x => new { x.Id, x.Name }).ToList();
            var users = _userManager.Users.Select(x => new { x.Id, x.FirstName, x.LastName });
            var caseTypes = _uow.CustomerCaseTypeRepo.GetAll().ToList();
            return Json(new { contactsList = contacts, UsersList = users, CaseTypesList = caseTypes }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUsersNames()
        {
            try
            {
                var Users = _userManager.Users
                    .Where(x => x.IsSuperAdmin == false)
                    .Select(x => new { x.Id, x.FirstName, x.LastName });
                return Json(new { Users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public ActionResult GetDetailed(int caseId)
        {
            try
            {
                var Case1 = _uow.CustomerCaseRepo.SearchInclude(x => x.Id == caseId, "Contact").ToList().Find(x => x.Id == caseId);
                return Json(new {

                    Id = Case1.Id,
                    Ended = Case1.EndDateTime?.ToString("dd/MM/yyyy"),
                    contact = Case1.Contact,
                    Started = Case1.StartDateTime?.ToString("dd/MM/yyyy"),
                    Description = Case1.Description,
                    Done = Case1.PercentDone,
                    CustomerId = Case1.CustomerId

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public ActionResult GetTimeReg(int TimeRegId)
        {
            try
            {
                var TimeReg = _uow.TimeRegistrationRepo.Find(TimeRegId);

                return Json(new { success = true, TimeReg, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting Time Registration Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAssignment(int AssignmentId)
        {
            try
            {
                var Assignment = _uow.CaseAssignmentRepo.Find(AssignmentId);

                return Json(new { success = true, Assignment = Assignment, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting Case Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCase(int caseId)
        {
            try
            {
                var Case = _uow.CustomerCaseRepo.Find(caseId);
                return Json(new { success = true, Case = Case, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting Case Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PinToggle(int caseId)
        {
            var customerCase = _uow.CustomerCaseRepo.Find(caseId);
            if (customerCase.Pinned)
            {
                customerCase.Pinned = false;

                try
                {
                    _uow.CustomerCaseRepo.Update(customerCase);
                    _uow.SaveChanges();

                    return Json(new { success = true, responseText = "Case was unpinned" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, responseText = "failed" }, JsonRequestBehavior.AllowGet);
                }

            }
            customerCase.Pinned = true;

            try
            {
                _uow.CustomerCaseRepo.Update(customerCase);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "Case was pinned" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult FilterAndSearch(CustomerCaseDatatableViewModel customerCaseViewModel)
        {
            
            var count = string.IsNullOrEmpty(customerCaseViewModel.SearchKey)
                ? _uow.CustomerCaseRepo.GetAll().Count(x => x.CustomerId == customerCaseViewModel.CustomerId)
                : _uow.CustomerCaseRepo.SearchInclude(x => x.CustomerId == customerCaseViewModel.CustomerId &&
                                                           x.Titel.Contains(customerCaseViewModel.SearchKey) ||
                                                           x.User.FirstName.Contains(customerCaseViewModel.SearchKey) ||
                                                           x.User.LastName.Contains(customerCaseViewModel.SearchKey) ||
                                                           x.StartDateTime.Value.Day.ToString().Contains(customerCaseViewModel.SearchKey) ||
                                                           x.StartDateTime.Value.Month.ToString().Contains(customerCaseViewModel.SearchKey) ||
                                                           x.StartDateTime.Value.Year.ToString().Contains(customerCaseViewModel.SearchKey) ||
                                                           x.EndDateTime.Value.Day.ToString().Contains(customerCaseViewModel.SearchKey) ||
                                                           x.EndDateTime.Value.Month.ToString().Contains(customerCaseViewModel.SearchKey) ||
                                                           x.EndDateTime.Value.Year.ToString().Contains(customerCaseViewModel.SearchKey), "User").Count();

            var query = string.IsNullOrEmpty(customerCaseViewModel.SearchKey)
                ? _uow.CustomerCaseRepo.GetAllPagination(x =>
                        x.CustomerId == customerCaseViewModel.CustomerId,
                    customerCaseViewModel.PageNumber - 1,
                    customerCaseViewModel.PageSize,
                    customerCaseViewModel.OrderBy,
                    customerCaseViewModel.Direction,
                    "User")
                : _uow.CustomerCaseRepo.GetAllPagination(x =>
                        x.CustomerId == customerCaseViewModel.CustomerId &&
                        x.Titel.Contains(customerCaseViewModel.SearchKey) ||
                        x.User.FirstName.Contains(customerCaseViewModel.SearchKey) ||
                        x.User.LastName.Contains(customerCaseViewModel.SearchKey) ||
                        x.StartDateTime.Value.Day.ToString().Contains(customerCaseViewModel.SearchKey) ||
                        x.StartDateTime.Value.Month.ToString().Contains(customerCaseViewModel.SearchKey) ||
                        x.StartDateTime.Value.Year.ToString().Contains(customerCaseViewModel.SearchKey) ||
                        x.EndDateTime.Value.Day.ToString().Contains(customerCaseViewModel.SearchKey) ||
                        x.EndDateTime.Value.Month.ToString().Contains(customerCaseViewModel.SearchKey) ||
                        x.EndDateTime.Value.Year.ToString().Contains(customerCaseViewModel.SearchKey),
                    customerCaseViewModel.PageNumber - 1,
                    customerCaseViewModel.PageSize,
                    customerCaseViewModel.OrderBy,
                    customerCaseViewModel.Direction,
                    "User");
            customerCaseViewModel.CustomerCases = new StaticPagedList<CustomerCase>(query.ToList(), customerCaseViewModel.PageNumber, customerCaseViewModel.PageSize, count);
            customerCaseViewModel.CurrentUserID = User.Identity.GetUserId();
            customerCaseViewModel.QueryCount = //count;
            customerCaseViewModel.TableCount = query.Count;
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerCasesList", customerCaseViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_CustomerCasePagination", customerCaseViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CasesForWeek(string offset, int? caseType)
        {
            try
            {
                bool isOffset = int.TryParse(offset, out int intOffset);
                IEnumerable<CustomerCase> list = null;

                if (isOffset)
                {
                    DateTime now = DateTime.Today;
                    double daysOffset = intOffset * 7;
                    string Iso8601WeekOfYear = 
                        now.AddDays(daysOffset).GetWeek() + "/" + now.Year;
                    list = _uow.CustomerCaseRepo
                        .SearchInclude(x => x.Week == Iso8601WeekOfYear, "User", "ProjectResponsible", "Customer")
                        .ToList();

                } else
                {
                    switch (offset.Replace("_", "").ToLower())
                    {
                        case "ikkeplanlagt":
                        case "pending":
                            list = _uow.CustomerCaseRepo
                                .SearchInclude(x => x.Status == CaseStatus.IkkePlanlagt, "User", "ProjectResponsible", "Customer")
                                .ToList();
                            break;
                        default:
                            throw new ArgumentException("Invalid offset");
                    }
                    
                }
                if (caseType.HasValue && caseType.Value > 0)
                {
                    list = list.Where(x => x.CustomerCaseTypeId == caseType.Value);
                }
                foreach (var c in list)
                {
                    var oldUser = c.User;
                    var oldProjectResponsible = c.ProjectResponsible;

                    c.User = new User()
                    {
                        FirstName = oldUser.FirstName,
                        LastName = oldUser.LastName,
                        Id = oldUser.Id
                    };
                    c.ProjectResponsible = new User()
                    {
                        FirstName = oldProjectResponsible.FirstName,
                        LastName = oldProjectResponsible.LastName,
                        Id = oldProjectResponsible.Id
                    };
                };

                JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                var data = JsonConvert.SerializeObject(new { data = list, success = true, responseText = "success" }, Formatting.Indented, jss);

                return Content(data, "application/json");
            }
            catch (ArgumentException ae)
            {
                logger.Debug(ae.Message + "\n\n" + ae.InnerException);
                return Json(new { success = false, responseText = ae.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Debug(e.Message + "\n\n" + e.InnerException);
                return Json(new { success = false, responseText = "Getting Case Failed" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}