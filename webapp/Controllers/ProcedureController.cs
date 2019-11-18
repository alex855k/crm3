using AutoMapper;
using CRM.Application.Core.Enums;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using X.PagedList;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharp.Pdf;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.Core;
using System.Configuration;
using CRM.Web.Helpers;

namespace CRM.Web.Controllers
{
    public class ProcedureController : Controller
    {
        private ProcedureViewModel _procedureViewModel = new ProcedureViewModel();
        private UnitofWork _uow = new UnitofWork();
        const int maxFileSizeInBytes = 104857600;
        private string pdfExtension = ".pdf";

        // GET: Procedure
        public ActionResult Index()
        {
            _procedureViewModel.DefaultOrderBy = "Created";

            var customersResult = _uow.ProcedureRepo.DynamicTable(
                _procedureViewModel.PageSize,
                _procedureViewModel.PageNumber - 1,
                string.Empty,
                _procedureViewModel.DefaultOrderBy,
                _procedureViewModel.DefaultDirection,
                null);
            var customersIPagedList = new StaticPagedList<Procedure>(
                customersResult.QueryResultList,
                _procedureViewModel.PageNumber,
                _procedureViewModel.PageSize, customersResult.TableCount);
            _procedureViewModel.ProcedureList = customersIPagedList;
            _procedureViewModel.TableCount = customersResult.TableCount;
            _procedureViewModel.QueryOperatorComparer = QueryOperatorComparer.And.ToString();

            return View(_procedureViewModel);
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction("Index");

            var procedures = _uow.ProcedureRepo.SearchInclude(x => x.Id == id, "ProcedureItems");
            foreach(Procedure p in procedures)
            {
                _procedureViewModel.Id = p.Id;
                _procedureViewModel.Title = p.Title;
                _procedureViewModel.Created = p.Created;
                _procedureViewModel.Edited = p.Edited;
                _procedureViewModel.ImagePath = p.ImagePath;
                _procedureViewModel.ProcedureItems = p.ProcedureItems;
            }
            
            return View(_procedureViewModel);
        }

        public ActionResult Create()
        {
            return View(_procedureViewModel);
        }

