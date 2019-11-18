using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class DailyReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public int KmFrom { get; set; }
        public int KmTo { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
