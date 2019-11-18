$(document).ready(function () {
    $.validator.setDefaults({
        onkeyup: false,
        onfocusout: true,
        onsubmit: true
    });
    $('#dispatchdate').datepicker({
        format: 'dd/mm/yyyy',
        todayHighlight: true,
        autoclose: true
    });
    //$('#DispatchDate').datepicker();
    $(".product").last().find(".btn-sm.editable-submit").click(function (e) {
        addProductRow();
        return false;
    });
    $(".product").last().keypress(function (e) {
        if (e.which == 13) {
            addProductRow();
            return false;
        }
    });
    $(".product").find(".btn-sm.editable-cancel").click(function (e) {
        var $toRemove = $(e.currentTarget.closest(".product"));
        $toRemove.slideUp("fast", function () { $toRemove.remove(); });
        return false;
    });
    $("#createOrderForm").submit(function (e) {
        e.preventDefault();
        if ($("#createOrderForm").valid()) {
            var order = {};
            order.Id = $("#OrderId").val();
            order.Street = $("#Street").val();
            order.HouseNr = $("#HouseNr").val();
            order.DispatchDate = $("#DispatchDate").val();
            order.CustomerId = $("#CustomerSelect").find(":selected").attr("customer-id");
            order.StatusId = $("#StatusId").val();
            order.PostalCode = $("#PostalCode").val();
            order.Town = $("#Town").val();
            order.OrderedProducts = getOrderProducts();
            $.ajax({
                url: "/Orders/Create",
                data: { "viewModel": order },
                type: "POST",
                success: function success(data, textStatus, jqXHR) {
                    var response = new TransactionResponse();
                    response = data.response;
                    if (data.success == true) {
                        if (response.TransactionType == TransactionType[TransactionType.Create]) {
                            toastr.success(response.ResponseMessage);
                            ResetForm("#createOrderForm", "");
                        } else if (response.TransactionType == TransactionType[TransactionType.Update]) {
                            toastr.success(response.ResponseMessage);
                        }
                    } else {
                        toastr.warning(response.ResponseMessage);
                    }
                },
                error: function error(jqXHR, textStatus, errorThrown) {
                    toastr.error("order save failed");
                }
            });
        }
    });
    function ResetForm(formSelector, fieldSelector) {
        $(':input', formSelector).not(fieldSelector).val('');
        $('.product:not(:last)').remove();
    }
    function addProductRow() {
        var newProd = $(".product").last();
        var prod = $(newProd).find("[name='product']");
        var quan = $(newProd).find("[name='quantity']");
        if (prod.val() && quan.val() > 0) {
            quan.addClass("valid");
            var $clone = $(newProd).clone(true);
            $clone.find(".editable-cancel").css("display", "inline-block");
            $clone.find(".editable-submit").css("display", "none");
            $clone.css("display", "none");
            $($clone).insertBefore(newProd).slideDown("fast");
            quan.val(1);
            prod.val('');
            newProd.find(".input.state-success").removeClass("state-success");
            newProd.find(".valid").removeClass("valid");
            prod.focus();
        }
    }
    function getOrderProducts() {
        var products = [];
        var product = {};
        $(".orderProducts").each(function (index, elem) {
            product.Product = $(elem).find(".productInput").val();
            product.Quantity = $(elem).find(".quantityInput").val();
            product.OrderId = $("#OrderId").val();
            products.push(product);
            product = {};
        });
        products.pop();
        return products;
    }
});