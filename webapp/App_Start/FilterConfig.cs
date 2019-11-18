#region Using

using CRM.Web.App_Helpers;
using System.Web.Mvc;

#endregion

namespace CRM.Web
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new JsonDateFilterAttribute());
        }
    }
}