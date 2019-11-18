
$(document).ready(function () {
    var colorPicker = "";
    ColorPickerSetting();
    var navigateToNotesAfterSave = false;
    $(".btnSaveCustomer").click(function () {
        navigateToNotesAfterSave = $(this).data("navigatenotes");
        $("#createCustomerForm").trigger("submit");
    });
    $("#createCustomerForm").submit(function (e) {
        e.preventDefault();
        if ($("#createCustomerForm").valid()) {
            $.ajax({
                url: "/Customers/Create",
                data: $("#createCustomerForm").serialize(),
                type: "POST",
                success: (data, textStatus, jqXHR) => {
                    let response = new TransactionResponse();
                    response = data.response;
                    if (response.TransactionType == TransactionType[TransactionType.Create]) {
                        if (!navigateToNotesAfterSave) {
                            toastr.success(response.ResponseMessage);
                            ResetForm("#createCustomerForm", "#hiddenPageNumber,#customerStatus");
                        } else {
                            toastr.success("<span>Page will redirect to customer notes<span>"
                                , response.ResponseMessage, { timeOut: 2000 });
                            window.setTimeout(function () {
                                return window.location.href = "/Customers/Edit/?id=" + data.savedCustomerId + "&latestCustomersPageNumber=0&navigateToNotes=true";
                            }, 2000);
                        }
                    } else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                        toastr.success(response.ResponseMessage);
                    }

                },
                error: (jqXHR, textStatus, errorThrown) => {
                    toastr.error("customer save failed");
                }
            });
        }
    });


    function ResetForm(formSelector, fieldSelector) {
        $(':input', formSelector)
            .not(fieldSelector)
            .val('');


    }


    $("#customerOrdersLi").click(function () {
        $.ajax({
            url: "/Customers/CustomerOrders",
            data: { 'customerId': $("#customerId").val() },
            type: "GET",
            success: function (data) {
                $("#customerOrders").html(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("Failed to load customers orders");
            }
        });
    });

    //$("#btnAddlicense").on("click", function () {
    //    debugger;
    //    $.ajax({
    //        url: "/Customers/AddLicense",
    //        data: { 'customerOrdersViewModel': '@Model' },
    //        type: "GET",
    //        success: function (data) {
    //            $("#customerOders").html(data);
    //        },
    //        error: (jqXHR, textStatus, errorThrown) => {
    //            toastr.error("Failed to load addlicense");
    //        }
    //    });
    //});


    $("#customerContactsLi").click(() => {
        $.ajax({
            url: "/Customers/CustomerContacts",
            data: { 'customerId': $("#customerId").val() },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#customerContacts").html(data);
                $.validator.unobtrusive.parse("#customerContactsForm");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed load customer contacts");
            }
        });
    });

    $(document).on("click", "#customerCasesLi", () => {

        $.ajax({
            url: "/CustomerCases/CustomerCaseIndex",
            data: { 'customerId': $("#hiddenCurrentCustomerId").val() },
            type: "GET",
            success: (data) => {
                $("#customerCase").html(data);
                $.validator.unobtrusive.parse("#customerCases");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed load customer contacts");
            }
        });
    });

    $("#customerNotesLi").click(function () {
        $.ajax({
            url: "/Customers/CustomerNotes",
            data: { 'customerId': $("#customerId").val() },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#customerNotes").html(data);
                $.validator.unobtrusive.parse("#customerNotes");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed load customer contacts");
            }
        });
    });

    $("#customerAppointmentsLi").click(function () {
        $.ajax({
            url: "/Customers/CustomerAppointments",
            data: { 'customerId': $("#customerId").val() },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#customerAppointments").html(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed load customer Appointments");
            }
        });
    });
    $("#OrdersLi").click(function () {
        $.ajax({
            url: "/Orders/CustomerOrderHistory",
            data: { 'customerId': $("#customerId").val() },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#Orders").html(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed to load order history");
            }
        });
    });
    $("#customerEmailCorrespondenceLi").click(function () {
        $.ajax({
            url: "/EmailMessages/CustomerEmailCorrespondenceIndex",
            data: { 'customerId': $("#customerId").val() },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#customerEmailCorrespondence").html(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed to load customer correspondence");
            }
        });
    });

    $(document).on("click", "#btnSaveCustomerContacts", function (e) {
        if ($("#customerContactsForm").valid()) {
            $.ajax({
                url: "/Customers/CustomerContacts",
                data: $("#customerContactsForm").serialize(),
                type: "POST",
                success: (data, textStatus, jqXHR) => {
                    toastr.success("customer contact saved");
                    $("#customerContactsListDiv").html(data);
                    ResetForm("#customerContactsForm", "#HiddenCustomerId");
                },
                error: (jqXHR, textStatus, errorThrown) => {
                    toastr.error("customer contact save failed");
                }
            });
        } else
            return false;
    });

    var files = [];
    var removedFiles = [];
    $(document).on("change", "#FileUpload1", function (e: any) {
        var newFiles = [];
        for (var i = 0; i < e.target.files.length; i++) {
            files.push(e.target.files[i].name);
            newFiles.push(e.target.files[i].name);
        }
        $("#tableAttachments").show();
        var lastRowId: number;
        if ($("#tableAttachments tbody tr").length == 0) {
            lastRowId = 1;
        } else {
            var trId = $('#tableAttachments tbody tr:last').attr('id');
            lastRowId = parseInt(trId) + 1;
        }
        $.each(newFiles, function (key, value) {
            var $row = $("<tr id='" + lastRowId + "'></tr>");
            var $col1 = ("<td>" + value + "</td>");
            var $col2 = ("<td><a data-name='" + value + "' class='removeAttachment' href='#'><i class='fa fa-trash-o' aria-hidden='true'></i></a></td>");
            $row.append($col1, $col2).appendTo("#tableAttachments tbody");
            lastRowId++;
        });
        newFiles = [];
    });

    $(document).on("click", "#btnSaveCustomerNotes", function (e) {
        var fileupload = <HTMLInputElement>document.getElementById("FileUpload1");
        var uploadedFiles = fileupload.files;
        $("#fileUploadValidationMessage").text('');
        if (removedFiles.length > 0)
            $("#HiddenRemovedAttachments").val(JSON.stringify(removedFiles));
        var myform = $("#customerNotesForm");
        var formdata = new FormData(myform[0] as HTMLFormElement);
        for (var i = 0; i < files.length; i++) {
            for (var j = 0; j < uploadedFiles.length; j++) {
                if (uploadedFiles[j].name == files[i]) {
                    formdata.append("customerNoteAttachment", uploadedFiles[j]);
                }
            }
        }
        if ($("#customerNotesForm").valid()) {
            $.ajax({
                url: "/Customers/CustomerNotes",
                data: formdata,
                type: "POST",
                processData: false,
                contentType: false,
                success: (data, textStatus, jqXHR) => {
                    toastr.success('customer notes saved');
                    $("#divCustomerNotesList").html(data);
                    ResetForm("#customerNotesForm", "#HiddenCustomerId");
                    ClearTableAttachments();
                },
                error: (jqXHR, textStatus, errorThrown) => {
                    if (jqXHR.status == 400)
                        $("#fileUploadValidationMessage").text(errorThrown);
                    else
                        toastr.error("customer note save failed");
                }
            });
        } else
            return false;
    });


    $(document).on("click", ".downloadAttachment", function (e) {
        e.preventDefault();
        var attachmentName = $(this).attr("data-attachment");
        var customerId = $("#HiddenCustomerId").val();
        var noteId = $("#HiddenCustomerNoteId").val();
        $.ajax({
            url: "/Customers/DownloadCustomerNoteAttachment",
            data: { "customerId": customerId, "attachmentName": attachmentName, "noteId": noteId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                window.open(data, "_blank");
            },
            error: (jqXHR, textStatus, errorThrown) => {

                toastr.error("download failed");
            }
        });
    });


    $(document).on("click", ".editCustomerContact", function (e) {
        var customerContactId = $(this).attr("data-id");
        $.ajax({
            url: "/Customers/EditCustomerContact",
            data: { "customerContactId": customerContactId },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#customerContactsFormDiv").html(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("update failed");
            }
        });
    });

    $(document).on("click", ".hiddenDeleteCustomerContact", function (e) {
        var contactId = $(this).attr("data-id");
        var customerId = $("#HiddenCustomerId").val();
        $.ajax({
            url: "/Customers/DeleteCustomerContact",
            data: { "customerId": customerId, "customerContactId": contactId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                $("#customerContactsListDiv").html(data);
                toastr.success('customer contact deleted');
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("delete failed");
            }
        });
    });
    function DeleteContact(buttonPressed, contactId) {
        var customerId = $("#HiddenCustomerId").val();
        $.ajax({
            url: "/Customers/DeleteCustomerContact",
            data: { "customerId": customerId, "customerContactId": contactId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                $("#customerContactsListDiv").html(data);
                toastr.success('customer contact deleted');
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("delete failed");
            }
        });
    }
    $(document).on("click", ".editCustomerNote", function (e) {
        var customerNoteId = $(this).attr("data-id");
        $.ajax({
            url: "/Customers/EditCustomerNote",
            data: { "customerNoteId": customerNoteId },
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                $("#customerNotesFormDiv").html(data);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("update failed");
            }
        });
    });

    $(document).on("click", ".removeAttachment", function (e) {
        e.preventDefault();
        var attachmentName = $(this).attr("data-name");
        for (var i = 0; i < files.length; i++) {
            if (files[i] == attachmentName) {
                files.splice(i, 1);
            }
        }
        var tr = $(this).closest("tr").attr("id");
        $("#" + tr).remove();
        if ($("#tableAttachments tbody tr").length == 0)
            $("#tableAttachments").hide();
        removedFiles.push(attachmentName);
    });

    function ClearTableAttachments() {
        $.each($("#tableAttachments tbody tr"), function (index, value) {
            $(this).remove();
        });
        $("#tableAttachments").hide();
    }

    function ColorPickerSetting() {
        if ($("#hiddenCustomerRowColor").val() == "" || $("#hiddenCustomerRowColor").val() == null) {
            colorPicker = '#ffffff';

        } else {
            colorPicker = $("#hiddenCustomerRowColor").val();
            setColorBar(colorPicker);
        }
    }

    $('#customerColorPicker').colorpicker({
        color: colorPicker,
        format: 'hex'
    });
    $(".customerColor").change(function (e: any) {
        var colorValue = e.target.value;
        $('#customerColorPicker').colorpicker('setValue', colorValue);
    });

    function setColorBar(color) {
        var radiobutton = $("input[name=customerColor][value=" + color.toUpperCase() + "]")[0];
        if (radiobutton != undefined) {
            $(".lblCustomerColor").removeClass("active");
            $(".customerColor").removeAttr("checked");
            $("#" + radiobutton.id + "").closest(".lblCustomerColor").addClass("active");

        } else
            $(".lblCustomerColor").removeClass("active");
    }

    $("#customerPhoneBtn").click(function () {
        window.open('tel:' + $("#customerPhone").val() + '');
    });

    $("#customerUrlBtn").click(function () {
        window.open($("#customerUrl").val(), "_blank");
    });

    $("#customerCvrBtn").click(function () {
        window.open("https://datacvr.virk.dk/data/visenhed?enhedstype=virksomhed&id=" + $("#customerCvr").val() + "", "_blank");
    });

    $("#customerAddressBtn").click(function () {
        window.open("https://www.google.dk/maps/place/" + $("#customerAddress").val() + "", "_blank");
    });

    $("#customerPostalBtn").click(function () {
        window.open("https://www.google.com.eg/search?q=postal+code+" + $("#customerPostalCode").val() + "", "_blank");
    });

    $("#customerEmailButton").click(function () {
        window.open('mailto:' + $("#customerEmail").val() + '');
    });

    $("#addCustomerToDashboardList").click(function (e) {
        var customerId = $("#customerId").val();
        $.ajax({
            url: "/Customers/GetDashboardListsByUser",
            data: { "customerId": customerId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                if (data.length == 0)
                    $("#btnSaveCustomerToList").attr("disabled", "disabled");
                else {
                    $("#divUserLists").empty();
                    $.each(data, function (index, elem) {
                        $("#divUserLists").append(`
                        <label class='checkbox'>
                        <input id=`+ elem.Id + ` type='checkbox' name='checkbox' class="checkUserDashboardList" >
                        <i></i> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp `+ elem.Name + `
                        </label>`);
                    });


                }
                $("#myModal").modal("show");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("customer save failed");
                $("#hiddenSelectedCustomerId").val("");
            }
        });
    });

    $('[data-toggle="tooltip"]').tooltip();


});
