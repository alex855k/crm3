$("#btnSaveCustomerToList").click(function () {
    debugger;
    var userListsIds = [];
    var customerId = 0;
    if ($("#hiddenSelectedCustomerId").val() != undefined)
        var customerId = $("#hiddenSelectedCustomerId").val();
    else
        var customerId = $("#customerId").val();
    $(".checkUserDashboardList").each(function (index, elem) {
        debugger;
        if ($(this).is(':checked'))
            userListsIds.push($(this).attr("id"));
    });
    if (userListsIds.length > 0) {
        $.ajax({
            url: "/Customers/SaveCustomerInUserLists",
            data: { "customerId": customerId, "userDashboardListIds": userListsIds },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                debugger;
                ListUserListsCheckboxes(data);
                if ($("#hiddenSelectedCustomerId").val() != undefined)
                    $("#hiddenSelectedCustomerId").val("");
                toastr.success("customer Added to lists");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("customer add to list failed");
            }
        })
    } else {
        $("#lblError").text("");
        return;
    }
})


function ListUserListsCheckboxes(data) {
    $("#divUserLists").empty();
    $.each(data, function (index, elem) {
        debugger;
        $("#divUserLists").append(`
                    <label class='checkbox'>
                    <input id=`+ elem.Id + ` type='checkbox' name='checkbox' class="checkUserDashboardList" >
                    <i></i> &nbsp; `+ elem.Name + `
                    </label>`)
    });
}