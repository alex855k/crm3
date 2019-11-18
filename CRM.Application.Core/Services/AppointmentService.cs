using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Web;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.IO;

namespace CRM.Application.Core.Services
{
    public class AppointmentService
    {
        UnitofWork _uow = new UnitofWork();
        public void CreateCustomerAppointment(CustomerAppointmentViewModel customerAppointmentViewModel)
        {
            CustomerAppointment customerAppointment = new CustomerAppointment();
            customerAppointment.Date = customerAppointmentViewModel.Date;
            customerAppointment.Subject = customerAppointmentViewModel.Subject;
            customerAppointment.StartTime = new TimeSpan(customerAppointmentViewModel.StartTimeHour, customerAppointmentViewModel.StartTimeMinute, 0);
            customerAppointment.EndTime = new TimeSpan(customerAppointmentViewModel.EndTimeHour, customerAppointmentViewModel.EndTimeMinute, 0);
            customerAppointment.AppointmentDescription = customerAppointmentViewModel.AppointmentDescription;
            customerAppointment.AppointmentColor = customerAppointmentViewModel.AppointmentColor;
            customerAppointment.AppointmentIcon = customerAppointmentViewModel.AppointmentIcon;
            customerAppointment.UserId = HttpContext.Current.User.Identity.GetUserId();
            customerAppointment.CustomerId = customerAppointmentViewModel.SelectedCustomerId;
            customerAppointment.AppointmentNote = customerAppointmentViewModel.AppointmentNote;
            customerAppointment.IsSaveAppointNote = customerAppointmentViewModel.IsSaveAppointmentNote;
            if (customerAppointmentViewModel.IsSaveAppointmentNote)
            {
                _uow.CustomerNotesRepo.Add(new CustomerNote
                {
                    Note = customerAppointmentViewModel.AppointmentNote,
                    UserId = HttpContext.Current.User.Identity.GetUserId(),
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

        public void UpdateCustomerAppointment(CustomerAppointmentViewModel customerAppointmentViewModel)
        {
            var customerAppointment = _uow.CustomerAppointmentsRepo.Find(customerAppointmentViewModel.Id);
            customerAppointment.Date = customerAppointmentViewModel.Date;
            customerAppointment.Subject = customerAppointmentViewModel.Subject;
            customerAppointment.StartTime = new TimeSpan(customerAppointmentViewModel.StartTimeHour, customerAppointmentViewModel.StartTimeMinute, 0);
            customerAppointment.EndTime = new TimeSpan(customerAppointmentViewModel.EndTimeHour, customerAppointmentViewModel.EndTimeMinute, 0);
            customerAppointment.AppointmentDescription = customerAppointmentViewModel.AppointmentDescription;
            customerAppointment.AppointmentColor = customerAppointmentViewModel.AppointmentColor;
            customerAppointment.AppointmentIcon = customerAppointmentViewModel.AppointmentIcon;
            customerAppointment.UserId = HttpContext.Current.User.Identity.GetUserId();
            customerAppointment.CustomerId = customerAppointmentViewModel.SelectedCustomerId;
            customerAppointmentViewModel.AppointmentNote = customerAppointment.AppointmentNote;
            var appointmentNote = _uow.CustomerNotesRepo.Search(x => x.CustomerAppointmentId.Value == customerAppointment.Id).SingleOrDefault();
            _uow.CustomerNotesRepo.Update(new CustomerNote
            {
                Note = customerAppointmentViewModel.AppointmentNote,
                CustomerNoteDemoId = customerAppointmentViewModel.CustomerNotesDemoId,
                CustomerNoteReportId = customerAppointmentViewModel.CustomerNotesReportId,
                CustomerNoteVisitTypeId = customerAppointmentViewModel.CustomerNotesVisitTypeId,
                UpdateNoteUserId = HttpContext.Current.User.Identity.GetUserId(),
                UpdateNoteDate = DateTime.Now
            });


            if (customerAppointmentViewModel.IsResendEmails)
            {
                var res = _uow.AppointmentEmailsRepo.Search(x => x.CustomerAppointmentId == customerAppointmentViewModel.Id).ToList();
                _uow.AppointmentEmailsRepo.RemoveRange(res);
            }
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

            _uow.CustomerAppointmentsRepo.Update(customerAppointment);

        }

        public MemoryStream GenerateAppointmentIcal(DateTime startDate,DateTime endDate,string appointmentSubject,string appointmentDetails)
        {
            var e = new CalendarEvent
            {
                Class = "PUBLIC",
                Summary = appointmentSubject,
                Created = new CalDateTime(DateTime.Now),
                Description = appointmentDetails,
                Start = new CalDateTime(startDate),
                End = new CalDateTime(endDate),
                Sequence = 0,
                Uid = Guid.NewGuid().ToString(),
            };

            var calendar = new Calendar();
            calendar.Events.Add(e);

            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(calendar);
            var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);
            MemoryStream ms = new MemoryStream(bytesCalendar);
            return ms;
        }

    }
}
