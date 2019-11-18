using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class EmailProtocol
    {
        [Key, Column(Order = 1)]
        public int Port { get; set; }
        [Key, Column(Order = 2)]
        public string Name { get; set; }
    }
}
