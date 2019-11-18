$(document).ready(function () {
    debugger
    var isEnabled = $("#hiddenIdEnabled").val();
    if (isEnabled == "False")
        $('#switchActiveUser').prop('checked', false);

    $("#createUsersForm").submit(function (e) {
        e.preventDefault();
        if ($("#createUsersForm").valid()) {
            $.ajax({
                url: "/Users/Create",
                data: $("#createUsersForm").serialize(),
                type: "POST",
                success: (data, textStatus, jqXHR) => {
                    debugger;
                    let response = new TransactionResponse();
                    response = data.response;
                    if (response.TransactionType == TransactionType[TransactionType.Create]) {
                        toastr.success(response.ResponseMessage);
                        ResetForm(); 
                    } else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                        toastr.success(response.ResponseMessage);
                    }
                },
                error: (jqXHR, textStatus, errorThrown) => {

                }
            })
        }
    });
    function ResetForm() {
        $('input', '#createUsersForm')
            .not("#hiddenId")
            .val('')
    }

    $("#switchActiveUser").change(function (e) {
        if ($(this).is(":checked"))
            $("#hiddenIdEnabled").val('true')
        else
            $("#hiddenIdEnabled").val('false')

    });
});