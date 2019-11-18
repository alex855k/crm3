$(document).ready(function () {
    $(document).on("click", ".LiPager", function (e) {
        debugger;
        e.preventDefault();
        var pageNumber = $(this).children().attr("data-pagenumber");
        var currentPageNumber = $(this).siblings(".active").children().attr("data-pagenumber");
        if (pageNumber == "Next") {
            pageNumber = String(parseInt(currentPageNumber) + 1);
        }
        else if (pageNumber == "Previous") {
            pageNumber = String(parseInt(currentPageNumber) - 1);
        }
        Search(true, parseInt(pageNumber));
    });
    $(".sortTable").click(function (e) {
        debugger;
        var newSort = "";
        var currentClass = $(this).attr("class");
        var orderBy = $(this).attr("data-sortKey");
        var currentSort = currentClass.substr(currentClass.indexOf(' ')).trim();
        if (currentSort == SortDirection.Asc) {
            newSort = SortDirection.Desc;
            $("#hiddenOrderBy").val(orderBy);
            $("#hiddenDirection").val("Desc");
        }
        else {
            newSort = SortDirection.Asc;
            $("#hiddenOrderBy").val(orderBy);
            $("#hiddenDirection").val("Asc");
        }
        $(".sortTable").removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting");
        $(".sortTable").not(this).addClass("sorting");
        $(this).addClass(newSort);
        Search(true, null);
    });
    $(document).on("click", ".operatorDropdown", function (e) {
        debugger;
        var operator = $(this).attr("data-operator");
        var inputId = $(this).attr("data-forInput");
        $("#" + inputId).attr('data-selectedOperator', operator);
        var parentUL = $(this).parent().parent();
        parentUL.children().removeClass("active");
        $(this).parent().addClass("active");
        Search(true, null);
    });
    $(document).on("keyup", ".query", function (e) {
        clearTimeout($.data(this, 'timer'));
        $(this).data('timer', setTimeout(Search(true, null), 500));
    });
    $("#commonFilter").keyup(function (e) {
        clearTimeout($.data(this, 'timer'));
        $(this).data('timer', setTimeout(Search(true, null), 500));
    });
    $("#switchTableComparer").change(function (e) {
        debugger;
        if ($(this).is(":checked"))
            $("#hiddenTableComparer").val('And');
        else
            $("#hiddenTableComparer").val('Or');
        Search(true, null);
    });
    function Search(forceSearch, pageNumber) {
        debugger;
        if (!forceSearch)
            return;
        var searchObject = {};
        var SearchParams = [];
        var searchParamsObj = {};
        var defaultSort = $(".defaultSort").attr("data-searchKey");
        var currentPageNumber = "";
        var isAllSearchParametersEmpty = true;
        var isCommonSearchParameterEmpty = true;
        if (pageNumber != "" && pageNumber != undefined && pageNumber != null)
            currentPageNumber = pageNumber;
        else
            currentPageNumber = $(".pagination").children().children().attr("data-pagenumber");
        $(".query").each(function (index, elem) {
            if ($(elem).val() != "") {
                searchParamsObj.SearchKey = $(elem).attr("data-searchKey");
                searchParamsObj.Operator = $(elem).attr("data-selectedOperator");
                searchParamsObj.Value = $(elem).val();
                SearchParams.push(searchParamsObj);
                searchParamsObj = {};
                isAllSearchParametersEmpty = false;
            }
            if ($("#commonFilter").val() != "") {
                searchParamsObj.SearchKey = $(elem).attr("data-searchKey");
                searchParamsObj.Operator = "Contains";
                searchParamsObj.Value = $("#commonFilter").val();
                SearchParams.push(searchParamsObj);
                searchParamsObj = {};
                isCommonSearchParameterEmpty = false;
            }
        });
        if ($("#commonFilter").val() != "") {
        }
        searchObject.QueryParameters = SearchParams;
        searchObject.PageNumber = parseInt(currentPageNumber);
        searchObject.DefaultOrderBy = defaultSort;
        if (isAllSearchParametersEmpty && !isCommonSearchParameterEmpty)
            searchObject.QueryOperatorComparer = "Or";
        else
            searchObject.QueryOperatorComparer = $("#hiddenTableComparer").val();
        searchObject.OrderBy = $("#hiddenOrderBy").val();
        searchObject.Direction = $("#hiddenDirection").val();
        searchObject.IsCustomers = $("#hiddenIsCustomers").val();
        var filterRow = $("#filterTr").clone();
        $.ajax({
            url: "/Customers/DynamicTable",
            data: { "customerViewModel": searchObject },
            type: "POST",
            success: function (data, textStatus, jqXHR) {
                debugger;
                $("#customersListDiv").html(data.tablePartial);
                $("#customersPaginationDiv").html(data.pagingPartial);
                $("#paginationFromNumber").text(data.RowsFrom);
                $("#paginationToNumber").text(data.RowsTo);
                $("#paginationTotalCount").text(data.totalCount);
                $("#HiddenQueryParameters").val(data.queryParameters);
                initQtip();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("failed load customer contacts");
            }
        });
    }
    initQtip();
    function initQtip() {
        $('.noteToolTip').qtip({
            content: {
                attr: 'notetip'
            },
        });
    }
    $(document).on("click", ".addCustomerToList", function (e) {
        debugger;
        var customerId = $(this).attr("data-customerId");
        $.ajax({
            url: "/Customers/GetDashboardListsByUser",
            data: { "customerId": customerId },
            type: "POST",
            success: function (data, textStatus, jqXHR) {
                debugger;
                if (data.length == 0)
                    $("#btnSaveCustomerToList").attr("disabled", "disabled");
                else {
                    ListUserListsCheckboxes(data);
                    $("#hiddenSelectedCustomerId").val(customerId);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("customer save failed");
                $("#hiddenSelectedCustomerId").val("");
            }
        });
    });
    $("#refreshTable").click(function (e) {
        debugger;
        e.preventDefault();
        var url_string = window.location.href;
        var url = new URL(url_string);
        var parameter = url.searchParams.get("isCustomers");
        $.ajax({
            url: "/customers/RefreshTable",
            data: { "isCustomers": parameter },
            success: function (data) {
                debugger;

                location.reload();
            },
            error: function () {

            }
        });
    });

    function ListUserListsCheckboxes(data) {
        $("#divUserLists").empty();
        $.each(data, function (index, elem) {
            $("#divUserLists").append(`<label class='checkbox'>
                    <input id=${elem.Id} type='checkbox' name='checkbox' class="checkUserDashboardList" >
                    <i></i> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp ${elem.Name}</label>`);
        });
    }

});

