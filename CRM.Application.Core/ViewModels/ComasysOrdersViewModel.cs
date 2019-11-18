using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class ComasysOrdersViewModel
    {
        public ComasysOrdersViewModel()
        {
            OwnedProducts = new List<OwnedProduct>();
        }

        public int CustomerId { get; set; }
        public List<OwnedProduct> OwnedProducts { get; set; }
        public List<ComasysProduct> ProductList { get; set; }
        public int Amount { get; set; }


    }
}
