using CRM.DAL;
using CRM.Identity;
using CRM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM.Web.Controllers
{
    public class CompanyProfileController : Controller
    {
        // GET: CompanyProfile
        UnitofWork _uow = new UnitofWork();
        [CRMAuthorize]
        public ActionResult Index()
        {
            var companyProfile = _uow.CompanyProfileRepo.GetAll().SingleOrDefault();
            return View(companyProfile);
        }

        [HttpPost]
        public ActionResult UploadCompanyImage()
        {
            var file = Request.Files[0];
            MemoryStream target = new MemoryStream();
            file.InputStream.CopyTo(target);
            byte[] imgByte = target.ToArray();
            var comnpanyProfile = _uow.CompanyProfileRepo.GetAll().SingleOrDefault();
            comnpanyProfile.Logo = imgByte;
            try
            {
                _uow.SaveChanges();
                UploadCompanyLogo(file);
                return Json(JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonRequestBehavior.DenyGet);
            }
        }

        private void UploadCompanyLogo(HttpPostedFileBase file)
        {
            string logoFilePath = Server.MapPath("~/Content/Uploads/Img/companyLogo"); 
            bool exists = Directory.Exists(logoFilePath);
            if (!exists)
                Directory.CreateDirectory(logoFilePath);
            string filePath = (Path.Combine(logoFilePath, "companyLogo.png  "));
            file.SaveAs(filePath);
        }

        [HttpPost]
        public ActionResult UpdateCompanyProfile (CompanyProfile companyProfile)
        {
            var companyProfileObj = _uow.CompanyProfileRepo.GetAll().SingleOrDefault();
            companyProfileObj.Address = companyProfile.Address;
            companyProfileObj.Email = companyProfile.Email;
            companyProfileObj.Name = companyProfile.Name;
            companyProfileObj.Phone = companyProfile.Phone;
            companyProfileObj.URL = companyProfile.URL;
            try
            {
                _uow.SaveChanges();
                return Json(JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonRequestBehavior.DenyGet);
            }
        }

        
    }
}