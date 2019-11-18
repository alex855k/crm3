$().ready(() => {
    var validator = $("#addEmailAccountForm").data("validator");
    if (validator) {
        validator.settings.onkeyup = false;
        validator.settings.onfocusout = false;
        onsubmit = true;
    }

    $("#addEmailAccountForm").submit(function (e) {
        debugger;
        e.preventDefault();
        if ($("#addEmailAccountForm").valid()) {
            //var account = {};
            //account.Id =       $("#accountId").val();
            //account.HostName = $("#HostName").val();
            //account.HostPort = $("#HostPort").val();
            //account.UserName = $("#UserName").val();
            //account.FullAddress = $("#FullAddress").val();
            $.ajax({
                url: "/EmailAccounts/Add",
                data: $("#addEmailAccountForm").serialize(),
                type: "POST",
                success: function (data, textStatus, jqXHR) {
                    var response = new TransactionResponse();
                    response = data.response;
                    if (response.TransactionType == TransactionType[TransactionType.Create]) {
                        toastr.success(response.ResponseMessage);
                        ResetForm("#addEmailAccountForm");

                    }
                    else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                        toastr.success(response.ResponseMessage);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error("emailaccount save failed");
                }
            });
        }
    });
    $("#showPass").change(function () {
        var checked = $(this).is(":checked");
        if (checked) {
            $("#PassWord").attr("type", "text");
        } else {
            $("#PassWord").attr("type", "PassWord");
        }
    });
    function ResetForm(formSelector) {
        $(':input', formSelector).not('.RetainOnReset').val('');
    }
})