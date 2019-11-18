using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using CRM.Application.Core.ViewModels;
using CRM.Web.Helpers;
using CRM.Application.Core.Services;
using CRM.Models;
using CRM.DAL;
using CRM.Application.Core.Enums;
using CRM.Identity;
using X.PagedList;
using S22.Imap;

namespace CRM.Web.Controllers
{
    //[CRMAuthorize] something with this attribute. need fixing.
    public class EmailMessagesController : Controller
    {
        private readonly EmailAccountsController accountController = new EmailAccountsController();
        private readonly UnitofWork _uow = new UnitofWork();
        private EmailMessageViewModel _emailMessageViewModel = new EmailMessageViewModel();
        private EmailMessageViewModel _emailCorrespondenceViewModel = new EmailMessageViewModel();
        // GET: EmailMessage
        //private void setEmailCorrespondenceTableCurrentPageCurrentCorrespondence(int? emailMessId, int? currentPageNumber)
        //{
        //    var custId = _emailMessageViewModel.CustomerId;
        //    HttpCookie emailCorrespondenceCookie = Request.Cookies["EmailCorresponence"+ custId + "Table"];
        //    if (emailCorrespondenceCookie == null)
        //    {
        //        HttpCookie cookie = new HttpCookie("EmailCorresponence" + custId + "Table");
        //        DateTime now = DateTime.Now;
        //        cookie.Expires = now.AddYears(50);
        //        if (emailMessId != null)
        //            cookie.Values.Add("emailMessId", emailMessId.Value.ToString());
        //        if (currentPageNumber != null)
        //            cookie.Values.Add("pageNumber", currentPageNumber.ToString());
        //        Response.Cookies.Add(cookie);
        //    }
        //    else
        //    {
        //        if (emailMessId != null)
        //            emailCorrespondenceCookie.Values["emailMessId"] = emailMessId.Value.ToString();
        //        if (currentPageNumber != null)
        //            emailCorrespondenceCookie.Values["pageNumber"] = currentPageNumber.ToString();
        //        Response.Cookies.Add(emailCorrespondenceCookie);
        //    }
        //}

        //public void FetchMessages(int customerId, ICollection<EmailAccount> fetchFrom = null)
        //{
        //    var customer = _uow.CustomersRepo.Find(customerId);
        //    var contacts = _uow.CustomerContactsRepo.Search(x => x.CustomerId == customerId).ToList();
        //    if (fetchFrom == null)
        //        fetchFrom = _uow.EmailAccountsRepo.GetAll();
        //    foreach (var account in fetchFrom)
        //    {
        //        _emailClient.Connect(account.HostName, account.HostPort);
        //        _emailClient.Authenticate(account.UserName, account.Password);
        //        if (!_emailClient.IsConnected || !_emailClient.IsAuthenticated)
        //        {
        //            //do some error logging
        //            continue;
        //        }
        //        _emailClient.Authenticate(account.UserName, account.Password);

        //        var qbuilder = new IMAPQueryBuilder(account.FullAddress, _emailClient.Messages);
        //        var queryResult = qbuilder.getCustomerQuery(customer, outgoing: true)
        //                   .Concat(qbuilder.getCustomerQuery(customer, outgoing: false))
        //                   .Concat(qbuilder.getCustomerQuery(contacts, outgoing: true))
        //                   .Concat(qbuilder.getCustomerQuery(contacts, outgoing: false))
        //                   .OrderBy(email => email.DateSent)
        //                   .ToList();

        //        //partial fetch when latestSync has a value
        //        //if (account.newestUidSynced == 0)
        //        //    customerCorrespondence = customerCorrespondence.TakeWhile(m => newByDate(m));
        //        //else
        //        //    customerCorrespondence = customerCorrespondence.TakeWhile(m => newByDate(m) && newByUid(m));
        //        _uow.EmailMessagesRepo.AddRange(queryResult);

        //    }
        //    _uow.SaveChanges();

