using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.Application.Core.ViewModels
{
    [Serializable]
    public class ExpressionBuilderParameters
    {
        public string SearchKey { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public bool HasSpecialConditionQueryOperatorComparer { get; set; }
    }
}