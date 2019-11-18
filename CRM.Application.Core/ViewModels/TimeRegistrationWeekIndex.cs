
using System.Collections.Generic;
using System.Web.Mvc;

namespace CRM.Application.Core.ViewModels
{
    public class TimeRegistrationWeekIndex
    {
        public IList<TimeRegistrationViewModel> TimeRegs { get; set; }
        public IList<SelectListItem> ViewScope { get; set; }
    }
}