        private void FetchAllMessages()
        {
            var emailaccounts = _uow.EmailAccountsRepo.GetAll();
            foreach (var account in emailaccounts)
            {
                ImapClient imapClient = null;
                try
                {
                    if (account.HostPort == 143 || account.HostPort == 993)
                    {
                        imapClient = new ImapClient(account.HostName, account.HostPort);
                        imapClient.Login(account.UserName, account.Password, AuthMethod.Auto);
                    }
                    else throw new Exception();
                }
                catch (Exception) { continue; }
                foreach (var customer in _uow.CustomersRepo.GetAll())
                {
                    var contactEmails = _uow.CustomerContactsRepo.Search(x => x.CustomerId == customer.Id).Select(cc => cc.Email).ToList();
                    var anyEmail = new List<string>(contactEmails);
                        anyEmail.Add(customer.Email);
                        anyEmail.Add(account.FullAddress);

                    var incoming = SearchCondition.From(customer.Email);
                    foreach (var emailAddr in contactEmails)
                        incoming = incoming.Or(SearchCondition.From(emailAddr));
                    incoming = incoming.And(SearchCondition.To(account.FullAddress));

                    var outgoing = SearchCondition.To(customer.Email);
                    foreach (var emailAddr in contactEmails)
                        outgoing = outgoing.Or(SearchCondition.To(emailAddr));
                    outgoing = outgoing.And(SearchCondition.From(account.FullAddress));

                    var uids = imapClient.Search(incoming.Or(outgoing));
                    var result = new List<EmailMessage>();

                    foreach (uint uid in uids)
                    {
                        var m = imapClient.GetMessage(uid);
                        result.Add(new EmailMessage
                        {
                            MailBox_Uid = uid,
                            Subject = m.Subject,
                            CustomerId = customer.Id,
                            CustomerCompanyName = customer.CompanyName,
                            EmailAccountId = account.Id,
                            Body = m.Body,
                            Recipients = string.Join(", ", m.To.Select(r => r.Address).Intersect(anyEmail)),
                            Sender = m.From.Address,
                            DateSent = m.Date()
                        });
                    }
                    //todo: test wether or not AsParallel would benefit perfomance

                    _uow.EmailMessagesRepo.AddRange(result);
                }
                _uow.SaveChanges();
            }
        }
        public ActionResult SyncAllAccounts()
        {
            FetchAllMessages();
            return Redirect(Request.UrlReferrer.ToString());
        }
        public ActionResult CustomerEmailCorrespondenceIndex(int customerId)
        {
            _emailCorrespondenceViewModel = new EmailMessageViewModel { CustomerId = customerId };

            _emailCorrespondenceViewModel.DefaultOrderBy = "DateSent";
            _emailCorrespondenceViewModel.DefaultDirection = "DESC";
            _emailCorrespondenceViewModel.Direction = "DESC";

            var correspondenceResult = _uow.EmailMessagesRepo.DynamicTable(
                _emailCorrespondenceViewModel.PageSize,
                _emailCorrespondenceViewModel.PageNumber,
                string.Empty,
                _emailCorrespondenceViewModel.DefaultOrderBy,
                _emailCorrespondenceViewModel.DefaultDirection,
                message => message.CustomerId == customerId);



            //this is where ordering goes wrong
            var emailCorrespondenceIPagedList = new StaticPagedList<EmailMessage>(
                correspondenceResult.QueryResultList,
                _emailCorrespondenceViewModel.PageNumber,
                _emailCorrespondenceViewModel.PageSize,
                correspondenceResult.TableCount
                );

            _emailCorrespondenceViewModel.EmailMessagesPL = emailCorrespondenceIPagedList;
            //_emailCorrespondenceViewModel.TableCount is the x in "showing 10 out of x" in _EmailMessagePagination
            _emailCorrespondenceViewModel.TableCount = correspondenceResult.QueryCount;
            _emailCorrespondenceViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();

            //setEmailCorrespondenceTableCurrentPageCurrentCorrespondence(null, emailCorrespondenceIPagedList.PageNumber);
            return View("_CustomerEmailCorrespondence", _emailCorrespondenceViewModel);
        }
        public ActionResult Index()
        {
            throw new NotImplementedException();
            var emailCorrespondenceCookie = Request.Cookies["EmailMessageTable"];
            int pageNumber = 0;
            if (emailCorrespondenceCookie != null && emailCorrespondenceCookie.Value != null && !string.IsNullOrEmpty(emailCorrespondenceCookie.Values["pageNumber"].ToString()))
                pageNumber = int.Parse(emailCorrespondenceCookie.Values["pageNumber"].ToString());
            else
                pageNumber = _emailMessageViewModel.PageNumber;

            _emailMessageViewModel.DefaultOrderBy = "DateSent";

            var correspondenceResult = _uow.EmailMessagesRepo.DynamicTable(
               _emailMessageViewModel.PageSize,
                pageNumber - 1,
               string.Empty,
               _emailMessageViewModel.DefaultOrderBy,
               _emailMessageViewModel.DefaultDirection,
               null);

            var emailCorrespondenceIPagedList = new StaticPagedList<EmailMessage>(
                correspondenceResult.QueryResultList,
                pageNumber,
                _emailMessageViewModel.PageSize,
                correspondenceResult.TableCount);

            _emailMessageViewModel.EmailMessagesPL = emailCorrespondenceIPagedList;
            _emailMessageViewModel.TableCount = correspondenceResult.TableCount;
            _emailMessageViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();

            return View(_emailMessageViewModel);
        }
        [HttpPost]
        public ActionResult DynamicCorrespondenceTable(EmailMessageViewModel emailMessageViewModel)
        {
            //make automapper profile?
            TableFilterSortPagingService<EmailMessage, EmailMessageViewModel> tableFilter = new TableFilterSortPagingService<EmailMessage, EmailMessageViewModel>();
            GenericViewModel<EmailMessageViewModel, EmailMessage> genericViewModel = new GenericViewModel<EmailMessageViewModel, EmailMessage>()
            {
                QueryParameters = emailMessageViewModel.QueryParameters,
                PageNumber = emailMessageViewModel.PageNumber,
                PageSize = emailMessageViewModel.PageSize,
                OrderBy = emailMessageViewModel.OrderBy,
                DefaultOrderBy = "DateSent",
                DefaultDirection = "DESC",
                Direction = emailMessageViewModel.Direction,
                QueryOperatorComparer = emailMessageViewModel.QueryOperatorComparer,
            };
            // _emailCorrespondenceViewModel.CustomerId = emailMessageViewModel;
            genericViewModel.QueryParameters.Add(
                new ExpressionBuilderParameters {
                    SearchKey = "CustomerId",
                    Operator = OperatorComparer.Equals.ToString(),
                    Value = emailMessageViewModel.CustomerId.ToString()
                });
            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _emailCorrespondenceViewModel);