        [HttpPost]
        public ActionResult Create(ProcedureViewModel procedureViewModel, HttpPostedFileBase procedureImage, HttpPostedFileBase[] procedureItemImages)
        {
            if (ModelState.IsValid)
            {
                if (procedureImage != null)
                {
                    string pic = System.IO.Path.GetFileName(procedureImage.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads/Img/Procedure/"));

                    Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                    path = path + unixTimestamp + pic;
                    procedureImage.SaveAs(path);

                    //Resize image
                    System.Drawing.Image image = System.Drawing.Image.FromFile(path);
                    float aspectRatio = (float)image.Size.Width / (float)image.Size.Height;
                    int newHeight = 200;
                    int newWidth = Convert.ToInt32(aspectRatio * newHeight);

                    System.Drawing.Bitmap thumbBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
                    System.Drawing.Graphics thumbGraph = System.Drawing.Graphics.FromImage(thumbBitmap);
                    thumbGraph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    thumbGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    thumbGraph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    var imageRectangle = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);
                    thumbGraph.DrawImage(image, imageRectangle);

                    thumbBitmap.Save(Server.MapPath("~/Content/Uploads/Img/Procedure/" + "th_" + unixTimestamp + pic));

                    thumbGraph.Dispose();
                    thumbBitmap.Dispose();
                    image.Dispose();

                    procedureViewModel.ImagePath = "th_" + unixTimestamp + pic;
                }
                if (procedureItemImages != null)
                {
                    int index = 0;
                    foreach (HttpPostedFileBase file in procedureItemImages)
                    {
                        if (file != null)
                        {
                            string pic = Path.GetFileName(file.FileName);
                            string path = Path.Combine(Server.MapPath("~/Content/Uploads/Img/Procedure/"));

                            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                            path = path + unixTimestamp + pic;
                            file.SaveAs(path);

                            //Resize image
                            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
                            float aspectRatio = (float)image.Size.Width / (float)image.Size.Height;
                            int newHeight = 200;
                            int newWidth = Convert.ToInt32(aspectRatio * newHeight);

                            System.Drawing.Bitmap thumbBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
                            System.Drawing.Graphics thumbGraph = System.Drawing.Graphics.FromImage(thumbBitmap);
                            thumbGraph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            thumbGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            thumbGraph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            var imageRectangle = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);
                            thumbGraph.DrawImage(image, imageRectangle);

                            thumbBitmap.Save(Server.MapPath("~/Content/Uploads/Img/Procedure/" + "th_" + unixTimestamp + pic));

                            thumbGraph.Dispose();
                            thumbBitmap.Dispose();
                            image.Dispose();

                            procedureViewModel.ProcedureItems[index].ImagePath = "th_" + unixTimestamp + pic;
                        }
                        index++;
                    }
                }

                if (procedureViewModel.Id == 0)
                {
                    CreateProcedure(procedureViewModel);
                    ReponseViewModel.ResponseMessage = CRM.Application.Core.Resources.Procedures.Procedure.ProcedureSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                }
                else
                {
                    UpdateProcedure(procedureViewModel);
                    ReponseViewModel.ResponseMessage = CRM.Application.Core.Resources.Procedures.Procedure.ProcedureUpdated;
                    ReponseViewModel.TransactionType = TransactionType.Update.ToString();
                }
                try
                {
                    _uow.SaveChanges();
                    return Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    throw;
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            return View();
        }

        private void CreateProcedure(ProcedureViewModel procedureViewModel)
        {
            List<ProcedureItem> procedureItem = new List<ProcedureItem>();
            foreach (var item in procedureViewModel.ProcedureItems)
            {
                procedureItem.Add(
                    new ProcedureItem
                    {
                        Title = item.Title,
                        Description = item.Description,
                        ImagePath = item.ImagePath,
                        ProcedureId = procedureViewModel.Id
                    }
                );
            }
            _uow.ProcedureRepo.Add(new Procedure { Title = procedureViewModel.Title, Created = DateTime.Now, ImagePath = procedureViewModel.ImagePath });
            _uow.ProcedureItemsRepo.AddRange(procedureItem);
        }
        
        private void UpdateProcedure(ProcedureViewModel procedureViewModel)
        {
            Procedure procedure = Mapper.Map<Procedure>(procedureViewModel);
            procedure.Edited = DateTime.Now;

            //Get the "old" procedure from the DB
            var oldProcedureItemsResult = _uow.ProcedureItemsRepo.Search(x => x.ProcedureId == procedure.Id);
            List<ProcedureItem> oldProcedureItems = new List<ProcedureItem>();
            foreach (ProcedureItem item in oldProcedureItemsResult)
            {
                oldProcedureItems.Add(item);
            }

            List<ProcedureItem> procedureItems = new List<ProcedureItem>();
            foreach (var item in procedureViewModel.ProcedureItems)
            {
                procedureItems.Add(
                    new ProcedureItem
                    {
                        Title = item.Title,
                        Description = item.Description,
                        ImagePath = item.ImagePath,
                        ProcedureId = procedure.Id                        
                    }
                );
            }

            procedure.ProcedureItems = procedureItems;

            //Remove old procedureItems
            _uow.ProcedureItemsRepo.RemoveRange(oldProcedureItems);
            //Add proc. items again
            _uow.ProcedureItemsRepo.AddRange(procedureItems);
            //Update the the procedure
            _uow.ProcedureRepo.Update(procedure);
        }

        public ActionResult Edit(int id)
        {
            var procedure = _uow.ProcedureRepo.SearchInclude(x => x.Id == id, "ProcedureItems");
            foreach (Procedure p in procedure)
            {
                _procedureViewModel.Id = p.Id;
                _procedureViewModel.Title = p.Title;
                _procedureViewModel.Created = p.Created;
                _procedureViewModel.Edited = p.Edited;
                _procedureViewModel.ImagePath = p.ImagePath;
                _procedureViewModel.ProcedureItems = p.ProcedureItems;
                _procedureViewModel.PDFName = p.PDFName;
            }
            return View("Create", _procedureViewModel);
        }

        public ActionResult Delete(int procedureItemId, int procedureId)
        {
            DeleteProcedureItemImage(procedureItemId);
            _uow.ProcedureItemsRepo.Remove(procedureItemId);
            try
            {
                _uow.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }
            return Redirect("/Procedure/Edit/" + procedureId);
        }

        [HttpPost]
        public ActionResult DeleteProcedureItemImage(int id)
        {
            var procedureItem = _uow.ProcedureItemsRepo.Find(id);

            string path = Path.Combine(Server.MapPath("~/Content/Uploads/Img/Procedure/" + procedureItem.ImagePath));
            string pathOriginal = Path.Combine(Server.MapPath("~/Content/Uploads/Img/Procedure/" + procedureItem.ImagePath.Substring(3)));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            if (System.IO.File.Exists(pathOriginal))
            {
                System.IO.File.Delete(pathOriginal);
            }

            procedureItem.ImagePath = null;

            var procedure = _uow.ProcedureRepo.Find(procedureItem.ProcedureId);
            procedure.Edited = DateTime.Now;

            _uow.ProcedureItemsRepo.Update(procedureItem);
            _uow.ProcedureRepo.Update(procedure);
            try
            {
                _uow.SaveChanges();
                return Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult DeleteProcedureImage(int id)
        {
            var procedure = _uow.ProcedureRepo.Find(id);

            string path = Path.Combine(Server.MapPath("~/Content/Uploads/Img/Procedure/" + procedure.ImagePath));
            string pathOriginal = Path.Combine(Server.MapPath("~/Content/Uploads/Img/Procedure/" + procedure.ImagePath.Substring(3) ));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            if(System.IO.File.Exists(pathOriginal))
            {
                System.IO.File.Delete(pathOriginal);
            }

            procedure.Edited = DateTime.Now;
            procedure.ImagePath = null;

            _uow.ProcedureRepo.Update(procedure);
            try
            {
                _uow.SaveChanges();
                return Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }
        }

        public ActionResult PdfVersion(int id)
        {
            //Generate pdf
            Procedure procedure = _uow.ProcedureRepo.SearchInclude(x => x.Id == id, "ProcedureItems").SingleOrDefault();
            
            if (procedure != null)
            {
                procedure.PDFName = GeneratePdf(procedure);
                _uow.ProcedureRepo.Update(procedure);
            }

            try
            {
                _uow.SaveChanges();

                string pdfPath = Server.MapPath("~/Content/Uploads/PDF/Procedure/");

                string result = pdfPath + procedure.PDFName + pdfExtension;

                //Show pdf
                return File(result, "application/pdf");
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }
        }

        public ActionResult SendToEmail(int id, string email, string subject, string body)
        {
            //Generate pdf
            Procedure procedure = _uow.ProcedureRepo.SearchInclude(x => x.Id == id, "ProcedureItems").SingleOrDefault();

            string result = "";

            if (procedure != null)
            {
                procedure.PDFName = GeneratePdf(procedure);
                _uow.ProcedureRepo.Update(procedure);
            }
            try
            {
                _uow.SaveChanges();
                string pdfPath = Server.MapPath("~/Content/Uploads/PDF/Procedure/");
                result = pdfPath + procedure.PDFName + pdfExtension;
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }

            //Send pdf to email
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["Email:Host"], Convert.ToInt32(ConfigurationManager.AppSettings["Email:Port"].ToString()));
            NetworkCredential basicCredential = new NetworkCredential(ConfigurationManager.AppSettings["Email:Username"], ConfigurationManager.AppSettings["Email:Password"]);
            MailMessage message = new MailMessage();
            MailAddress fromAddress = new MailAddress(ConfigurationManager.AppSettings["Email:Username"]);

            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredential;
            smtpClient.Timeout = (60 * 5 * 1000);

            message.From = fromAddress;
            message.Subject = subject != "" ? subject : "Send from CRM subject"; // TODO: Maybe change this standard message or translate it
            message.IsBodyHtml = false;
            message.Body = body != "" ? body : "Send from CRM body"; // TODO: Maybe change this standard message or translate it
            message.To.Add(email);

            if (result != null)
            {
                Attachment attachment = new Attachment(result, MediaTypeNames.Application.Octet);
                ContentDisposition disposition = attachment.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(result);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(result);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(result);
                disposition.FileName = Path.GetFileName(result);
                disposition.Size = new FileInfo(result).Length;
                disposition.DispositionType = DispositionTypeNames.Attachment;

                message.Attachments.Add(attachment);
            }

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception e)
            {
                throw;
            }

            return Redirect("/Procedure/Details/" + id);
        }

        private string GeneratePdf(Procedure procedure)
        {
            string htmlContent = "";

            string templateFolderPath = Server.MapPath("~/Views/ProcedureTemplates/");
            string imageFolderPath = Server.MapPath("~/Content/Uploads/Img/Procedure/");
            htmlContent = RazorEngineRender.PartialViewToString(templateFolderPath, "ProcedurePDFTemplate.cshtml", new ProcedureViewModel
            {
                Id = procedure.Id,
                Title = procedure.Title,
                Created = procedure.Created,
                Edited = procedure.Edited,
                ImagePath = procedure.ImagePath,
                ProcedureItems = procedure.ProcedureItems,
                ImageFolderPath = imageFolderPath
            });

            PdfGenerateConfig config = new PdfGenerateConfig();
            PdfDocument pdf = PdfGenerator.GeneratePdf(htmlContent, PageSize.A4);
            pdf.Info.Title = procedure.Title + "/" + procedure.Created;
            pdf.Info.CreationDate = DateTime.Now;

            string pdfPath = Server.MapPath("~/Content/Uploads/PDF/Procedure/");

            string fileName = "";

            if (procedure.PDFName != null && procedure.PDFName != "")
            {
                fileName = procedure.PDFName;
            }
            else
            {
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                fileName = (unixTimestamp + "_" + procedure.Title).Replace(" ", "_");
            }

            string filePath = Path.Combine(pdfPath, string.Format("{0}{1}", fileName, pdfExtension));

            try
            {
                pdf.Save(filePath);
            }
            catch(Exception e)
            {
                throw;
            }

            return fileName;
        }
    }
}