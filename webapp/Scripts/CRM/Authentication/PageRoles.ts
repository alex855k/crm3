$(document).ready(function () {

    $("#btnSave").click(function (e) {
        var assignedRolesId = $("#pageRolesDuallistbox").val();
        var controllerId = $("#hiddenControllerId").val();
        $.ajax({
            url: "/Roles/AssignControllerRole",
            data: { "controllerId": controllerId, "assignedRolesId": assignedRolesId},
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                toastr.success("roles saved");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.success("roles save failed");
            }
        })
    })
});