            _emailCorrespondenceViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _emailCorrespondenceViewModel.Direction = tableFilterResult.Direction;
            _emailCorrespondenceViewModel.OrderBy = tableFilterResult.OrderBy;
            _emailCorrespondenceViewModel.PageNumber = tableFilterResult.PageNumber;
            _emailCorrespondenceViewModel.QueryCount = tableFilterResult.QueryCount;
            _emailCorrespondenceViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _emailCorrespondenceViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _emailCorrespondenceViewModel.EmailMessagesPL = tableFilterResult.ResultList;
            _emailCorrespondenceViewModel.TableCount = tableFilterResult.TableCount;

            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_EmailMessageList", _emailCorrespondenceViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_EmailMessagePagination", _emailCorrespondenceViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = tableFilterResult.PageNumber,
                RowsFrom = tableFilterResult.ResultList.FirstItemOnPage,
                RowsTo = tableFilterResult.ResultList.LastItemOnPage,
                totalCount = emailMessageViewModel.QueryParameters != null ? tableFilterResult.QueryCount : tableFilterResult.TableCount
            }, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public ActionResult DynamicTable(EmailMessageViewModel emailMessageViewModel)
        //{
        //    //make automapper profile?
        //    TableFilterSortPagingService<EmailMessage, EmailMessageViewModel> tableFilter = new TableFilterSortPagingService<EmailMessage, EmailMessageViewModel>();
        //    GenericViewModel<EmailMessageViewModel, EmailMessage> genericViewModel = new GenericViewModel<EmailMessageViewModel, EmailMessage>()
        //    {
        //        QueryParameters = emailMessageViewModel.QueryParameters,
        //        PageNumber = emailMessageViewModel.PageNumber,
        //        PageSize = emailMessageViewModel.PageSize,
        //        OrderBy = emailMessageViewModel.OrderBy,
        //        DefaultOrderBy = emailMessageViewModel.DefaultOrderBy,
        //        DefaultDirection = emailMessageViewModel.DefaultDirection,
        //        Direction = emailMessageViewModel.Direction,
        //        QueryOperatorComparer = emailMessageViewModel.QueryOperatorComparer,
        //    };

        //    var tableFilterResult = tableFilter.FilterTable(genericViewModel, _emailMessageViewModel);

        //    _emailMessageViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
        //    _emailMessageViewModel.Direction = tableFilterResult.Direction;
        //    _emailMessageViewModel.OrderBy = tableFilterResult.OrderBy;
        //    _emailMessageViewModel.PageNumber = tableFilterResult.PageNumber;
        //    _emailMessageViewModel.QueryCount = tableFilterResult.QueryCount;
        //    _emailMessageViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
        //    _emailMessageViewModel.QueryParameters = tableFilterResult.QueryParameters;
        //    _emailMessageViewModel.EmailMessagesPL = tableFilterResult.ResultList;
        //    _emailMessageViewModel.TableCount = tableFilterResult.TableCount;

        //    var tablePartial = RenderRazorView.RenderRazorViewToString(ControllerContext, "_EmailMessageList", _emailMessageViewModel);
        //    var pagingPartial = RenderRazorView.RenderRazorViewToString(ControllerContext, "_EmailMessagePagination", _emailMessageViewModel);
        //    return Json(new
        //    {
        //        tablePartial = tablePartial,
        //        pagingPartial = pagingPartial,
        //        pageNumber = tableFilterResult.PageNumber,
        //        RowsFrom = tableFilterResult.ResultList.FirstItemOnPage,
        //        RowsTo = tableFilterResult.ResultList.LastItemOnPage,
        //        totalCount = emailMessageViewModel.QueryParameters != null ? tableFilterResult.QueryCount : tableFilterResult.TableCount
        //    }, JsonRequestBehavior.AllowGet);
        //}//for list of all messages across all customers
    }
}