namespace CRM.Application.Core.ViewModels
{
    public class BudgetBarChartViewModel
    {
        public string[] Labels { get; set; }
        public decimal[] BudgetBar { get; set; }
        public decimal[] SalesBar { get; set; }
    }
}
