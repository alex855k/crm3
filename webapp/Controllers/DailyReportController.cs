using CRM.Application.Core.Services;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

namespace CRM.Web.Controllers
{
    public class DailyReportController : Controller
    {
        DailyReportViewModel _dailyReportViewModel = new DailyReportViewModel();
        UnitofWork _uow = new UnitofWork();
        public UserManager UserManager => System.Web.HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
        [CRMAuthorize]
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            DailyReportService dailyReportService = new DailyReportService();
            dailyReportService.CreateDailyReport();
            _dailyReportViewModel.DefaultOrderBy = "Date";
            _dailyReportViewModel.DefaultDirection = "Desc";
            _dailyReportViewModel.QueryParameters = new List<ExpressionBuilderParameters> {
                new ExpressionBuilderParameters {SearchKey="UserId",Operator="Equals",Value=currentUserId}
            };
            var dailyReportResult = DailyReportDynamicTable(_dailyReportViewModel);
            if (AuthorizationService.AuthorizeRenderHTML("DailyReportUserSelection", "DailyReport"))
            {
                dailyReportResult.UsersList = UserManager.Users.ToList();
                dailyReportResult.SelectedUserId = currentUserId;
            }

            return View(dailyReportResult);
        }

        public ActionResult TodayNotesReport(int id, DateTime date)
        {
            var currentUserId = User.Identity.GetUserId();
            var dailyReport = _uow.DailyReportsRepo.Find(id);
            _dailyReportViewModel.Id = dailyReport.Id;
            _dailyReportViewModel.Date = dailyReport.Date.Date;
            _dailyReportViewModel.KmFrom = dailyReport.KmFrom;
            _dailyReportViewModel.KmTo = dailyReport.KmTo;
            _dailyReportViewModel.CustomerNotesList = _uow.CustomerNotesRepo.Search(x =>
            DbFunctions.TruncateTime(x.CreationDate) == DbFunctions.TruncateTime(date) &&
            x.UserId == currentUserId).ToList();
            return View(_dailyReportViewModel);
        }

