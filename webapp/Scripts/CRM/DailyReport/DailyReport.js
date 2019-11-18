$(document).on("click", ".editDailyReport", function () {
    debugger;
    var id = $(this).data("id");
    var date = $(this).data("date");
    var kmFrom = $(this).data("kmfrom");
    var kmTo = $(this).data("kmto");
    $("#hiddenDailyReportId").val(id);
    $("#dailyReportDate").text(date);
    $("#kmFrom").val(kmFrom);
    $("#kmTo").val(kmTo);
    $("#dailyReportModal").modal("show");
});

$("#saveDailyReport").click(function () {
    debugger;
    var id = $("#hiddenDailyReportId").val();
    var date = $("#dailyReportDate").text();
    var kmFrom = $("#kmFrom").val();
    var kmTo = $("#kmTo").val();
    var dailyReport = { "Id": id, "Date": date, "KmFrom": kmFrom, "KmTo": kmTo }
    $.ajax({
        url: "/DailyReport/UpdateDailyReport",
        data: { "dailyReportViewModel": dailyReport },
        type: "POST",
        success: function (data, textStatus, jqXHR) {
            $("#dailyReportListDiv").html(data.tablePartial);
            $("#dailyReportPaginationDiv").html(data.pagingPartial);
            $("#paginationFromNumber").text(data.RowsFrom);
            $("#paginationToNumber").text(data.RowsTo);
            $("#paginationTotalCount").text(data.totalCount);
            $("#dailyReportModal").modal("hide");
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });



});



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

$("#commonFilter").keyup(function (e) {
    clearTimeout($.data(this, 'timer'));
    $(this).data('timer', setTimeout(Search(true, null), 500));
});

$("#usersDropdown").change(function () {
    Search(true, null);
});

function Search(forceSearch, pageNumber) {
    debugger;
    if (!forceSearch)
        return;
    var searchObject = {};
    var defaultSort = $("#hiddenDefaultOrderBy").val();
    var currentPageNumber = "";
    if (pageNumber != "" && pageNumber != undefined && pageNumber != null)
        currentPageNumber = pageNumber;
    else
        currentPageNumber = $(".pagination").children().children().attr("data-pagenumber");

    searchObject.SearchKey = $("#commonFilter").val();
    searchObject.PageNumber = parseInt(currentPageNumber);
    searchObject.DefaultOrderBy = defaultSort;
    searchObject.OrderBy = $("#hiddenOrderBy").val();
    searchObject.Direction = $("#hiddenDirection").val();
    searchObject.SelectedUserId = $("#usersDropdown").val();
    $.ajax({
        url: "/DailyReport/SearchDailyReport",
        data: searchObject,
        type: "POST",
        success: function (data, textStatus, jqXHR) {
            debugger;
            $("#dailyReportListDiv").html(data.tablePartial);
            $("#dailyReportPaginationDiv").html(data.pagingPartial);
            $("#paginationFromNumber").text(data.RowsFrom);
            $("#paginationToNumber").text(data.RowsTo);
            $("#paginationTotalCount").text(data.totalCount);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            toastr.error("failed");
        }
    });
}