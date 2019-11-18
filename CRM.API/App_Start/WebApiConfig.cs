using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Web.Http;

namespace CRM.API.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "OrdersApi",                
                routeTemplate: "api/orders/{action}/{id}",
                defaults: new { controller = "OrderAPIv1", id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "CustomersApi",
                routeTemplate: "api/customers/{action}/{id}",
                defaults: new { controller = "CustomersAPIv1", id = RouteParameter.Optional }
            );
            // Default always last
            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new MediaTypeHeaderValue("text/html"));

            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}