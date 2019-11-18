$(document).ready(function () {
    $("#createListForm").submit(function (e) {
        debugger;
        e.preventDefault();
        if ($("#createListForm").valid()) {
            $.ajax({
                url: "/DashboardLists/Create",
                data: $("#createListForm").serialize(),
                type: "POST",
                success: function (data, textStatus, jqXHR) {
                    var response = new TransactionResponse();
                    response = data.response;
                    if (response.TransactionType == TransactionType[TransactionType.Create]) {
                        toastr.success(response.ResponseMessage);
                        ResetForm("#createListForm", "");
                    }
                    else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                        toastr.success(response.ResponseMessage);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(errorThrown);
                }
            });
        }
    });
    $("#dashboardListColumnsForm").submit(function (e) {
        debugger;
        e.preventDefault();
        if ($("#dashboardListColumnsForm").valid()) {
            $.ajax({
                url: "/DashboardLists/CreateDashboardListColumns",
                data: $("#dashboardListColumnsForm").serialize(),
                type: "POST",
                success: function (data, textStatus, jqXHR) {
                    debugger;
                    toastr.success('columns saved');
                    $("#dashboardListColumnsListDiv").html(data);
                    ResetForm("#dashboardListColumnsForm", "#HiddenDashboardListId");
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(errorThrown);
                }
            });
        }
    });
    function ResetForm(formSelector, fieldSelector) {
        $(':input', formSelector)
            .not(fieldSelector)
            .val('');
    }
});
//# sourceMappingURL=CreateList.js.map