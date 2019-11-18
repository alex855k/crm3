$(document).ready(function () {
    $("#createProcedureForm").submit(function (e) {
        e.preventDefault();
        var formData = new FormData();
        //var procedureInput = $('input[name="procedureImage"]')[0];
        var procedureInput = $("#ProcedureImage");

        if (procedureInput[0] != null) {
            if (procedureInput[0].files.length > 0) {
                var file = procedureInput[0].files[0];
                formData.append("procedureImage", file);
            }
        }

        var procedureId = $("#ProcedureId").val();
        var title = $("#Title").val();
        var created = $("#Created").val();
        var imagePath = $("#OldProcedureImage").val();
        var PDFName = $("#OldPDFName").val();
        
        if (procedureId != null && procedureId != undefined) {
            formData.append("Id", procedureId);
        }

        formData.append("Title", title);
        formData.append("Created", created);
        formData.append("ImagePath", imagePath);
        formData.append("PDFName", PDFName);

        var addedFieldsCount = $(".js-addFieldsCounter").length;

        for (var i = 0; i < addedFieldsCount; i++) {
            var itemTitle = $("#ProcedureItemTitle" + i).val();
            var itemDesc = $("#ProcedureItemDescription" + i).val();
            var itemId = $("#ProcedureItemId" + i).val();
            var itemProcedureId = $("#ProcedureId").val();
            var itemImagePath = $("#OldProcedureItemImage" + i).val();

            if (itemId != null && itemId != undefined) {
                formData.append("ProcedureItems[" + i + "].Id", itemId);
            }

            formData.append("ProcedureItems[" + i + "].Title", itemTitle);
            formData.append("ProcedureItems[" + i + "].Description", itemDesc);
            formData.append("ProcedureItems[" + i + "].ProcedureId", itemProcedureId);
            formData.append("ProcedureItems[" + i + "].ImagePath", itemImagePath);

            var procedureItemInput = $("#ProcedureItemImage" + i);
            if (procedureItemInput[0] != null) {
                if (procedureItemInput[0].files.length > 0) {
                    var file = procedureItemInput[0].files[0];
                    formData.append("procedureItemImages[" + i + "]", file);
                } else {
                    formData.append("procedureItemImages[" + i + "]", "");
                }
            } else {
                formData.append("procedureItemImages[" + i + "]", "");
            }
        }

        if ($("#createProcedureForm").valid()) {
            $.ajax({
                url: "/Procedure/Create",
                data: formData,
                type: "POST",
                processData: false,
                contentType: false,
                success: (data, textStatus, jqXHR) => {
                    let response = new TransactionResponse();
                    response = data.response;
                    if (response.TransactionType == TransactionType[TransactionType.Create]) {
                        toastr.success(response.ResponseMessage);
                        ResetForm("#createProcedureForm", "");
                    } else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                        toastr.success(response.ResponseMessage);
                    }
                    //Refreshes the page, if we don't, we can't create a new Procedure right after another because we don't have the viewmodel
                    setTimeout(function () {
                        location.reload();
                    }, 2000);
                },
                error: (jqXHR, textStatus, errorThrown) => {
                    toastr.error("Procedure save failed"); //TODO: Translation
                }
            })
        }
    });

    function ResetForm(formSelector, fieldSelector) {
        $(':input', formSelector)
            .not(fieldSelector)
            .val('')
    };

    $("#btnAddProcedureItem").click(function (e) {
        e.preventDefault();
        var itemCount = $(".js-addFieldsCounter").length;

        var fieldsContainer = $(".js-dynamicFieldsContainer");
        AddFields(itemCount + 1, fieldsContainer);

        itemCount++;
        $(".js-removeFields").hide();
        $(".js-removeFields").last().show();
    });

    $("#createProcedureForm").on("click", ".js-removeFields", function () {
        var itemCount = $(".js-addFieldsCounter").length;
        $(this).parent().remove();
        itemCount--;
        $(".js-removeFields").last().show();
    });

    function AddFields(itemCount, container) {
        var html = "";
        for (var i = 0; i < itemCount; i++) {
            html = $("<fieldset class='js-addFieldsCounter'>" +
                "<span>Procedure Item" + i + "</span >" +
                "<div class='row'>" +
                    "<section class='col col-4' >" + 
                        "<label for='title" + i + "' class='input'>" +
                            "<input type='text' name='ProcedureItems[" + i + "].Title' id='ProcedureItemTitle" + i + "' class='form-control' />" + 
                        "</label>" +
                    "</section>" +
                "</div>" +
                "<div class='row'>" +
                    "<section class='col col-4' >" +
                        "<label for='" + i + "' class='input'>" +
                            "<textarea name='ProcedureItems[" + i + "].Description' id='ProcedureItemDescription" + i + "' class='form-control' rows='5'></textarea>" +
                        "</label>" +
                    "</section>" +
                "</div>" +
                "<div class='row'>" +
                    "<section class='col col-4' >" +
                        "<label for='" + i + "' class='input'>" +
                            "<input type='file' name='ProcedureItemImage[]' class='form-control' id='ProcedureItemImage" + i + "' />" +
                        "</label>" +
                    "</section>" +
                "</div>" +
                "<a href='#' class='js-removeFields' style='display: none;'>Remove fields</a>" +
            "</fieldset>");
        }
        html.appendTo(container);
    }

    $("#createProcedureForm").on("click", ".js-deleteProcedureItemImage", function (e) {
        e.preventDefault();
        var procedureItemId = $(this).data("procedureitemid");
        var itemNumber = $(this).data("itemnumber");

        $.ajax({
            url: "/Procedure/DeleteProcedureItemImage",
            data: { id: procedureItemId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                $(this).prev().prev().remove();
                $(this).parent().append(
                    "<label for='image' class='input'>" +
                        "<input type='file' name='ProcedureItemImage[]' class='form-control' id='ProcedureItemImage" + itemNumber + "' />" +
                    "</label>");
                $(this).remove();

                $("#OldProcedureItemImage" + itemNumber).val(null);

                toastr.success(response.ResponseMessage);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("Deletion of procedureitem image failed"); //TODO: Translation
            }
        });
    });

    $("#createProcedureForm").on("click", ".js-deleteProcedureImage", function (e) {
        e.preventDefault();
        var procedureId = $(this).data("procedureid");

        $.ajax({
            url: "/Procedure/DeleteProcedureImage",
            data: { id: procedureId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                $(this).prev().prev().remove();
                $(this).parent().append(
                    "<label for='image' class='input'>" +
                        "<input type='file' name='procedureImage' class='form-control' id='ProcedureImage' />" +
                    "</label>");
                $(this).remove();

                $("#OldProcedureImage").val(null);

                toastr.success(response.ResponseMessage);
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("Deletion of procedureitem image failed"); //TODO: Translation
            }
        });
    });
});
