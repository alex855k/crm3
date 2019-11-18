using AutoMapper;
using CRM.Application.Core.Enums;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.DAL.Repositories;
using CRM.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CRM.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardListsController : Controller
    {
        UnitofWork _uow = new UnitofWork();
        public ActionResult Index()
        {
            DashboardListsViewModel dashboardListsViewModel = new DashboardListsViewModel();
            var lists = _uow.DashboardListsRepo.GetAll();
            dashboardListsViewModel.DashboardLists = Mapper.Map<List<DashboardListsViewModel>>(lists);
            return View(dashboardListsViewModel);
        }
        public ActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Create(DashboardListsViewModel dashboardListsViewModel)
        {
            //if (ModelState.IsValid)
            //{
            if (dashboardListsViewModel.Id == 0)
            {
                if (IsDashboardlistExist(dashboardListsViewModel.Name.Trim(), dashboardListsViewModel.Id))
                {
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, CRM.Application.Core.Resources.Administration.DashboardLists.DashboardListNameExists);
                }
                else
                {
                    var dashboardList = Mapper.Map<DashboardList>(dashboardListsViewModel);
                    _uow.DashboardListsRepo.Add(dashboardList);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.DashboardLists.ListSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                }
            }
            else
            {
                var dashboardList = Mapper.Map<DashboardList>(dashboardListsViewModel);
                _uow.DashboardListsRepo.Update(dashboardList);
                ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.DashboardLists.ListUpdated;
                ReponseViewModel.TransactionType = TransactionType.Update.ToString();
            }
            try
            {
                _uow.SaveChanges();
                return Json(new { response = new { ReponseViewModel.ResponseMessage, ReponseViewModel.TransactionType } }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //}
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public ActionResult Edit(int Id)
        {
            DashboardListsViewModel dashboardListsViewModel = new DashboardListsViewModel();
            dashboardListsViewModel = Mapper.Map<DashboardListsViewModel>(_uow.DashboardListsRepo.Find(Id));
            return View("Create", dashboardListsViewModel);
        }
        public ActionResult IsListNameExist(string Name, int? Id)
        {
            int resultCount = 0;
            if (Id == null)
            {
                resultCount = _uow.DashboardListsRepo.Search(x => x.Name == Name).Count();
            }
            else
            {
                resultCount = _uow.DashboardListsRepo.Search(x => x.Name == Name && x.Id != Id).Count();


            }
            return resultCount > 0 ?
                  Json(false, JsonRequestBehavior.AllowGet) :
                  Json(true, JsonRequestBehavior.AllowGet);
        }
        private bool IsDashboardlistExist(string Name, int Id)
        {
            int resultCount = 0;
            if (Id == 0)
            {
                resultCount = _uow.DashboardListsRepo.Search(x => x.Name == Name).Count();
            }
            else
            {
                resultCount = _uow.DashboardListsRepo.Search(x => x.Name == Name && x.Id != Id).Count();


            }
            return resultCount > 0;
        }
        public ActionResult IsListColumnNameExist(string Name, int? Id, int DashboardListId)
        {
            int resultCount = 0;
            if (Id == 0)
            {
                resultCount = _uow.DashboardListColumnsRepo.Search(x => x.Name == Name && x.DashboardListId == DashboardListId).Count();
            }
            else
            {
                resultCount = _uow.DashboardListColumnsRepo.Search(x => x.Name == Name && x.Id != Id && x.DashboardListId == DashboardListId).Count();


            }
            return resultCount > 0 ?
                  Json(false, JsonRequestBehavior.AllowGet) :
                  Json(true, JsonRequestBehavior.AllowGet);
        }

        private bool IsColumnNameExist(string Name, int Id, int DashboardListId)
        {
            int resultCount = 0;
            if (Id == 0)
            {
                resultCount = _uow.DashboardListColumnsRepo.Search(x => x.Name == Name && x.DashboardListId == DashboardListId).Count();
            }
            else
            {
                resultCount = _uow.DashboardListColumnsRepo.Search(x => x.Name == Name && x.Id != Id && x.DashboardListId == DashboardListId).Count();


            }
            return resultCount > 0;
        }
        public ActionResult DashboardListColumns(int dashboardListId)
        {
            DashboardListColumnsViewModel dashboardListColumnsViewModel = new DashboardListColumnsViewModel();
            dashboardListColumnsViewModel.DashboardListId = dashboardListId;
            return View(dashboardListColumnsViewModel);
        }
        public ActionResult DashboardListColumnsForm(int dashboardListId)
        {
            DashboardListColumnsViewModel dashboardListColumnsViewModel = new DashboardListColumnsViewModel();
            dashboardListColumnsViewModel.DashboardListId = dashboardListId;
            return View(dashboardListColumnsViewModel);
        }
        public ActionResult DashboardListColumnsList(int dashboardListId)
        {
            DashboardListColumnsViewModel dashboardListColumnsViewModel = new DashboardListColumnsViewModel();
            dashboardListColumnsViewModel.DashboardListId = dashboardListId;
            var res = _uow.DashboardListColumnsRepo.Search(x => x.DashboardListId == dashboardListId).ToList();
            dashboardListColumnsViewModel.DashboardListColumnsList = Mapper.Map<List<DashboardListColumnsViewModel>>(res);
            return View(dashboardListColumnsViewModel);
        }
        [HttpPost]
        public ActionResult CreateDashboardListColumns(DashboardListColumnsViewModel dashboardListColumnsViewModel)
        {
            //if (ModelState.IsValid)
            //{
            if (dashboardListColumnsViewModel.Id == 0)
            {
                if(IsColumnNameExist(dashboardListColumnsViewModel.Name.Trim(), dashboardListColumnsViewModel.Id, dashboardListColumnsViewModel.DashboardListId))
                {
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, Application.Core.Resources.Administration.DashboardLists.ColumnNameExist);
                }
                else
                {
                    var dashboardListColumn = Mapper.Map<DashboardListColumn>(dashboardListColumnsViewModel);
                    _uow.DashboardListColumnsRepo.Add(dashboardListColumn);
                    ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.DashboardLists.ListSaved;
                    ReponseViewModel.TransactionType = TransactionType.Create.ToString();
                }
            }
            else
            {
                var dashboardListColumn = Mapper.Map<DashboardListColumn>(dashboardListColumnsViewModel);
                _uow.DashboardListColumnsRepo.Update(dashboardListColumn);
                ReponseViewModel.ResponseMessage = Application.Core.Resources.Administration.DashboardLists.ListUpdated;
                ReponseViewModel.TransactionType = TransactionType.Update.ToString();
            }
            try
            {
                _uow.SaveChanges();
                dashboardListColumnsViewModel.DashboardListColumnsList =
                    Mapper.Map<List<DashboardListColumnsViewModel>>(_uow.DashboardListColumnsRepo.Search(x => x.DashboardListId == dashboardListColumnsViewModel.DashboardListId).ToList());
                return View("DashboardListColumnsList", dashboardListColumnsViewModel);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //}
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}