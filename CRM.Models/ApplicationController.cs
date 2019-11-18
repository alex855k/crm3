using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class ApplicationController
    {
        public ApplicationController()
        { }
        //public ApplicationController(int id, string controllerName, string actionName, string displayName, bool isDisabled, bool isPartialPage, int applicationControllerCategoryId)
        //{
        //    this.Id = id;
        //    this.ControllerName = controllerName;
        //    this.ActionName = actionName;
        //    this.DisplayName = displayName;
        //    this.IsDisabled = isDisabled;
        //    this.IsPartialPage = isPartialPage;
        //    this.ApplicationControllerCategoryId = applicationControllerCategoryId;
        //}


        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string DisplayName { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsPartialPage { get; set; }
        public ApplicationControllerCategory ApplicationControllerCategory { get; set; }
        public int ApplicationControllerCategoryId { get; set; }
    }
}
