using System.Collections.Generic;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class GenericViewModel<T,T2> where T : class
                                        where T2 : class
    {
        public GenericViewModel()
        {
            QueryParameters = new List<ExpressionBuilderParameters>();
        }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int QueryCount { get; set; }
        public int TableCount { get; set; }
        public string DefaultOrderBy { get; set; }
        public string DefaultDirection { get; set; }
        public string OrderBy { get; set; }
        public string Direction { get; set; }
        public string QueryOperatorComparer { get; set; }
        public string SpecialConditionQueryOperatorComparer { get; set; }

        public string TablePartial { get; set; }
        public string PagingPartial { get; set; }
        public List<ExpressionBuilderParameters> QueryParameters { get; set; }
        public StaticPagedList<T2> ResultList { get; set; }
    }
}
