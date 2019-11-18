using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CRM.Models;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using System.Collections.Generic;
using System.Linq;

namespace CRM.API
{
    //[RoutePrefix("api/orders")]
    public class OrderAPIv1Controller : ApiController
    {
        private UnitofWork _uow = new UnitofWork();

        [HttpPost, ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> PlaceOrder(OrderViewModel orderVm)
        {
            //for every ProductViewModel in orderVm.OrderItems
                //if a valid productId is provided, include that product in the order.
                //if no id is provided, then Name and Price are required. 
                //If no Product exists with the Name and Price, then a new Product will be created.


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var orderItems = new List<OrderItem>();
            var newProducts = new List<Product>();

            foreach (var oProd in orderVm.OrderItems)
            {
                if (oProd.Quantity < 1)
                    return BadRequest();
                if (oProd.ProductViewModel.Id > 0)
                {
                    orderItems.Add(new OrderItem {
                        ProductId = oProd.ProductViewModel.Id,
                        Quantity = oProd.Quantity
                    });
                }
                else if (oProd.ProductViewModel.Name.Length <= 0 || oProd.ProductViewModel.Price <= 0)
                    return BadRequest("CustomerTypeId(int) or CustomerType(string) is required");
                else
                {
                    var existingProduct = _uow.ProductRepo.Search(
                        p => p.Name  == oProd.ProductViewModel.Name 
                        &&   p.Price == oProd.ProductViewModel.Price
                        ).FirstOrDefault();
                    if (existingProduct != null)
                    {
                        orderItems.Add(new OrderItem
                        {
                            ProductId = existingProduct.Id,
                            Quantity = oProd.Quantity
                        });
                    }
                    else
                    {
                        var prod = new Product
                        {
                            Name = oProd.ProductViewModel.Name,
                            Price = oProd.ProductViewModel.Price
                        };
                        _uow.ProductRepo.Add(prod);
                        _uow.SaveChanges();
                        orderItems.Add(new OrderItem
                        {
                            ProductId = prod.Id,
                            Quantity = oProd.Quantity,
                        });
                    }
                }

            }

            if (orderItems.Count == 0)
                return BadRequest(ModelState);

            Order o = new Order
            {
                CreationDate = DateTime.Now,
                Status = _uow.OrderStatusRepo.Find(1),

                BillingAddressStreet = orderVm.BillingStreet,
                BillingAddressHouseNr = orderVm.BillingHouseNr,
                BillingAddressPostalCode = orderVm.BillingPostalCode,
                BillingAddressTown = orderVm.BillingTown,
                DeliveryAddressStreet = orderVm.DeliveryStreet,
                DeliveryAddressHouseNr = orderVm.DeliveryHouseNr,
                DeliveryAddressPostalCode = orderVm.DeliveryPostalCode,
                DeliveryAddressTown = orderVm.DeliveryTown,
                CustomerId = orderVm.CustomerId,
                OrderItems = orderItems
            };

            _uow.OrderItemRepo.AddRange(o.OrderItems);
            _uow.OrdersRepo.Add(o);

            await _uow.SaveChangesAsync();

            //return CreatedAtRoute("Orders/edit", new { id = orderVm.OrderId, latestOrdersPageNumber = 1 }, orderVm);
            return Ok(o.Id);
        }
    }
}
