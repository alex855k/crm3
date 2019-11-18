$(document).ready(function () {
    debugger;
    $('#appointmentCustomersMail').tagsinput({
        allowDuplicates: false,
        trimValue: true,
        trimValue: true,
    });
    $('#appointmentSalesPersonMail').tagsinput({
        allowDuplicates: false,
        trimValue: true,
        trimValue: true,
    });

    $('#appointmentCustomersMail').on('beforeItemAdd', function (event) {
        if (!validateEmail(event.item))
            event.cancel = true;
    });
    $('#appointmentSalesPersonMail').on('beforeItemAdd', function (event) {
        if (!validateEmail(event.item))
            event.cancel = true;
    });
    function validateEmail(email) {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    }

    $("#saveAppointmentNote").change(function () {
        debugger;
        if ($(this).is(":checked"))
            $("#noteOptions").show();
        else
            $("#noteOptions").hide();
    })
    function ValidateSaveNote() {
        debugger;
        var valid = false;
        if ($("#saveAppointmentNote").is(":checked")) {
            if ($("#customerNotesReportId").val() == "") {
                valid = false;
                $("#customerNotesReportIdValidation").show();
            } else {
                valid = true;
                $("#customerNotesReportIdValidation").hide();
            }
            if ($("#customerNotesVisitTypeId").val() == "") {
                valid = false;
                $("#customerNotesVisitTypeIdValidation").show();
            } else {
                valid = true;
                $("#customerNotesVisitTypeIdValidation").hide();
            }
            if ($("#customerNotesDemoId").val() == "") {
                valid = false;
                $("#customerNotesDemoIdValidation").show();
            } else {
                valid = true;
                $("#customerNotesDemoIdValidation").hide();
            }
        } else {
            valid = true;
        }
        return valid;
    }
})