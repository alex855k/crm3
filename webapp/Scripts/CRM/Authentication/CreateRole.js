$(document).ready(function () {
    $("#createRolesForm").submit(function (e) {
        e.preventDefault();
        if ($("#createRolesForm").valid()) {
            $.ajax({
                url: "/Roles/Create",
                data: $("#createRolesForm").serialize(),
                type: "POST",
                success: function (data, textStatus, jqXHR) {
                    debugger;
                    var response = new TransactionResponse();
                    response = data.response;
                    if (response.TransactionType == TransactionType[TransactionType.Create]) {
                        toastr.success(response.ResponseMessage);
                        ResetForm();
                    }
                    else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                        toastr.success(response.ResponseMessage);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error('save role failed');
                }
            });
        }
    });
    function ResetForm() {
        $('input', '#createRolesForm')
            .not("#hiddenId")
            .val('');
    }
});
//# sourceMappingURL=CreateRole.js.map