$(document).ready(function () {
    $("#btnSave").click(function (e) {
        debugger;
        var unassignedRoles = [];
        $.each($("#bootstrap-duallistbox-nonselected-list_duallistbox_demo option"), function () {
            unassignedRoles.push($(this).val());
        });
        var assignedRoles = $("#rolesDuallistbox").val();
        var userId = $("#hiddenUserId").val();
        debugger;
        $.ajax({
            url: "/Users/AssignUserRoles",
            data: { "userId": userId, "assignedRoles": assignedRoles, "unassignedRoles": unassignedRoles },
            type: "POST",
            success: function (data, textStatus, jqXHR) {
                debugger;
                toastr.success("roles saved");
            },
            error: function (jqXHR, textStatus, errorThrown) {
            }
        });
    });
});
//# sourceMappingURL=UserRole.js.map