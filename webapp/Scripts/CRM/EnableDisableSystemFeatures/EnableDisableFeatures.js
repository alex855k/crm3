$(document).ready(function () {
    $(".enableDisableFeature").change(function () {
        var _this = this;
        debugger;
        var thisRadio = $(this)[0];
        var id = thisRadio.id;
        var isDisabled = thisRadio.checked;
        $.ajax({
            url: "/EnableDisableSystemFeatures/EnableDisableFeature",
            data: { "id": id, "isDisabled": isDisabled },
            type: "POST",
            success: function (data, textStatus, jqXHR) {
                debugger;
                toastr.success(data);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("save failed");
                $(_this).prop("checked", !isDisabled);
            }
        });
    });
});
//# sourceMappingURL=EnableDisableFeatures.js.map