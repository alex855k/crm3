using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class OrderViewModel : DynamicTableViewModel<Order>
    {
        public int Id { get; set; }

        public DateTime CreationDate { get; set; }

        [DataType(DataType.Date),
        DisplayFormat(ConvertEmptyStringToNull = true)]
        public DateTime? DispatchDate { get; set; }
        public DateTime? ETA { get; set; } //Estimated time of arrival
        public TimeSpan? DeliveryTime { get; set; } //Estimated time to process and deliver

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.General.General))]
        public string BillingStreet { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.General.General))]
        public string BillingHouseNr { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.General.General))]
        [Range(1, 999999999, ErrorMessageResourceName = "PostalCodeOutOfRange", ErrorMessageResourceType = typeof(Resources.Orders.Order))]
        public int BillingPostalCode { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.General.General))]
        public string BillingTown { get; set; }

        public bool DeliverySameAsBilling { get; set; }
        public string DeliveryStreet { get; set; }
        public string DeliveryHouseNr { get; set; }
        public int DeliveryPostalCode { get; set; }
        public string DeliveryTown { get; set; }

        [RegularExpression("([0-9]+)"),
        Remote("CustomerExists", "Customers", ErrorMessageResourceName = "CustomerNotFound", ErrorMessageResourceType = typeof(Resources.Customers.Customer))]
        public int CustomerId { get; set; }

        public string CustomerCompany { get; set; }
        public List<Select2> CustomerOptions { get; set; }
        public StaticPagedList<Order> OrdersPL { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.General.General))]
        public int StatusId { get; set; }

        public OrderStatus Status { get; set; }
        public IEnumerable<OrderStatus> StatusOptions { get; set; }
        public OrderItemViewModel[] OrderItems { get; set; }
        public bool IsWebshopOrder { get; set; }
        public string WebShopUrl { get; set; }
    }
}