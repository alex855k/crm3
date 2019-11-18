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
    public class CustomersAPIv1Controller : ApiController
    {
        private UnitofWork _uow = new UnitofWork();

        [HttpPost, ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> Create(CustomerViewModel custVm)
        {
            //both (CompanyName AND Address) are required
            //Either CustomerTypeId(int) or CustomerType(string) is required

            CustomerType customerType = null;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (custVm.CustomerTypeId > 0)
                customerType = _uow.CustomerTypesRepo.Find(custVm.CustomerTypeId);
            if (customerType == null)
            {
                customerType = _uow.CustomerTypesRepo.Search(ct => ct.Name == custVm.CustomerType
                ).SingleOrDefault();
                if (customerType == null)
                    return BadRequest("CustomerTypeId(int) or CustomerType(string) is required");
            }

            var customer = _uow.CustomersRepo.Search(
                c => c.Address == custVm.Address && c.CompanyName == custVm.CompanyName
                ).SingleOrDefault();

            if (customer == null)
            {
                customer = new Customer
                {
                    CompanyName = custVm.CompanyName,
                    Address = custVm.Address,
                    PostalCode = custVm.PostalCode,
                    Country = custVm.Country,
                    CustomerTypeId = customerType.Id,
                    CustomerStatusId = 1,

                    //optional
                    CVR = custVm.CVR,
                    EAN = custVm.EAN,
                    DELEAN = custVm.DELEAN,
                    Email = custVm.Email,
                    Phone = custVm.Phone,
                    Town = custVm.Town,
                    CompanyURL = custVm.CompanyURL,
                    AdditionalInfo = custVm.AdditionalInfo
                };
                _uow.CustomersRepo.Add(customer);
            }
            await _uow.SaveChangesAsync();
            return Ok(customer.Id);
        }
    }
}
