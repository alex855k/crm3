$(document).ready(function () {
    $(".enableDisableFeature").change(function () {
        debugger;
        var thisRadio = $(this)[0] as HTMLInputElement
        var id = thisRadio.id;
        var isDisabled = thisRadio.checked;
        $.ajax({
            url: "/EnableDisableSystemFeatures/EnableDisableFeature",
            data: { "id": id, "isDisabled": isDisabled},
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                debugger;
                toastr.success(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("save failed");
                $(this).prop("checked", !isDisabled)
            }
        })
    })
})