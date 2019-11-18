$(document).ready(function () {
    $("#editCompanyProfile").click(function () {
        $(".toReplace").toggle();
    });
    $("#cancel").click(function () {
        $(".toReplace").toggle();
    });

    $("#companyLogoImg").click(function () {
        $("#logoFileUpload").click();
    });

    $("#logoFileUpload").change(function () {
        debugger
        var formData = new FormData();
        var fileUpload = document.getElementById("logoFileUpload").files[0];
        formData.append("companyLogo", fileUpload);
        var fileUploadControl = $(this)[0];
        $.ajax({
            url: '/CompanyProfile/UploadCompanyImage',
            type: "POST",
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (data) {
                debugger
                toastr.success("company logo uploaded")
                readURL(fileUploadControl);
                $("#logoFileUpload").replaceWith($("#logoFileUpload").val('').clone(true));
            },
            error: function (error) {
                toastr.error("upload logo faild")
            }
        })
    });
    function readURL(input) {

        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $('#companyLogoImg').attr('src', e.target.result);
                $('#headerLogo').attr('src', e.target.result);
            }

            reader.readAsDataURL(input.files[0]);
        }
    }
    $("#saveCompanyProfile").click(function () {
        var companyProfile = {};
        companyProfile.Address = $("#companyAddress").val();
        companyProfile.Email = $("#companyMail").val();
        companyProfile.Name = $("#companyName").val();
        companyProfile.Phone = $("#companyPhone").val();
        companyProfile.URL = $("#companyURL").val();
        $.ajax({
            url: '/CompanyProfile/UpdateCompanyProfile',
            type: "POST",
            data: companyProfile,
            success: function (data) {
                debugger
                $("#companyAddressText").text(companyProfile.Address);
                $("#companyMailText").text(companyProfile.Email);
                $("#companyNameText").text(companyProfile.Name);
                $("#companyPhoneText").text(companyProfile.Phone);
                $("#companyURLText").text(companyProfile.URL);
                $(".toReplace").toggle();
                toastr.success("company profile saved")
            },
            error: function (error) {
                toastr.error("save faild")
            }
        })

    });
})
