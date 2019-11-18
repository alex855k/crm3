using CRM.Models;
using System;
using System.Collections.Generic;

namespace CRM.Application.Core.ViewModels
{
    [Serializable]
    public class APIPlaceOrderViewModel
    {
        public int? OrderId { get; set; }

        public string BillingAddressStreet { get; set; }
        public string BillingAddressHouseNr { get; set; }
        public int BillingAddressPostalCode { get; set; }
        public string BillingAddressTown { get; set; }

        public string DeliveryAddressStreet { get; set; }
        public string DeliveryAddressHouseNr { get; set; }
        public int DeliveryAddressPostalCode { get; set; }
        public string DeliveryAddressTown { get; set; }

        public int CustomerId { get; set; }
        public OrderItemViewModel[] OrderItems { get; set; }
    }
}
