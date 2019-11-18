using AutoMapper;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using CRM.Web.App_Helpers;
using System.Net.Mail;
using System.Web.Configuration;
using System.Net;
using CRM.Application.Core.Services;
using CRM.Web.Helpers;

namespace CRM.Web.Controllers
{

    public class CalendarController : Controller
    {
        // GET: Calendar
        UnitofWork _uow = new UnitofWork();
        public string CompanyMailAddress => WebConfigurationManager.AppSettings["CompanyMail:EmailAddress"].ToString();
        public string CompanyMailPassword => WebConfigurationManager.AppSettings["CompanyMail:EmailPasword"].ToString();
        public string CompanyMailServer => WebConfigurationManager.AppSettings["CompanyMail:EmailServer"].ToString();
        public int CompanyMailPort => int.Parse(WebConfigurationManager.AppSettings["CompanyMail:EmailPort"].ToString());
        AppointmentService _appointmentService = new AppointmentService();
        DailyReportService dailyReportService = new DailyReportService();
        public ActionResult Index()
        {
            CustomerAppointmentViewModel customerAppointmentViewModel = new CustomerAppointmentViewModel();
            customerAppointmentViewModel.Hours = CRM.Application.Core.Constants.Constants.Hours;
            customerAppointmentViewModel.Minutes = CRM.Application.Core.Constants.Constants.Minutes;
            var appointments = _uow.CustomerAppointmentsRepo.GetAll("Customer");
            foreach (var appointment in appointments)
            {
                customerAppointmentViewModel.AppointmentsList.Add(new Appointments
                {
                    id = appointment.Id,
                    title = appointment.Customer.CompanyName,
                    start = appointment.Date,
                    description = appointment.Subject,
                    className = new object[2] { "events", appointment.AppointmentColor },
                    icon = appointment.AppointmentIcon,
                    allDay = false,
                });
            }
            var customers = _uow.CustomersRepo.Search(x => x.CustomerStatusId == 1).ToList();
            customerAppointmentViewModel.CustomersList = Mapper.Map<List<CustomerViewModel>>(customers);
            customerAppointmentViewModel.CustomerNotesReportList.AddRange(GetCustomerNoteReport());
            customerAppointmentViewModel.CustomerNotesVisitTypeList.AddRange(GetCustomerNoteVisitType());
            customerAppointmentViewModel.CustomerNotesDemoList.AddRange(GetCustomerNoteDemo());
            return View(customerAppointmentViewModel);
        }
        [HttpPost]
        [JsonDateFilter]
        public ActionResult CreateAppointment(CustomerAppointmentViewModel customerAppointmentViewModel)
        {
            if (customerAppointmentViewModel.Id != 0)
            {
                var customerAppointment = _uow.CustomerAppointmentsRepo.Find(customerAppointmentViewModel.Id);
                customerAppointment.Date = customerAppointmentViewModel.Date;
                customerAppointment.Subject = customerAppointmentViewModel.Subject;
                customerAppointment.StartTime = new TimeSpan(customerAppointmentViewModel.StartTimeHour, customerAppointmentViewModel.StartTimeMinute, 0);
                customerAppointment.EndTime = new TimeSpan(customerAppointmentViewModel.EndTimeHour, customerAppointmentViewModel.EndTimeMinute, 0);
                customerAppointment.AppointmentDescription = customerAppointmentViewModel.AppointmentDescription;
                customerAppointment.AppointmentColor = customerAppointmentViewModel.AppointmentColor;
                customerAppointment.AppointmentIcon = customerAppointmentViewModel.AppointmentIcon;
                customerAppointment.UserId = User.Identity.GetUserId();
                customerAppointment.AppointmentNote = customerAppointmentViewModel.AppointmentNote;
                _uow.CustomerAppointmentsRepo.Update(customerAppointment);
                var appointmentNote = _uow.CustomerNotesRepo.Search(x => x.CustomerAppointmentId.Value == customerAppointment.Id).SingleOrDefault();
                if (appointmentNote != null)
                {
                    appointmentNote.Note = customerAppointmentViewModel.AppointmentNote;
                    appointmentNote.CustomerNoteDemoId = customerAppointmentViewModel.CustomerNotesDemoId;
                    appointmentNote.CustomerNoteReportId = customerAppointmentViewModel.CustomerNotesReportId;
                    appointmentNote.CustomerNoteVisitTypeId = customerAppointmentViewModel.CustomerNotesVisitTypeId;
                    appointmentNote.UpdateNoteUserId = User.Identity.GetUserId();
                    appointmentNote.UpdateNoteDate = DateTime.Now;
                    _uow.CustomerNotesRepo.Update(appointmentNote);
                }

                var res = _uow.AppointmentEmailsRepo.Search(x => x.CustomerAppointmentId == customerAppointmentViewModel.Id).ToList();
                    _uow.AppointmentEmailsRepo.RemoveRange(res);


                if (customerAppointmentViewModel.AppointmentCustomerEmails != null && customerAppointmentViewModel.AppointmentCustomerEmails.Length > 0)
                {
                    foreach (var customerEmail in customerAppointmentViewModel.AppointmentCustomerEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail { Email = customerEmail, AppointmentEmailTypeId = 1, CustomerAppointmentId = customerAppointmentViewModel.Id });
                    }
                }
                if (customerAppointmentViewModel.AppointmentSalesPersonEmails != null && customerAppointmentViewModel.AppointmentSalesPersonEmails.Length > 0)
                {

                    foreach (var salespersonEmail in customerAppointmentViewModel.AppointmentSalesPersonEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail { Email = salespersonEmail, AppointmentEmailTypeId = 2, CustomerAppointmentId = customerAppointmentViewModel.Id });
                    }
                }

        
            }
            else
            {

                var customerAppointment = new CustomerAppointment();
                customerAppointment.Date = customerAppointmentViewModel.Date;
                customerAppointment.Subject = customerAppointmentViewModel.Subject;
                customerAppointment.StartTime = new TimeSpan(customerAppointmentViewModel.StartTimeHour, customerAppointmentViewModel.StartTimeMinute, 0);
                customerAppointment.EndTime = new TimeSpan(customerAppointmentViewModel.EndTimeHour, customerAppointmentViewModel.EndTimeMinute, 0);
                customerAppointment.AppointmentDescription = customerAppointmentViewModel.AppointmentDescription;
                customerAppointment.AppointmentColor = customerAppointmentViewModel.AppointmentColor;
                customerAppointment.AppointmentIcon = customerAppointmentViewModel.AppointmentIcon;
                customerAppointment.UserId = User.Identity.GetUserId();
                customerAppointment.CustomerId = customerAppointmentViewModel.SelectedCustomerId;
                customerAppointment.AppointmentNote = customerAppointmentViewModel.AppointmentNote;
                customerAppointment.IsSaveAppointNote = customerAppointmentViewModel.IsSaveAppointmentNote;
                if (customerAppointmentViewModel.IsSaveAppointmentNote)
                {
                    _uow.CustomerNotesRepo.Add(new CustomerNote
                    {
                        Note = customerAppointmentViewModel.AppointmentNote,
                        UserId = User.Identity.GetUserId(),
                        CustomerAppointmentId = customerAppointmentViewModel.Id,
                        CustomerNoteReportId = customerAppointmentViewModel.CustomerNotesReportId,
                        CustomerNoteVisitTypeId = customerAppointmentViewModel.CustomerNotesVisitTypeId,
                        CustomerNoteDemoId = customerAppointmentViewModel.CustomerNotesDemoId,
                        CustomerId = customerAppointmentViewModel.SelectedCustomerId,
                        CreationDate = DateTime.Now
                    });
                }
                if (customerAppointmentViewModel.AppointmentCustomerEmails != null && customerAppointmentViewModel.AppointmentCustomerEmails.Length > 0)
                {
                    foreach (var customerEmail in customerAppointmentViewModel.AppointmentCustomerEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail
                        {
                            Email = customerEmail,
                            AppointmentEmailTypeId = 1,
                            CustomerAppointmentId = customerAppointmentViewModel.Id
                        });
                    }
                }
                if (customerAppointmentViewModel.AppointmentSalesPersonEmails != null && customerAppointmentViewModel.AppointmentSalesPersonEmails.Length > 0)
                {

                    foreach (var salespersonEmail in customerAppointmentViewModel.AppointmentSalesPersonEmails)
                    {
                        _uow.AppointmentEmailsRepo.Add(new AppointmentEmail { Email = salespersonEmail, AppointmentEmailTypeId = 2, CustomerAppointmentId = customerAppointmentViewModel.Id });
                    }
                }
                _uow.CustomerAppointmentsRepo.Add(customerAppointment);
            }
            try
            {
                _uow.SaveChanges();
                if (customerAppointmentViewModel.IsSaveAppointmentNote)
                    dailyReportService.CreateDailyReport();
                SendAppointmentEmails(customerAppointmentViewModel, customerAppointmentViewModel.IsResendEmails);

            }
            catch (Exception ex)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Failed");
            }

            var customerAppointmentsViewModel = new CustomerAppointmentViewModel();
            var appointments = _uow.CustomerAppointmentsRepo.GetAll();
            foreach (var appointment in appointments)
            {
                customerAppointmentsViewModel.AppointmentsList.Add(new Appointments
                {
                    id = appointment.Id,
                    title = appointment.Subject,
                    start = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.StartTime.Hours, appointment.StartTime.Minutes, 0),
                    description = appointment.AppointmentDescription,
                    className = new object[2] { "events", appointment.AppointmentColor },
                    icon = appointment.AppointmentIcon,
                    allDay = false,
                });
            }
            return Json(customerAppointmentsViewModel, JsonRequestBehavior.AllowGet);
        }


        private void SendAppointmentEmails(CustomerAppointmentViewModel customerAppointmentViewModel, bool resendEmail)
        {
            //if (resendEmail)
            //{
                var client = new SmtpClient(CompanyMailServer, CompanyMailPort)
                {
                    Credentials = new NetworkCredential(CompanyMailAddress, CompanyMailPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,

                };
                var TemplateFolderPath = Server.MapPath("~/Views/EmailTemplates");
                var customersMailBody = RazorEngineRender.PartialViewToString(TemplateFolderPath, "CustomersAppointmentTemplate.cshtml", new AppointmentEmailTemplateViewModel
                {
                    AppointmentSubject = customerAppointmentViewModel.AppointmentDescription,
                    AppointmentDate = new DateTime(customerAppointmentViewModel.Date.Year,
                    customerAppointmentViewModel.Date.Month,
                    customerAppointmentViewModel.Date.Day,
                    customerAppointmentViewModel.StartTimeHour,
                    customerAppointmentViewModel.StartTimeMinute,
                    0).ToString(),

                    AppointmentToTime = new DateTime(customerAppointmentViewModel.Date.Year,
                    customerAppointmentViewModel.Date.Month,
                    customerAppointmentViewModel.Date.Day,
                    customerAppointmentViewModel.EndTimeHour,
                    customerAppointmentViewModel.EndTimeMinute,
                    0).ToShortTimeString(),

                    IsSalesPerson = false
                });
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(CompanyMailAddress);
                mail.Subject = "Customer Appointment";
                mail.Body = customersMailBody;
                mail.IsBodyHtml = true;
               var ical = _appointmentService.GenerateAppointmentIcal(new DateTime(customerAppointmentViewModel.Date.Year,
                    customerAppointmentViewModel.Date.Month,
                    customerAppointmentViewModel.Date.Day,
                    customerAppointmentViewModel.StartTimeHour,
                    customerAppointmentViewModel.StartTimeMinute,
                    0), new DateTime(customerAppointmentViewModel.Date.Year,
                    customerAppointmentViewModel.Date.Month,
                    customerAppointmentViewModel.Date.Day,
                    customerAppointmentViewModel.EndTimeHour,
                    customerAppointmentViewModel.EndTimeMinute,
                    0),
                    customerAppointmentViewModel.Subject, customerAppointmentViewModel.AppointmentDescription);
                Attachment attachment = new System.Net.Mail.Attachment(ical, "appointment.ics", "text/calendar");
                mail.Attachments.Add(attachment);
                if (customerAppointmentViewModel.AppointmentCustomerEmails != null && customerAppointmentViewModel.AppointmentCustomerEmails.Length > 0)
                {
                    foreach (var mailToCustomer in customerAppointmentViewModel.AppointmentCustomerEmails)
                    {
                        mail.To.Add(mailToCustomer);
                    }
                    client.Send(mail);

                }

                var salespersonMailBody = RazorEngineRender.PartialViewToString(TemplateFolderPath, "CustomersAppointmentTemplate.cshtml", new AppointmentEmailTemplateViewModel
                {
                    AppointmentSubject = customerAppointmentViewModel.AppointmentDescription,
                    AppointmentNote = customerAppointmentViewModel.AppointmentNote,
                    AppointmentDate = new DateTime(customerAppointmentViewModel.Date.Year,
                    customerAppointmentViewModel.Date.Month,
                    customerAppointmentViewModel.Date.Day,
                    customerAppointmentViewModel.StartTimeHour,
                    customerAppointmentViewModel.StartTimeMinute,
                    0).ToString(),

                    AppointmentToTime = new DateTime(customerAppointmentViewModel.Date.Year,
                    customerAppointmentViewModel.Date.Month,
                    customerAppointmentViewModel.Date.Day,
                    customerAppointmentViewModel.EndTimeHour,
                    customerAppointmentViewModel.EndTimeMinute,
                    0).ToShortTimeString(),

                    IsSalesPerson = true
                });
                mail.Body = salespersonMailBody;
                mail.To.Clear();
                if (customerAppointmentViewModel.AppointmentSalesPersonEmails != null && customerAppointmentViewModel.AppointmentSalesPersonEmails.Length > 0)
                {
                    foreach (var mailToSalesperson in customerAppointmentViewModel.AppointmentSalesPersonEmails)
                    {
                        mail.To.Add(mailToSalesperson);
                    }
                    client.Send(mail);
                }
            //}
        }

        public ActionResult EditAppointment(int id)
        {
            CustomerAppointment customerAppointment = _uow.CustomerAppointmentsRepo.Find(id);
            CustomerAppointmentViewModel customerAppointmentViewModel = new CustomerAppointmentViewModel();
            customerAppointmentViewModel.Id = customerAppointment.Id;
            customerAppointmentViewModel.Date = customerAppointment.Date;
            customerAppointmentViewModel.Subject = customerAppointment.Subject;
            customerAppointmentViewModel.StartTimeHour = customerAppointment.StartTime.Hours;
            customerAppointmentViewModel.StartTimeMinute = customerAppointment.StartTime.Minutes;
            customerAppointmentViewModel.EndTimeHour = customerAppointment.EndTime.Hours;
            customerAppointmentViewModel.EndTimeMinute = customerAppointment.EndTime.Minutes;
            customerAppointmentViewModel.AppointmentDescription = customerAppointment.AppointmentDescription;
            customerAppointmentViewModel.AppointmentColor = customerAppointment.AppointmentColor;
            customerAppointmentViewModel.AppointmentIcon = customerAppointment.AppointmentIcon;
            customerAppointmentViewModel.SelectedCustomerId = customerAppointment.CustomerId;
            customerAppointmentViewModel.AppointmentNote = customerAppointment.AppointmentNote;
            customerAppointmentViewModel.IsSaveAppointmentNote = customerAppointment.IsSaveAppointNote;
            customerAppointmentViewModel.CustomerStatusId = _uow.CustomersRepo.Search(x => x.Id == customerAppointment.CustomerId).Select(s => s.CustomerStatusId).SingleOrDefault();
            if (customerAppointment.IsSaveAppointNote)
            {
                var appointmentNote = _uow.CustomerNotesRepo.Search(x => x.CustomerAppointmentId.Value == customerAppointment.Id).SingleOrDefault();
                customerAppointmentViewModel.AppointmentNote = appointmentNote.Note;
                customerAppointmentViewModel.CustomerNotesDemoId = appointmentNote.CustomerNoteDemoId;
                customerAppointmentViewModel.CustomerNotesReportId = appointmentNote.CustomerNoteReportId;
                customerAppointmentViewModel.CustomerNotesVisitTypeId = appointmentNote.CustomerNoteVisitTypeId;
            }
            customerAppointmentViewModel.AppointmentCustomerEmails = customerAppointment.AppointmentEmails.Where(x => x.AppointmentEmailTypeId == 1).Select(x => x.Email).ToArray();
            customerAppointmentViewModel.AppointmentSalesPersonEmails = customerAppointment.AppointmentEmails.Where(x => x.AppointmentEmailTypeId == 2).Select(x => x.Email).ToArray();
            return Json(customerAppointmentViewModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAppointmentDate(int id, DateTime date)
        {
            var customerAppointment = _uow.CustomerAppointmentsRepo.Find(id);
            customerAppointment.Date = date;
            _uow.CustomerAppointmentsRepo.Update(customerAppointment);
            _uow.SaveChanges();
            CustomerAppointmentViewModel customerAppointmentsViewModel = new CustomerAppointmentViewModel();
            var appointments = _uow.CustomerAppointmentsRepo.GetAll();
            foreach (var appointment in appointments)
            {
                customerAppointmentsViewModel.AppointmentsList.Add(new Appointments
                {
                    id = appointment.Id,
                    title = appointment.Subject,
                    start = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.StartTime.Hours, appointment.StartTime.Minutes, 0),
                    description = appointment.AppointmentDescription,
                    className = new object[2] { "events", appointment.AppointmentColor },
                    icon = appointment.AppointmentIcon,
                    allDay = false,
                });
            }
            return Json(customerAppointmentsViewModel, JsonRequestBehavior.AllowGet);
        }

        [JsonDateFilter]
        public ActionResult RemoveCalendarAppointment(int appointmentId)
        {
            RemoveAppointmentWithEmailsAndNotes(appointmentId);
            CustomerAppointmentViewModel customerAppointmentsViewModel = new CustomerAppointmentViewModel();
            var appointments = _uow.CustomerAppointmentsRepo.GetAll();
            foreach (var appointment in appointments)
            {
                customerAppointmentsViewModel.AppointmentsList.Add(new Appointments
                {
                    id = appointment.Id,
                    title = appointment.Subject,
                    start = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.StartTime.Hours, appointment.StartTime.Minutes, 0),
                    description = appointment.AppointmentDescription,
                    className = new object[2] { "events", appointment.AppointmentColor },
                    icon = appointment.AppointmentIcon,
                    allDay = false,
                });
            }
            return Json(customerAppointmentsViewModel, JsonRequestBehavior.AllowGet);
        }
        private void RemoveAppointmentWithEmailsAndNotes(int appointmentId)
        {
            var customerAppointment = _uow.CustomerAppointmentsRepo.Find(appointmentId);
          
            if (customerAppointment.AppointmentEmails != null && customerAppointment.AppointmentEmails.Count > 0)
                _uow.AppointmentEmailsRepo.RemoveRange(customerAppointment.AppointmentEmails);
            if (customerAppointment.IsSaveAppointNote)
            {
                var customerAppointmentNote = _uow.CustomerNotesRepo.Search(x => x.CustomerAppointmentId == customerAppointment.Id).SingleOrDefault();
                if (customerAppointmentNote != null)
                    _uow.CustomerNotesRepo.Remove(customerAppointmentNote.Id);
            }
            _uow.CustomerAppointmentsRepo.Remove(customerAppointment.Id);
            _uow.SaveChanges();
        }
        private List<CustomerNoteReport> GetCustomerNoteReport()
        {
            var customerNoteReport = new List<CustomerNoteReport>
            {
                new CustomerNoteReport{Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.NotInReport},
                new CustomerNoteReport { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.InReport },
            };
            return customerNoteReport;
        }
        private List<CustomerNoteVisitType> GetCustomerNoteVisitType()
        {
            var customerNoteVisitType = new List<CustomerNoteVisitType>
            {
                new CustomerNoteVisitType {Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.NoType},
                new CustomerNoteVisitType { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.Appointment },
                new CustomerNoteVisitType { Id = 3, Name = CRM.Application.Core.Resources.Customers.Customer.Canvas },
                new CustomerNoteVisitType { Id = 4, Name = CRM.Application.Core.Resources.Customers.Customer.Telephone},
                new CustomerNoteVisitType { Id = 5, Name = CRM.Application.Core.Resources.Customers.Customer.CallCenter },
            };
            return customerNoteVisitType;
        }
        private List<CustomerNoteDemo> GetCustomerNoteDemo()
        {
            var customerNoteDemo = new List<CustomerNoteDemo>
            {
                new CustomerNoteDemo{Id = 1,Name = CRM.Application.Core.Resources.Customers.Customer.NoDemonstration},
                new CustomerNoteDemo { Id = 2, Name = CRM.Application.Core.Resources.Customers.Customer.DemoCompleted },
            };
            return customerNoteDemo;
        }
    }
}