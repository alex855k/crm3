using System;
using System.Collections.Generic;


namespace CRM.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set;}
        public DateTime? DispatchDate { get; set; }
        public DateTime? ETA { get; set; } //Estimated time of arrival

        public string BillingAddressStreet { get; set; }
        public string BillingAddressHouseNr { get; set; }
        public int BillingAddressPostalCode { get; set; }
        public string BillingAddressTown { get; set; }
        public string BillingAddressStreetAndHouseNr { get; set; }

        public string DeliveryAddressStreet { get; set; }
        public string DeliveryAddressHouseNr { get; set; }
        public int DeliveryAddressPostalCode { get; set; }
        public string DeliveryAddressTown { get; set; }

        public string DeliveryAddressStreetAndHouseNr { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual OrderStatus Status { get; set; }

        public virtual List<OrderItem> OrderItems { get; set; }

        //get { return OrderedProducts; }
        //set
        //{
        //    value.ForEach(x => x.OrderId = Id);
        //    OrderedProducts = value;
        //}
    }
}
