using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace CRM.Application.Core.ViewModels
{
    public class DynamicTableViewModel<T> where T : class
    {
        public DynamicTableViewModel()
        {
            QueryParameters = new List<ExpressionBuilderParameters>();
        }
        const int DefaultPageNumber = 1;
        public int PageNumber { get; set; } = DefaultPageNumber;
        public int PageSize
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["PageSize"].ToString());
            }
            set
            {
                PageSize = value;
            }
        }
        public int QueryCount { get; set; }
        public int TableCount { get; set; }
        public string DefaultOrderBy { get; set; }
        public string DefaultDirection { get; set; } = "Asc";
        public string OrderBy { get; set; }
        public string Direction { get; set; }
        public int FirstItemOnPage { get; set; }
        public int LastItemOnPage { get; set; }
        public int MyProperty { get; set; }
        public string QueryOperatorComparer { get; set; } = ExpressionType.And.ToString();
        public List<ExpressionBuilderParameters> QueryParameters { get; set; }
        public object WebConfigurationManager { get; private set; }
    }
}
