using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace CRM.Application.Core.ViewModels
{
    public class ProcedureViewModel : DynamicTableViewModel<ProcedureViewModel>
    {
        public ProcedureViewModel()
        {
            ProcedureItems = new List<ProcedureItem>();
        }

        public int Id { get; set; }
        [Required(ErrorMessageResourceName = "ProcedureTitleRequired", ErrorMessageResourceType = typeof(Resources.Procedures.Procedure))]
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Edited { get; set; }
        public string ImagePath { get; set; }
        public string PDFName { get; set; }
        public IPagedList<Procedure> ProcedureList { get; set; }

        public List<ProcedureItem> ProcedureItems { get; set; }
        public string ImageFolderPath { get; set; }
    }
}