        public ActionResult TodayNotesReportByDate(string date, bool prevDate)
        {
            var currentUserId = User.Identity.GetUserId();
            DateTime targetDailyReportsDate = DateTime.ParseExact(date, "dd/MM/yyyy",CultureInfo.CurrentCulture);
            if (prevDate)
            {
                targetDailyReportsDate = targetDailyReportsDate.AddDays(-1);
            }
            var dailyReport = _uow.DailyReportsRepo
                .Search(x => DbFunctions.TruncateTime(x.Date) == DbFunctions.TruncateTime(targetDailyReportsDate) && x.UserId == currentUserId)
                .SingleOrDefault();
            if (dailyReport != null)
            {
                _dailyReportViewModel.Id = dailyReport.Id;
                _dailyReportViewModel.Date = dailyReport.Date;
                _dailyReportViewModel.KmFrom = dailyReport.KmFrom;
                _dailyReportViewModel.KmTo = dailyReport.KmTo;
                _dailyReportViewModel.CustomerNotesList = _uow.CustomerNotesRepo.Search(x =>
                DbFunctions.TruncateTime(x.CreationDate) == DbFunctions.TruncateTime(targetDailyReportsDate) &&
                x.UserId == currentUserId).ToList();
            }
            else
            {
                _dailyReportViewModel.Date = targetDailyReportsDate;
            }
            return View("TodayNotesReport", _dailyReportViewModel);
        }
        public ActionResult UpdateDailyReport(DailyReportViewModel dailyReportViewModel)
        {
            var dailyReport = _uow.DailyReportsRepo.Find(dailyReportViewModel.Id);
            dailyReport.KmFrom = dailyReportViewModel.KmFrom;
            dailyReport.KmTo = dailyReportViewModel.KmTo;
            _uow.DailyReportsRepo.Update(dailyReport);
            _uow.SaveChanges();
            _dailyReportViewModel.DefaultOrderBy = "Date";
            _dailyReportViewModel.DefaultDirection = "Desc";
            var dailyReportResult = DailyReportDynamicTable(_dailyReportViewModel);
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_DailyReportList", dailyReportResult);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_DailyReportPagination", dailyReportResult);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = dailyReportResult.PageNumber,
                RowsFrom = dailyReportResult.DailyReportList.FirstItemOnPage,
                RowsTo = dailyReportResult.DailyReportList.LastItemOnPage,
                totalCount = dailyReportViewModel.QueryParameters != null ? dailyReportResult.QueryCount : dailyReportResult.TableCount
            }, JsonRequestBehavior.AllowGet);
        }
        public DailyReportViewModel DailyReportDynamicTable(DailyReportViewModel dailyReportViewModel)
        {
            TableFilterSortPagingService<DailyReport, DailyReportViewModel> tableFilter = new TableFilterSortPagingService<DailyReport, DailyReportViewModel>();
            GenericViewModel<DailyReportViewModel, DailyReport> genericViewModel = new GenericViewModel<DailyReportViewModel, DailyReport>();
            genericViewModel.QueryParameters = dailyReportViewModel.QueryParameters;
            genericViewModel.PageNumber = dailyReportViewModel.PageNumber;
            genericViewModel.PageSize = dailyReportViewModel.PageSize;
            genericViewModel.OrderBy = dailyReportViewModel.OrderBy;
            genericViewModel.DefaultOrderBy = dailyReportViewModel.DefaultOrderBy;
            genericViewModel.DefaultDirection = dailyReportViewModel.DefaultDirection;
            genericViewModel.Direction = dailyReportViewModel.Direction;
            genericViewModel.QueryOperatorComparer = dailyReportViewModel.QueryOperatorComparer;

            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _dailyReportViewModel);

            _dailyReportViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _dailyReportViewModel.Direction = tableFilterResult.Direction;
            _dailyReportViewModel.OrderBy = tableFilterResult.OrderBy;
            _dailyReportViewModel.PageNumber = tableFilterResult.PageNumber;
            _dailyReportViewModel.QueryCount = tableFilterResult.QueryCount;
            _dailyReportViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _dailyReportViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _dailyReportViewModel.DailyReportList = tableFilterResult.ResultList;
            _dailyReportViewModel.TableCount = tableFilterResult.TableCount;
            return _dailyReportViewModel;
        }
        public ActionResult SearchDailyReport(DailyReportViewModel dailyReportViewModel)
        {
            string currentUserId = string.Empty;
            if (!string.IsNullOrEmpty(dailyReportViewModel.SelectedUserId))
                currentUserId = dailyReportViewModel.SelectedUserId;
            else
                currentUserId = User.Identity.GetUserId();

            List<DailyReport> dailyReportResult = null;

            if (!string.IsNullOrEmpty(dailyReportViewModel.SearchKey))
            {
                dailyReportResult = _uow.DailyReportsRepo.Search(x =>
                 (x.Date.Day.ToString().Equals(dailyReportViewModel.SearchKey.ToString()) ||
                 x.Date.Month.ToString().Equals(dailyReportViewModel.SearchKey.ToString()) ||
                 x.Date.Year.ToString().Equals(dailyReportViewModel.SearchKey.ToString()) ||
                 x.KmFrom.ToString().Contains(dailyReportViewModel.SearchKey) ||
                 x.KmTo.ToString().Contains(dailyReportViewModel.SearchKey) ||
                (x.KmTo - x.KmFrom).ToString().Contains(dailyReportViewModel.SearchKey)) && x.UserId == currentUserId).ToList();
            }
            else
                dailyReportResult = _uow.DailyReportsRepo.GetAllPagination(x => x.UserId == currentUserId, dailyReportViewModel.PageNumber - 1, dailyReportViewModel.PageSize,
                    dailyReportViewModel.DefaultOrderBy, "Desc");


            var result = new StaticPagedList<DailyReport>(dailyReportResult,
                dailyReportViewModel.PageNumber,
                dailyReportViewModel.PageSize,
                     dailyReportResult.Count);

            dailyReportViewModel.DailyReportList = result;
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_DailyReportList", dailyReportViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_DailyReportPagination", dailyReportViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = result.PageNumber,
                RowsFrom = result.FirstItemOnPage,
                RowsTo = result.LastItemOnPage,
                totalCount = result.Count
            }, JsonRequestBehavior.AllowGet);
        }
    }
}