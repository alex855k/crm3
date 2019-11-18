using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM.Web.App_Helpers
{
    public class JsonDateFilterAttribute : ActionFilterAttribute
    {
        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    if (filterContext.Result is JsonResult == false)
        //    {
        //        return;
        //    }
        //    filterContext.Result = new JsonNetResult(
        //        (JsonResult)filterContext.Result);
        //}

        private class JsonNetResult : JsonResult
        {
            public JsonNetResult(JsonResult jsonResult)
            {
                this.ContentEncoding = jsonResult.ContentEncoding;
                this.ContentType = jsonResult.ContentType;
                this.Data = jsonResult.Data;
                this.JsonRequestBehavior = jsonResult.JsonRequestBehavior;
                this.MaxJsonLength = jsonResult.MaxJsonLength;
                this.RecursionLimit = jsonResult.RecursionLimit;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                var isMethodGet = string.Equals(
                    context.HttpContext.Request.HttpMethod,
                    "GET",
                    StringComparison.OrdinalIgnoreCase);

                if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet
                    && isMethodGet)
                {
                    throw new InvalidOperationException(
                        "GET not allowed! Change JsonRequestBehavior to AllowGet.");
                }

                var response = context.HttpContext.Response;

                response.ContentType = string.IsNullOrEmpty(this.ContentType)
                    ? "application/json"
                    : this.ContentType;

                if (this.ContentEncoding != null)
                {
                    response.ContentEncoding = this.ContentEncoding;
                }

                if (this.Data != null)
                {
                    response.Write(JsonConvert.SerializeObject(this.Data));
                }
            }
        }
    }
}