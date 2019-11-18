$(document).ready(function () {
    $(document).on("click", ".LiPager", function (e) {
        debugger;
        e.preventDefault();
        var ahref = $(this).children()[0];
        var pageNumber = $(ahref).attr("data-pagenumber");
        var currentPageNumber = "";
        if (pageNumber == "Next") {
            currentPageNumber = $(".pagination").children().children().attr("data-pagenumber");
            pageNumber = String(parseInt(currentPageNumber) + 1);
        }
        else if (pageNumber == "Previous") {
            currentPageNumber = $(".pagination").children().children().attr("data-pagenumber");
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
        $(this).data('timer', setTimeout(function () {
            Search(true, null);
        }, 500));
    });
    $("#commonFilter").keyup(function (e) {
        clearTimeout($.data(this, 'timer'));
        $(this).data('timer', setTimeout(function () {
            Search(true, null);
        }, 500));
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
            }
            if ($("#commonFilter").val() != "") {
                searchParamsObj.SearchKey = $(elem).attr("data-searchKey");
                searchParamsObj.Operator = "Contains";
                searchParamsObj.Value = $("#commonFilter").val();
                SearchParams.push(searchParamsObj);
                searchParamsObj = {};
            }
        });
        if ($("#commonFilter").val() != "") {
        }
        searchObject.QueryParameters = SearchParams;
        searchObject.PageNumber = parseInt(currentPageNumber);
        searchObject.DefaultOrderBy = defaultSort;
        searchObject.QueryOperatorComparer = $("#hiddenTableComparer").val();
        searchObject.OrderBy = $("#hiddenOrderBy").val();
        searchObject.Direction = $("#hiddenDirection").val();
        var filterRow = $("#filterTr").clone();
        var url = $("#hiddenUrl").val();
        $.ajax({
            url: "/" + url + "/DynamicTable",
            data: searchObject,
            type: "POST",
            success: function (data, textStatus, jqXHR) {
                debugger;
                $("#listDiv").html(data.tablePartial);
                $("#paginationDiv").html(data.pagingPartial);
                $("#paginationFromNumber").text(data.RowsFrom);
                $("#paginationToNumber").text(data.RowsTo);
                $("#paginationTotalCount").text(data.totalCount);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("failed load customer contacts");
            }
        });
    }
});
var SearchParameters = (function () {
    function SearchParameters() {
    }
    return SearchParameters;
}());
var SearchObject = (function () {
    function SearchObject() {
    }
    return SearchObject;
}());
//# sourceMappingURL=GenericTable.js.map