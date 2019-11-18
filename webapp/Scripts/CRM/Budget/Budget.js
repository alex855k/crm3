$(document).ready(function () {
    $('#createBudgetDate').datepicker({
        format: "mm-yyyy",
        startView: "months",
        minViewMode: "months"
    });

    $('#budgetDate').datepicker({
        format: "mm-yyyy",
        startView: "months",
        minViewMode: "months"
    });

    $("#saveBudget").click(function () {
        var budget = {};
        budget.SalesPersonId = $("#salesPerson").val();
        budget.BudgetAmount = $("#budgetAmount").val();
        budget.BudgetDate= $("#budgetDate").val();
        $.ajax({
            url: "/Budget/CreateBudget",
            data: { "budgetViewModel": budget },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                debugger;
                $('#budgetModal').modal('hide');
                toastr.success("Budget Saved")
                $("#saveBudget").val("");
                $("#budgetAmount").val("");
                $("#budgetDate").val("");
                
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed create budget");
            }
        })
    });
});