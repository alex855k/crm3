using AutoMapper;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using LinqToExcel;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;



namespace CRM.Web.Controllers
{
    public class UploadCustomersController : Controller
    {
        // GET: UploadCustomers
        [CRMAuthorize]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UploadCustomers()
        {
            var file = Request.Files["customersUploadFile"];
            if (file != null && Path.GetExtension(file.FileName).Substring(1) == "xlsx")
            {
                string currentUserId = User.Identity.GetUserId();
                string uploadedFilesPath = Server.MapPath("~/Content/tempFiles/");
                bool exists = Directory.Exists(uploadedFilesPath);
                if (!exists)
                    Directory.CreateDirectory(uploadedFilesPath);
                string fileName = Path.GetFileName(file.FileName);
                string filePath = (Path.Combine(uploadedFilesPath, fileName));
                file.SaveAs(filePath);
                int? addCustomersCount = ReadExcel(filePath);
                return addCustomersCount != null ?
                    Json(string.Format("{0} {1}", addCustomersCount.Value.ToString(), CRM.Application.Core.Resources.Customers.Customer.CustomersUploadedCount),
                    JsonRequestBehavior.AllowGet) :
                    Json("Error Upload Customer File", JsonRequestBehavior.DenyGet);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "File Not Supported");
        }

        private int? ReadExcel(string filePath)
        {
            var excel = new ExcelQueryFactory(filePath);
            var language = (from c in excel.WorksheetNoHeader("Language") select c).ToList().First().SingleOrDefault();
            if (language == "dk")
            {
                excel.AddMapping<CustomerViewModel>(x => x.CompanyName, "FirmaNavn");
                excel.AddMapping<CustomerViewModel>(x => x.CustomerTypeId, "KundeTypeId");
                excel.AddMapping<CustomerViewModel>(x => x.CustomerStatusId, "KundeStatusId");
                excel.AddMapping<CustomerViewModel>(x => x.CVR, "VATNumber");
                excel.AddMapping<CustomerViewModel>(x => x.EAN, "EAN");
                excel.AddMapping<CustomerViewModel>(x => x.DELEAN, "PartEAN");
                excel.AddMapping<CustomerViewModel>(x => x.Email, "Email");
                excel.AddMapping<CustomerViewModel>(x => x.Phone, "Telefon");
                excel.AddMapping<CustomerViewModel>(x => x.Country, "Land");
                excel.AddMapping<CustomerViewModel>(x => x.Town, "By");
                excel.AddMapping<CustomerViewModel>(x => x.PostalCode, "Postnummer");
                excel.AddMapping<CustomerViewModel>(x => x.Address, "Adresse");
                excel.AddMapping<CustomerViewModel>(x => x.CompanyURL, "Adresse");
                excel.AddMapping<CustomerViewModel>(x => x.AdditionalInfo, "YderligereInformation");
            }
            var customersListFromExcel = (from c in excel.Worksheet<CustomerViewModel>("Customers")
                                          select c).ToList();

            customersListFromExcel = customersListFromExcel.Where(s => !string.IsNullOrWhiteSpace(s.CompanyName) ||
                                      s.CustomerTypeId != 0 ||
                                      s.CustomerStatusId != 0).ToList();
            UnitofWork uow = new UnitofWork();
            uow.CustomersRepo.AddRange(Mapper.Map<List<Customer>>(customersListFromExcel));
            try
            {
                uow.SaveChanges();
                return customersListFromExcel.Count();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}