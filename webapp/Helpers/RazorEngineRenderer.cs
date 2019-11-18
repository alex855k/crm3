using RazorEngine;
using System.IO;
using System.Web.Mvc;

namespace CRM.Web.Helpers
{
    public static class RazorEngineRender
    {
        public static string PartialViewToString<T>(string templatePath, string viewName, T model)
        {
            string text = File.ReadAllText(Path.Combine(templatePath, viewName));
            string renderedText = Razor.Parse(text, model);

            return renderedText;
        }
        public static string PartialViewToString(string templatePath, string viewName)
        {
            string text = File.ReadAllText(Path.Combine(templatePath, viewName));
            string renderedText = Razor.Parse(text);

            return renderedText;
        }
        public static string RazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
    }
}