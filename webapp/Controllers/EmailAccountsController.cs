using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using CRM.Application.Core.Enums;
using CRM.Application.Core.Services;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using CRM.Web.Helpers;
using S22.Imap;

namespace CRM.Web.Controllers
{
    [CRMAuthorize]
    public class EmailAccountsController : Controller
    {
        //private IImapClient _client;
        //https://tools.ietf.org/html/rfc3501#section-3
        private UnitofWork _uow = new UnitofWork();
        private EmailAccountViewModel _emailAccountViewModel = new EmailAccountViewModel();
        private void SetEmailAccountsTableCurrentPageCurrentEmailAccount(int? emailaccountId, int? currentPageNumber)
        {
            HttpCookie emailacccountTableCookie = Request.Cookies["EmailAccountTable"];
            if (emailacccountTableCookie == null)
            {
                HttpCookie cookie = new HttpCookie("EmailAccountTable");
                DateTime now = DateTime.Now;
                cookie.Expires = now.AddYears(50);
                if (emailaccountId != null)
                    cookie.Values.Add("emailaccountId", emailaccountId.Value.ToString());
                if (currentPageNumber != null)
                    cookie.Values.Add("pageNumber", currentPageNumber.ToString());
                Response.Cookies.Add(cookie);
            }
            else
            {
                if (emailaccountId != null)
                    emailacccountTableCookie.Values["emailaccountId"] = emailaccountId.Value.ToString();
                if (currentPageNumber != null)
                    emailacccountTableCookie.Values["pageNumber"] = currentPageNumber.ToString();
                Response.Cookies.Add(emailacccountTableCookie);
            }
        }
        private void updateEmailAccount(EmailAccountViewModel vm)
        {
            var emailAccount = Mapper.Map<EmailAccount>(vm);
            _uow.EmailAccountsRepo.Update(emailAccount);
        }

