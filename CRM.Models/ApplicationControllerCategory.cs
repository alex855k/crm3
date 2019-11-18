using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class ApplicationControllerCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<ApplicationController> ApplicationControllers { get; set; }
    }
}
