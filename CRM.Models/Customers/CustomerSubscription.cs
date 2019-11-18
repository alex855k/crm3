using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class CustomerSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CrayonSupcriptionId { get; set; }
        public int ProductId { get; set; }

        public string Reference1 { get; set; }
        public string Reference2 { get; set; }

        public string AdditionalData { get; set; }

        public static explicit operator CustomerSubscription(int v)
        {
            throw new NotImplementedException();
        }
    }
}