        // GET: EmailAccounts
        public ViewResult Index()
        {
            var emailAccountsCookie = Request.Cookies["EmailAccountsTable"];
            int pageNumber = 0;
            if (emailAccountsCookie != null && emailAccountsCookie.Value != null && !string.IsNullOrEmpty(emailAccountsCookie.Values["pageNumber"].ToString()))
            {
                pageNumber = int.Parse(emailAccountsCookie.Values["pageNumber"].ToString());
            }
            else
            {
                pageNumber = _emailAccountViewModel.PageNumber;
            }
            _emailAccountViewModel.DefaultOrderBy = "HostName";
            var emailAccountsresult = _uow.EmailAccountsRepo.DynamicTable(
                _emailAccountViewModel.PageSize,
                 pageNumber - 1,
                string.Empty,
                _emailAccountViewModel.DefaultOrderBy,
                _emailAccountViewModel.DefaultDirection,
                null);

            var emailAccountsIPagedList = new X.PagedList.StaticPagedList<EmailAccount>(
                emailAccountsresult.QueryResultList,
                pageNumber,
                _emailAccountViewModel.PageSize,
                emailAccountsresult.TableCount);


            _emailAccountViewModel.EmailAccountsPL = emailAccountsIPagedList;
            _emailAccountViewModel.TableCount = emailAccountsresult.TableCount;
            _emailAccountViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();
            SetEmailAccountsTableCurrentPageCurrentEmailAccount(null, emailAccountsIPagedList.PageNumber);
            return View(_emailAccountViewModel);
        }
        public void Delete(int id)
        {
            _uow.EmailAccountsRepo.Remove(id);
            _uow.SaveChanges();
            //return RedirectToAction("Index");
        }
        public ActionResult Add()
        {
            _emailAccountViewModel = new EmailAccountViewModel { ProtocolOptions = _uow.EmailProtocolsRepo.GetAll() };
            return View(_emailAccountViewModel);
        }
        [HttpPost]
        public ActionResult Add(EmailAccountViewModel vm)
        {
            if (ModelState.IsValid)
            {
                if (vm.Id == 0)
                {
                    addAccount(vm);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Email.Email.EmailAccountSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                    _emailAccountViewModel = new EmailAccountViewModel() { Id = 0 };
                }
                else
                {
                    updateEmailAccount(vm);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Email.Email.EmailAccountUpdated;
                    ReponseViewModel.TransactionType = TransactionType.Update.ToString();
                }
                try
                {
                    _uow.SaveChanges();
                    return Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    throw;
                }
            }
            return View();
        }
        private void addAccount(EmailAccountViewModel vm)
        {
            EmailAccount acc = AutoMapper.Mapper.Map<EmailAccount>(vm);
            _uow.EmailAccountsRepo.Add(acc);
        }
        public ActionResult Edit(int id, int latestEmailAccountsPageNumber)
        {
            EmailAccount acc = _uow.EmailAccountsRepo.Find(id);
            _emailAccountViewModel = AutoMapper.Mapper.Map<EmailAccountViewModel>(acc);
            //latestCustomersPageNumber is not used. Should probably be passed instead of null
            SetEmailAccountsTableCurrentPageCurrentEmailAccount(id, null);
            return View("Add", _emailAccountViewModel);
        }
        [HttpPost]
        public ActionResult DynamicTable(EmailAccountViewModel emailAccountViewModel)
        {

            TableFilterSortPagingService<EmailAccount, EmailAccountViewModel> tableFilter = new TableFilterSortPagingService<EmailAccount, EmailAccountViewModel>();
            var genericVM = new GenericViewModel<EmailAccountViewModel, EmailAccount> 
            {
                QueryParameters = emailAccountViewModel.QueryParameters,
                PageNumber = emailAccountViewModel.PageNumber,
                PageSize = emailAccountViewModel.PageSize,
                OrderBy = emailAccountViewModel.OrderBy,
                DefaultOrderBy = emailAccountViewModel.DefaultOrderBy,
                Direction = emailAccountViewModel.Direction,
                QueryOperatorComparer = emailAccountViewModel.QueryOperatorComparer
            };

            genericVM.QueryParameters = emailAccountViewModel.QueryParameters;
            genericVM.PageNumber = emailAccountViewModel.PageNumber;
            genericVM.PageSize = emailAccountViewModel.PageSize;
            genericVM.OrderBy = emailAccountViewModel.OrderBy;
            genericVM.DefaultOrderBy = emailAccountViewModel.DefaultOrderBy;
            genericVM.DefaultDirection = emailAccountViewModel.DefaultDirection;
            genericVM.Direction = emailAccountViewModel.Direction;
            genericVM.QueryOperatorComparer = emailAccountViewModel.QueryOperatorComparer;

            var tableFilterResult = tableFilter.FilterTable(genericVM, _emailAccountViewModel);

            _emailAccountViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _emailAccountViewModel.Direction = tableFilterResult.Direction;
            _emailAccountViewModel.OrderBy = tableFilterResult.OrderBy;
            _emailAccountViewModel.PageNumber = tableFilterResult.PageNumber;
            _emailAccountViewModel.QueryCount = tableFilterResult.QueryCount;
            _emailAccountViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _emailAccountViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _emailAccountViewModel.EmailAccountsPL = tableFilterResult.ResultList;
            _emailAccountViewModel.TableCount = tableFilterResult.TableCount;
            SetEmailAccountsTableCurrentPageCurrentEmailAccount(null, tableFilterResult.PageNumber);
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_EmailAccountList", _emailAccountViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_EmailAccountPagination", _emailAccountViewModel);
            return Json(new
                {
                    tablePartial = tablePartial,
                    pagingPartial = pagingPartial,
                    pageNumber = tableFilterResult.PageNumber,
                    RowsFrom = tableFilterResult.ResultList.FirstItemOnPage,
                    RowsTo = tableFilterResult.ResultList.LastItemOnPage,
                    totalCount = emailAccountViewModel.QueryParameters != null ? tableFilterResult.QueryCount : tableFilterResult.TableCount
                }, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult ValidAccount(string hostName, int hostPort, string userName, string password)
        //{
        //    var imapClient = new ImapClient();
        //    bool valid = false;
        //    try
        //    {
        //        imapClient.Connect(hostName, hostPort);
        //        imapClient.Authenticate(userName, password);

        //        valid = imapClient.IsConnected && imapClient.IsAuthenticated;
        //    }
        //    catch (ArgumentOutOfRangeException)
        //    {
                
        //    }
        //    catch (ArgumentNullException)
        //    {

        //    }
        //    catch (IOException)
        //    {

        //    }
        //    catch (SocketException)
        //    {

        //    }
        //    catch (AuthenticationException)
        //    {

        //    }
        //    finally
        //    {
        //        imapClient.Disconnect();
        //        imapClient.Dispose();
        //    }
        //    return Json(valid, JsonRequestBehavior.AllowGet);

        //}
    }
}