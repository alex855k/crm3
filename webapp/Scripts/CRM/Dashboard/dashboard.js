$("#document").ready(function () {

    function initializeWidgets() {
        $(".listColumns").each(function (index, elemet) {
            var id = $(this)[0].id;
            if (id != 0 || id != undefined || id != null || id != "")
                $("#" + id).nestable({ group: 1, maxdepth: 1, onDragStart: onDragStart, callback: callback});
        });
    }

    function onDragStart(l, e) {
        var customerDashboardListId = $(e).attr("data-customerDashboardListId");
        $("#hiddenCurrentCustomerDashboardListId").val(customerDashboardListId);
    }
    function callback(l, e) {
        debugger;
         // l is the main container
        // e is the element that was moved
        var columnId = $(l).attr("data-columnid");
        var dashboardlistId = $(e).attr("data-dashboardlistid");
        var customerDashboardListId = $("#hiddenCurrentCustomerDashboardListId").val();
        var customerId = $(e).attr("data-customerId");
        $.ajax({
            url: "/Home/AssignCustomerToList",
            data: {
                "dashboardlistId": dashboardlistId, "columnId": columnId,
                "customerId": customerId, "customerDashboardListId": customerDashboardListId
            },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                $("#widgetsDiv").html(data);
                initializeWidgets();
                $("#hiddenCurrentCustomerDashboardListId").val("");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error(errorThrown);
            }
        });
    }


    initializeWidgets();
    $(".dashboardListsMenu").click(function () {
        debugger;
        var dashboardListId = $(this)[0].id
        $.ajax({
            url: "/Home/AddWidget",
            data: { "dashboardListId": dashboardListId },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                $("#widgetsDiv").html(data);
                initializeWidgets();
            },
            error: (jqXHR, textStatus, errorThrown) => {
                debugger;
                toastr.error(errorThrown)
            }
        })
    })

    $(".btnDeleteWidget").click(function (e) {
        var dashboardlistId = $(this).attr("data-dashboardlistId");
        debugger;
        $.SmartMessageBox({
            title: $(this).data("title"),
            content: $(this).data("content"),
            buttons: '[' + $(this).data("no") + '][' + $(this).data("yes") + ']'
        }, function (ButtonPressed) {
            if (ButtonPressed == "Yes")
                $.ajax({
                    url: "/Home/DeleteWidget",
                    data: { "dashboardlistId": dashboardlistId },
                    type: "POST",
                    success: (data, textStatus, jqXHR) => {
                        $("#widgetsDiv").html(data);
                        initializeWidgets();
                    },
                    error: (jqXHR, textStatus, errorThrown) => {
                        toastr.error(errorThrown);
                    }
                })
        });
        e.preventDefault();
    });


    $(".removeCustomerFromList").click(function () {
        debugger;
        var customerDashboardList = $(this).attr("data-customerDashboardList");
        $.ajax({
            url: "/Home/RemoveCustomerFromList",
            data: { "customerDashboardList": customerDashboardList },
            type: "POST",
            success: (data, textStatus, jqXHR) => {
                toastr.success("Customer removed from list successfully");
                $("#widgetsDiv").html(data);
                initializeWidgets();
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error(errorThrown);
            }
        })
    });

});