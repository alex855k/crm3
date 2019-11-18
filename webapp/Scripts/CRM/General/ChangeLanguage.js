$(document).ready(function () {
    $(".aLanguage").click(function () {
        debugger;
        var culture = $(this).data("culture");
        var uiCulture = $(this).data("uiculture");
        $.ajax({
            url: "/Language/ChangeCulture",
            data: { 'culture': culture, 'uiCulture': uiCulture },
            type: "GET",
            success: function (result, status, xhr) {
                location.reload();
            },
            error: function (xhr, status, error) {
                toastr.error("change language failed");
            }
        });
    });
});
//# sourceMappingURL=ChangeLanguage.js.map