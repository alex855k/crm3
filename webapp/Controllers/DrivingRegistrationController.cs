using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Data.Entity;
using CRM.DAL;
using Hangfire;
using CRM.Web.Extensions;
using CRM.Application.Core.ViewModels;
using User = CRM.Models.User;
using NLog;

namespace CRM.Web.Controllers
{

    public class DrivingRegistrationController : Controller
    {
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private UnitofWork _uow = new UnitofWork();

      
        public ActionResult DrivingRegistrationModal()
        {
            return View("_DrivingRegistrationModal");
        }

        public ActionResult DrivingRegistrationList()
        {
            return View("_DrivingRegistrationList");
        }

        public ActionResult DrivingRegistrationForm()
        {
            return View("_DrivingRegistrationForm");
        }
       
        [HttpPost]
        public ActionResult CreateDrivingRegistration(string caseId, string description, string dateOfDrive, string addressFrom, string addressTo, string startMileage, string endMileage)
        {
            string errMsg = "Failed to save driving registration";
            DrivingRegistration drivingReg = null;
            try {
                drivingReg = new DrivingRegistration()
                {
                    CaseId = caseId,
                    Description = description,
                    DateOfDrive = DateTime.Parse(dateOfDrive), 
                    AddressFrom = addressFrom,
                    AddressTo = addressTo,
                    StartMileage = decimal.Parse(startMileage),
                    EndMileage = decimal.Parse(endMileage)
                };
                drivingReg.DateOfDrive = DateTime.Parse(dateOfDrive);
            }catch(Exception e)
            {
                logger.Debug(errMsg + ":\n" + e.Message + "\nstacktrace:\n" + e.StackTrace);
            }
            if (drivingReg == null)
            {
                return Json(new
                {
                    status = "failure",
                    success = false,
                    responseText = errMsg + ":\n- Failed to parse parameter"
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                _uow.DrivingRegistrationRepo.Add(drivingReg);
                _uow.SaveChanges();
                return Json(new { success = true, responseText = "success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e, errMsg);
                return Json(new
                {
                    status = "error",
                    success = false,
                    responseText = "Failed to save driving registration"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateDrivingRegistration(DrivingRegistration drivingReg)
        {
            string errMsg = "Failed to save driving registration";
            TryValidateModel(drivingReg);
            try
            {
                if (ModelState.IsValid) { 
                    _uow.DrivingRegistrationRepo.Add(drivingReg);
                    _uow.SaveChanges();
                    return Json(new { success = true, responseText = "success." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        status = "failure",
                        success = false,
                        responseText = "Model failed to validate."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, errMsg);
                return Json(new
                {
                    status = "error",
                    success = false,
                    responseText = errMsg
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpDelete]
        public ActionResult DeleteDrivingReg(int DrivingRegId)
        {
            try
            {
                var DrivingReg = _uow.DrivingRegistrationRepo.Find(DrivingRegId);

                _uow.DrivingRegistrationRepo.Remove(DrivingRegId);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Deleting Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DrivingRegEdit(DrivingRegistration drivingReg)
        {
            try
            {
                var oldDrivingReg = _uow.DrivingRegistrationRepo.Find(drivingReg.Id);
                oldDrivingReg.StartMileage = drivingReg.StartMileage;
                oldDrivingReg.EndMileage = drivingReg.EndMileage;
                oldDrivingReg.AddressTo = drivingReg.AddressTo;
                oldDrivingReg.AddressFrom = drivingReg.AddressFrom;
                oldDrivingReg.Description = drivingReg.Description;
                oldDrivingReg.DateOfDrive = drivingReg.DateOfDrive;

                _uow.DrivingRegistrationRepo.Update(oldDrivingReg);
                _uow.SaveChanges();

                return Json(new { success = true, responseText = "Success." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "failed." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDrivingReg(int DrivingRegId)
        {
            try
            {
                var DrivingReg = _uow.DrivingRegistrationRepo.Find(DrivingRegId);

                return Json(new { success = true, DrivingReg = DrivingReg, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = "Getting Driving Registration Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDrivingRegs(string caseId)
        {
            try
            {
                List<DrivingRegistration> DrivingRegs = null;
                DrivingRegs = _uow.DrivingRegistrationRepo.SearchInclude(x => caseId == x.CaseId).OrderByDescending(x => x.DateOfDrive).ToList();
                return Json(new { success = true, DrivingRegsList = DrivingRegs, responseText = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Debug(e, "Getting drivingRegs failed: \n" + e.StackTrace);
                return Json(new { success = false, responseText = "Getting Driving Registrations Failed" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}