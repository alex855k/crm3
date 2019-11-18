using AutoMapper;
using CRM.Application.Core.Enums;
using CRM.Application.Core.Services;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.DAL.Repositories;
using CRM.Identity;
using CRM.Models;
using CRM.Web.Helpers;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

namespace CRM.Web.Controllers
{
    [CRMAuthorize]
    public class BudgetController: Controller
    {
        // GET: Budget
        UnitofWork _uow = new UnitofWork();
        BudgetViewModel _budgetViewModel = new BudgetViewModel();
        private UserManager _userManager;
        public BudgetController()
        {
        }
        public BudgetController(UserManager userManager)
        {
            UserManager = userManager;
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
        public ActionResult Index()
        {
            var budgetList = _uow.BudgetRepo.GetAll();
            _budgetViewModel.SalesPersonList = GetSalesUsers();

            _budgetViewModel.DefaultOrderBy = "BudgetDate";
            var budgetResult = _uow.BudgetRepo.DynamicTable(
                _budgetViewModel.PageSize,
                _budgetViewModel.PageNumber - 1,
                string.Empty,
                _budgetViewModel.DefaultOrderBy,
                _budgetViewModel.DefaultDirection,
                null);

            var budgetIPagedList = new StaticPagedList<Budget>(
                budgetResult.QueryResultList,
                _budgetViewModel.PageNumber,
                _budgetViewModel.PageSize, _budgetViewModel.TableCount);
            _budgetViewModel.BudgetList = budgetIPagedList;
            _budgetViewModel.TableCount = budgetResult.TableCount;
            _budgetViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();

            return View(_budgetViewModel);
        }

        private List<UserViewModel> GetSalesUsers()
        {
            var users = UserManager.Users.Where(x => x.IsSuperAdmin == false);
            List<UserViewModel> userViewModel = new List<UserViewModel>();
            List<UserViewModel> userViewModelList = Mapper.Map<List<UserViewModel>>(users);
            userViewModel.AddRange(userViewModelList);
            return userViewModel;
        }

        [HttpPost]
        public ActionResult CreateBudget(BudgetViewModel budgetViewModel)
        {
            _uow.BudgetRepo.Add(new Budget {
                BudgetAmount = budgetViewModel.BudgetAmount,
                BudgetDate = budgetViewModel.BudgetDate,
                SalesPersonId = budgetViewModel.SalesPersonId
            });
            try
            {
                _uow.SaveChanges();
                return View();
            }
            catch (Exception)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public ActionResult BudgetBarChart()
        {
            BudgetBarChartViewModel budgetBarChartViewModel = new BudgetBarChartViewModel();
            var budget = _uow.BudgetRepo.Search(x => DbFunctions.TruncateTime(x.BudgetDate).Value.Year == DateTime.Now.Year).OrderBy(x => x.BudgetDate).ToList();
            budgetBarChartViewModel.Labels = budget.Select(x => x.BudgetDate.ToString("MMMM")).ToArray();
            budgetBarChartViewModel.BudgetBar = budget.Select(x => x.BudgetAmount).ToArray();
            budgetBarChartViewModel.SalesBar = new decimal[] { 4000, 20, 30, 50, 40 };
            return Json(budgetBarChartViewModel,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DynamicTable(BudgetViewModel budgetViewModel)
        {
            
            Expression<Func<Budget, bool>> query = null;
            Expression<Func<Budget, bool>> temp = null;
            QueryOperatorComparer queryOperator = (QueryOperatorComparer)Enum.Parse(typeof(QueryOperatorComparer), budgetViewModel.QueryOperatorComparer);
            DynamicTableQueryResult<Budget> dynamicTableQueryResult = new DynamicTableQueryResult<Budget>();
            if (budgetViewModel.QueryParameters != null)
            {

                for (int i = 0; i < budgetViewModel.QueryParameters.Count(); i++)
                {
                    var queryItem = budgetViewModel.QueryParameters[i];
                    if (!string.IsNullOrEmpty(queryItem.SearchKey) && !string.IsNullOrEmpty(queryItem.Value))
                    {
                        query = LinqExpressionBuilder.BuildPredicate<Budget>(queryItem.Value, (OperatorComparer)Enum.Parse(typeof(OperatorComparer), queryItem.Operator), queryItem.SearchKey);
                        if (temp != null)
                        {
                            query = LinqExpressionBuilder.AddOperatorBetweenTwoExpression<Budget>(query, temp, queryOperator);

                        }
                        temp = query;
                    }
                    dynamicTableQueryResult = _uow.BudgetRepo.DynamicTable(
                        budgetViewModel.PageSize,
                        budgetViewModel.PageNumber - 1,
                        budgetViewModel.OrderBy,
                        budgetViewModel.DefaultOrderBy,
                        budgetViewModel.Direction ?? budgetViewModel.DefaultDirection,
                        query);
                }
            }
            else
                dynamicTableQueryResult = _uow.BudgetRepo.DynamicTable(
                         budgetViewModel.PageSize,
                         budgetViewModel.PageNumber - 1,
                         budgetViewModel.OrderBy,
                         budgetViewModel.DefaultOrderBy,
                         budgetViewModel.Direction ?? budgetViewModel.DefaultDirection,
                         null);
            budgetViewModel.BudgetList = new StaticPagedList<Budget>(dynamicTableQueryResult.QueryResultList,
                budgetViewModel.PageNumber,
                budgetViewModel.PageSize,
               budgetViewModel.QueryParameters != null ? dynamicTableQueryResult.QueryCount : dynamicTableQueryResult.TableCount);
            budgetViewModel.TableCount = dynamicTableQueryResult.TableCount;
            budgetViewModel.QueryCount = dynamicTableQueryResult.QueryCount;
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_BudgetList", _budgetViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(this.ControllerContext, "_BudgetPagination", _budgetViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = _budgetViewModel.PageNumber,
                RowsFrom = _budgetViewModel.BudgetList.FirstItemOnPage,
                RowsTo = _budgetViewModel.BudgetList.LastItemOnPage,
                totalCount = budgetViewModel.QueryParameters != null ? _budgetViewModel.QueryCount : _budgetViewModel.TableCount
            }, JsonRequestBehavior.AllowGet);
        }

   
    }
}