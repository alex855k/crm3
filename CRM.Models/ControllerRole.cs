using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class ControllerRole
    {
        [Key]
        public int Id { get; set; }
        public string RoleId { get; set; }
        public virtual Role Role { get; set; }
        public int ControllerId { get; set; }
        public virtual ApplicationController Controller { get; set; }
    }
}
