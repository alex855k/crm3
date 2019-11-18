using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class ComasysOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public ComasysProduct Product { get; set; }
        public int LicenseQuantity { get; set; }
        public int CustomerId { get; set; }
    }
}
