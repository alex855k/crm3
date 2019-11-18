using CRM.Application.Core.Enums;
using CRM.Application.Core.Services;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using CRM.Web.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using X.PagedList;

namespace CRM.Web.Controllers
{
    public class OrdersController : Controller
    {
        private OrderViewModel _orderViewModel = new OrderViewModel();
        private OrderViewModel _customerOrdersViewModel;
        private UnitofWork _uow = new UnitofWork();
        // GET: Orders
        public ViewResult Index()
        {
            var ordersCookie = Request.Cookies["OrdersTable"];
            int pageNumber = 0;
            if (ordersCookie != null && ordersCookie.Value != null && !string.IsNullOrEmpty(ordersCookie.Values["pageNumber"].ToString()))
            {
                pageNumber = int.Parse(ordersCookie.Values["pageNumber"].ToString());
            }
            else
            {
                pageNumber = _orderViewModel.PageNumber;
            }
            _orderViewModel.DefaultOrderBy = "CreationDate";
            _orderViewModel.DefaultDirection = "DESC";
            _orderViewModel.Direction = "DESC";
            var ordersResult = _uow.OrdersRepo.DynamicTable(
                _orderViewModel.PageSize,
                 pageNumber - 1,
                string.Empty,
                _orderViewModel.DefaultOrderBy,
                _orderViewModel.DefaultDirection,
                null);

            var ordersIPagedList = new StaticPagedList<Order>(
                ordersResult.QueryResultList,
                pageNumber,
                _orderViewModel.PageSize,
                ordersResult.TableCount);


            _orderViewModel.OrdersPL = ordersIPagedList;
            _orderViewModel.TableCount = ordersResult.TableCount;
            _orderViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();
            SetOrdersTableCurrentPageCurrentOrder(null, ordersIPagedList.PageNumber);
            return View(_orderViewModel);
        }
        public ActionResult Create()
        {
            _orderViewModel.StatusOptions = _uow.OrderStatusRepo.GetAll();
            _orderViewModel.CustomerOptions =
                _uow.CustomersRepo.Get(x => new Select2 { Id = x.Id, Text = x.CompanyName });
            return View(_orderViewModel);
        }
        public ActionResult CreateViaWebShop(int customerId)
        {
            Customer c = _uow.CustomersRepo.Find(customerId);
            if (c == null)
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);

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
            string query = string.Join("&", encodedProps);
            _orderViewModel = new OrderViewModel
            {
                IsWebshopOrder = true,
                WebShopUrl = base_url + query
            };
            return View("Create", _orderViewModel);
        }
        [System.Web.Mvc.HttpPost]
        public ActionResult Create(OrderViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                if (viewModel.Id == 0)
                {
                    createOrder(viewModel);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Orders.Order.OrderSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                }
                else
                {
                    updateOrder(viewModel);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Orders.Order.OrderUpdated;
                    ReponseViewModel.TransactionType = TransactionType.Update.ToString();
                }
                try
                {
                    _uow.SaveChanges();
                    return Json(new { success = true, response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    return Json(new { success = false, response = JsonConvert.SerializeObject(e) }, JsonRequestBehavior.AllowGet);
                }
            }
            return View();

        }
        public ActionResult CustomerOrderHistory(int customerId)
        {
            _customerOrdersViewModel = new OrderViewModel { CustomerId = customerId };
            _customerOrdersViewModel.DefaultOrderBy = "CreationDate";
            _customerOrdersViewModel.DefaultDirection = "DESC";
            _customerOrdersViewModel.Direction = "DESC";

            var historyResult = _uow.OrdersRepo.DynamicTable(
               _customerOrdersViewModel.PageSize,
               _customerOrdersViewModel.PageNumber - 1,
               string.Empty,
               _customerOrdersViewModel.DefaultOrderBy,
               _customerOrdersViewModel.DefaultDirection,
               order => order.CustomerId == customerId);

            var orderHistoryPagedList = new StaticPagedList<Order>(
                historyResult.QueryResultList,
                _customerOrdersViewModel.PageNumber,
                _customerOrdersViewModel.PageSize,
                historyResult.TableCount);

            _customerOrdersViewModel.OrdersPL = orderHistoryPagedList;
            _customerOrdersViewModel.QueryCount = historyResult.QueryCount;
            _customerOrdersViewModel.TableCount = historyResult.TableCount;
            _customerOrdersViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();

            return View("_CustomerOrderHistory", _customerOrdersViewModel);
        }
        private void SetOrdersTableCurrentPageCurrentOrder(int? orderId, int? currentPageNumber)
        {
            HttpCookie ordersTableCookie = Request.Cookies["OrdersTable"];
            if (ordersTableCookie == null)
            {
                HttpCookie cookie = new HttpCookie("OrdersTable");
                DateTime now = DateTime.Now;
                cookie.Expires = now.AddYears(50);
                if (orderId != null)
                    cookie.Values.Add("orderId", orderId.Value.ToString());
                if (currentPageNumber != null)
                    cookie.Values.Add("pageNumber", currentPageNumber.ToString());
                Response.Cookies.Add(cookie);
            }
            else
            {
                if (orderId != null)
                    ordersTableCookie.Values["orderId"] = orderId.Value.ToString();
                if (currentPageNumber != null)
                    ordersTableCookie.Values["pageNumber"] = currentPageNumber.ToString();
                Response.Cookies.Add(ordersTableCookie);
            }
        }
        private void createOrder(OrderViewModel viewModel)
        {

            Order order = AutoMapper.Mapper.Map<Order>(viewModel);
            order.Status = _uow.OrderStatusRepo.Find(viewModel.StatusId);
            order.CreationDate = DateTime.Now.Date;
            //order.OrderItems = viewModel.OrderItems.ToList();

            order.OrderItems = viewModel.OrderItems.Select(x =>
            new OrderItem
            {
                Id = x.ProductViewModel.Id,
                OrderId = order.Id,
                ProductId = x.ProductViewModel.Id,
                Quantity = x.Quantity
            }).ToList();



            _uow.OrderItemRepo.AddRange(order.OrderItems);
            _uow.OrdersRepo.Add(order);
        }
        [System.Web.Http.HttpPost]
        public ActionResult DynamicCustomerOrderHistoryTable(OrderViewModel orderViewModel)
        {
            TableFilterSortPagingService<Order, OrderViewModel> tableFilter = new TableFilterSortPagingService<Order, OrderViewModel>();
            GenericViewModel<OrderViewModel, Order> genericViewModel = new GenericViewModel<OrderViewModel, Order>()
            {
                QueryParameters = orderViewModel.QueryParameters,
                PageNumber = orderViewModel.PageNumber,
                PageSize = orderViewModel.PageSize,
                OrderBy = orderViewModel.OrderBy,
                DefaultOrderBy = "CreationDate",
                DefaultDirection = "DESC",
                Direction = orderViewModel.Direction,
                QueryOperatorComparer = orderViewModel.QueryOperatorComparer
            };
            genericViewModel.QueryParameters.Add(
                new ExpressionBuilderParameters
                {
                    SearchKey = "CustomerId",
                    Operator = OperatorComparer.Equals.ToString(),
                    Value = orderViewModel.CustomerId.ToString()
                });
            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _customerOrdersViewModel);

            _customerOrdersViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _customerOrdersViewModel.Direction = tableFilterResult.Direction;
            _customerOrdersViewModel.OrderBy = tableFilterResult.OrderBy;
            _customerOrdersViewModel.PageNumber = tableFilterResult.PageNumber;
            _customerOrdersViewModel.QueryCount = tableFilterResult.QueryCount;
            _customerOrdersViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _customerOrdersViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _customerOrdersViewModel.OrdersPL = tableFilterResult.ResultList;
            _customerOrdersViewModel.TableCount = tableFilterResult.TableCount;
            SetOrdersTableCurrentPageCurrentOrder(null, tableFilterResult.PageNumber);
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_OrdersList", _customerOrdersViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_OrderPagination", _customerOrdersViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = tableFilterResult.PageNumber,
                RowsFrom = tableFilterResult.ResultList.FirstItemOnPage,
                RowsTo = tableFilterResult.ResultList.LastItemOnPage,
                totalCount = orderViewModel.QueryParameters != null ? tableFilterResult.QueryCount : tableFilterResult.TableCount
            }, JsonRequestBehavior.AllowGet);
        }
        [System.Web.Http.HttpPost]
        public ActionResult DynamicTable(OrderViewModel orderViewModel)
        {
            TableFilterSortPagingService<Order, OrderViewModel> tableFilter = new TableFilterSortPagingService<Order, OrderViewModel>();
            GenericViewModel<OrderViewModel, Order> genericViewModel = new GenericViewModel<OrderViewModel, Order>()
            {
                QueryParameters = orderViewModel.QueryParameters,
                PageNumber = orderViewModel.PageNumber,
                PageSize = orderViewModel.PageSize,
                OrderBy = orderViewModel.OrderBy,
                DefaultOrderBy = orderViewModel.DefaultOrderBy,
                DefaultDirection = orderViewModel.DefaultDirection,
                Direction = orderViewModel.Direction,
                QueryOperatorComparer = orderViewModel.QueryOperatorComparer,
            };
            var tableFilterResult = tableFilter.FilterTable(genericViewModel, _orderViewModel);

            _orderViewModel.DefaultOrderBy = tableFilterResult.DefaultOrderBy;
            _orderViewModel.Direction = tableFilterResult.Direction;
            _orderViewModel.OrderBy = tableFilterResult.OrderBy;
            _orderViewModel.PageNumber = tableFilterResult.PageNumber;
            _orderViewModel.QueryCount = tableFilterResult.QueryCount;
            _orderViewModel.QueryOperatorComparer = tableFilterResult.QueryOperatorComparer;
            _orderViewModel.QueryParameters = tableFilterResult.QueryParameters;
            _orderViewModel.OrdersPL = tableFilterResult.ResultList;
            _orderViewModel.TableCount = tableFilterResult.TableCount;
            SetOrdersTableCurrentPageCurrentOrder(null, tableFilterResult.PageNumber);
            var tablePartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_OrdersList", _orderViewModel);
            var pagingPartial = Helpers.RazorEngineRender.RazorViewToString(ControllerContext, "_OrderPagination", _orderViewModel);
            return Json(new
            {
                tablePartial = tablePartial,
                pagingPartial = pagingPartial,
                pageNumber = tableFilterResult.PageNumber,
                RowsFrom = tableFilterResult.ResultList.FirstItemOnPage,
                RowsTo = tableFilterResult.ResultList.LastItemOnPage,
                totalCount = orderViewModel.QueryParameters != null ? tableFilterResult.QueryCount : tableFilterResult.TableCount
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Edit(int id, int latestOrdersPageNumber)
        {
            var order = _uow.OrdersRepo.Find(id);
            _orderViewModel = AutoMapper.Mapper.Map<OrderViewModel>(order);
            _orderViewModel.StatusOptions = _uow.OrderStatusRepo.GetAll();
            //latestCustomersPageNumber is not used. Should it be passed instead of null?
            SetOrdersTableCurrentPageCurrentOrder(id, null);
            return View("Create", _orderViewModel);
        }
        private void updateOrder(OrderViewModel orderViewModel)
        {
            var order = _uow.OrdersRepo.Search(x => x.Id == orderViewModel.Id).SingleOrDefault();
            order.DeliveryAddressStreetAndHouseNr = string.Format("{0} {1}",orderViewModel.DeliveryStreet, orderViewModel.DeliveryHouseNr);
            order.DispatchDate = orderViewModel.DispatchDate;
            order.Status = orderViewModel.Status;
            order.DeliveryAddressPostalCode = orderViewModel.DeliveryPostalCode;
            order.DeliveryAddressTown = orderViewModel.DeliveryTown;

            _uow.OrdersRepo.Update(order);
            _uow.OrderItemRepo.RemoveRange(order.OrderItems);
            _uow.OrderItemRepo.AddRange(orderViewModel.OrderItems.Select(x =>
                new OrderItem
                {
                    Id = x.ProductViewModel.Id,
                    OrderId = order.Id,
                    ProductId = x.ProductViewModel.Id,
                    Quantity = x.Quantity
                }).ToList());
        }
    }
}