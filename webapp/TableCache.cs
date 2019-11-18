using CRM.Application.Core.ViewModels;
using System.Collections.Generic;

namespace CRM.Web
{
    public class TableCache
    {
        public TableCache()
        {
            SearchParameters = new List<ExpressionBuilderParameters>();
        }
        public int PageNumber { get; set; }
        public int LastEditedCustomer { get; set; }
        public List<ExpressionBuilderParameters> SearchParameters{ get; set; }
    }
}