using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using CRM.API;
using CRM.Application.Core.ViewModels;
using CRM.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace API_Testing
{
    [TestClass]
    public class OrderApi
    {
        private OrderAPIv1Controller orderAPI = new OrderAPIv1Controller();
        [TestMethod]
        public void PlaceOrderAsyncTest()
        {
            var placeOrderViewModel = new APIPlaceOrderViewModel
            {
                BillingAddressStreet = "BillingAddressStreet",
                BillingAddressHouseNr = "BillingAddressHouseNr",
                BillingAddressPostalCode = 4444,
                BillingAddressTown = "BillingAddressTown",
                DeliveryAddressStreet = "DeliveryAddressStreet",
                DeliveryAddressHouseNr = "DeliveryAddressHouseNr",
                DeliveryAddressPostalCode = 4445,
                DeliveryAddressTown = "DeliveryAddressTown",
                OrderedProducts = new List<OrderedProduct> { new OrderedProduct { Id = 1 } },
                CustomerId = 3200
            };



            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:58395/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Create the JSON formatter.
            MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();

            // Use the JSON formatter to create the content of the request body.
            HttpContent content = new ObjectContent<APIPlaceOrderViewModel>(placeOrderViewModel, jsonFormatter);

            // Send the request.
            var resp = client.PostAsync("api/orders/PlaceOrderAsync", content).Result;
            Assert.IsNotNull(resp);
        }
    }
}
