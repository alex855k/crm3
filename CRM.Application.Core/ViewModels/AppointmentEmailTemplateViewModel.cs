namespace CRM.Application.Core.ViewModels
{
    public class AppointmentEmailTemplateViewModel
    {
        public string AppointmentSubject { get; set; }
        public string AppointmentNote { get; set; }
        public bool IsSalesPerson { get; set; }
        public string AppointmentDate { get; set; }
        public string AppointmentToTime { get; set; }
    }
}
