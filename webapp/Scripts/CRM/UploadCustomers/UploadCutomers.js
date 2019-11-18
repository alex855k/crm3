$(document).ready(function () {

  

    $("#customersFileUpload1").change(function (e) {
        debugger;
        $('#waitMeContainer').waitMe({
            effect: 'bounce',
            text: 'Please Wait...',
            waitTime: -1,
            textPos: 'vertical',
        });
        $("#fileUploadValidationMessage").text("");
        var file = e.target.files[0];
        var formdata = new FormData(uploadCustomerForm[0]);
        formdata.append("customersUploadFile", file);
        $.ajax({
            url: "/UploadCustomers/UploadCustomers",
            data: formdata,
            type: "POST",
            processData: false,
            contentType: false,
            success: (data, textStatus, jqXHR) => {
                debugger;
                toastr.success(data)
                $("#customersFileUpload1").replaceWith($("#customersFileUpload1").val('').clone(true));
                $('#waitMeContainer').waitMe("hide");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                if (jqXHR.status == 400)
                    toastr.error(errorThrown);
                else
                    toastr.error("customer note save failed");
                $('#waitMeContainer').waitMe("hide");
            }
        })
    });

    $(".downloadFile").click(function (e) {
        debugger;
        var language = $(this).data("language");
        var currentDomain = window.location.hostname;
        if (language == "en")
            window.open("/Content/CustomersExcelTemplate/Customers_en.xlsx", "_blank")
        else if (language == "dk")
            window.open("/Content/CustomersExcelTemplate/Customers_dk.xlsx", "_blank")

    });

});