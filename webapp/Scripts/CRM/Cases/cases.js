var doneSlider;
var $caseModal;

//=================INIT==================//
//#region INIT

$(document).ready(function () {

    $caseModal = $("#caseModal");
    doneSlider = document.getElementById('g1-div');
    noUiSlider.create(doneSlider,
        {
            start: [0],
            connect: [true, false],
            tooltips: [true],
            step: 1,
            range: {
                "min": [0],
                "max": [100]
            }
        });
    $("#timeRegStart").datetimepicker({
        locale: "da",
        sideBySide: true
    });
    $(".week-picker").on("change", function (event) {
        console.log(event.detail); // { mode: "deselect", range: ["2017-06-12"] }
    });
    $("._week-picker").prepend("<label for='category'>Week Picker</label>")
    $("._week-picker :input").addClass("form-control")
    $("#assignmentStart").datetimepicker({
        locale: "da"
    });
    $("#assignmentDeadlineDate").datetimepicker({
        locale: "da"
    });

    $("#assignmentDeadlineDate").on("dp.change", function (e) {
            $("#assignmentStart").data("DateTimePicker").maxDate(e.date);
        });

    $("#assignmentStart").on("dp.change", function (e) {
            $("#assignmentDeadlineDate").data("DateTimePicker").minDate(e.date);
        });

    $("#timeRegEnd").datetimepicker({
        locale: "da",
        sideBySide: true
    });

    $("#timeRegEnd").on("dp.change", function (e) {
            $("#timeRegStart").data("DateTimePicker").maxDate(e.date);
        });

    $("#timeRegStart").on("dp.change", function (e) {
            $("#timeRegEnd").data("DateTimePicker").minDate(e.date);
        });

    $("#caseStartDate").datetimepicker({
        date: new Date(),
        locale: "da"
    });

    $("#caseDeadlineDate").datetimepicker({
        locale: "da"
    });

    $("#caseDeadlineDate").on("dp.change", function (e) {
            $("#caseStartDate").data("DateTimePicker").maxDate(e.date);
        });

    $("#caseStartDate").on("dp.change", function (e) {
            $("#caseDeadlineDate").data("DateTimePicker").minDate(e.date);
        });

    $(`#estimatedTimeHour-spinner`).spinner({
        step: 1.0,
        numberFormat: "n",
        min: 0
    });
    $(`#estimatedTimeMinute-spinner`).spinner({
        step: 5.0,
        numberFormat: "n",
        min: 0,
        max: 59
    });
    $(`#estimatedTimeHour-spinner`).parent().css("width", "58px");
    $(`#estimatedTimeMinute-spinner`).parent().css("width", "58px");
    //#endregion
    //================INIT END===============//
});
$("#createBtn").click(function () {
    clearCaseModal();
    $caseModal.find("#caseDeadlineDate").data("DateTimePicker").minDate(new Date());
    $caseModal.find("#caseStartDate").data("DateTimePicker").date(new Date());
    $caseModal.find("#caseStartDate").data("DateTimePicker").maxDate(new Date());
    $caseModal.find("#Status").val('0');
    $caseModal.modal("show");
});


//===============Case===================//
//#region Case

function setWeekTo(week, year) {
    $(".week-picker").weekPicker("clear");
    $(".week-picker").weekPicker("toggleWeek", week, year);
    $(".week-picker").weekPicker("updateSelection");
}
function getWeekString() {
    var dates = $(".week-picker").weekPicker("value");
    var weeks = dates.sort(function (a, b) {
        return a > b;
    }).map(function (date) {
        return moment(date, "YYYY-MM-DD").format("W[/]YYYY");
    });
    return weeks[0];
}
function clearAssignmentModal() {

    $("#assignmentTitle").val("");
    $("#assignmentDescription").val("");
    $("#assignmentStart").val("");
    $("#otherAssignment").empty();
}
function clearCaseModal() {
    $("#customerSelect").val("null"); // Select the option with a value of "1"
    $("#customerSelect").trigger("change");
    doneSlider.noUiSlider.set(0);
    $("#contactPerson").empty();
    $("#caseResponsible").empty();
    $("#projectLeader").empty();
    $("#caseTypes").empty();
    $(".week-picker").weekPicker("clear");
    $("#caseTitle").val("");
    $("#caseDescription").val("");
    $("#caseStartDate").val("");
    $("#caseDeadlineDate").val("");
    $("#estimatedTimeHour-spinner").val(1);
    $("#estimatedTimeMinute-spinner").val(0);
    $("#caseDeadlineDate").data("DateTimePicker").minDate(new Date());
    $("#caseStartDate").data("DateTimePicker").date(new Date());
    $("#caseStartDate").data("DateTimePicker").maxDate(new Date());
    $("#Status").val("0");
}
$(document).on("click", "table.projects-table tbody tr[role='row']", function (event) {
    var tr = $(this);

    var row;
    switch (tr.parent()[0].id) {
        case "pendingTbody":
            row = pendingtable.row(tr);
            break;
        case "-1Tbody":
            row = minus1table.row(tr);
            break;
        case "0Tbody":
            row = zerotable.row(tr);
            break;
        case "1Tbody":
            row = plus1table.row(tr);
            break;
        case "2Tbody":
            row = plus2table.row(tr);
            break;

    }

    var caseid = tr.attr('caseid');
    if (row.child.isShown()) {
        // This row is already open - close it
        row.child.hide();
        tr.removeClass('shown');
    }
    else {
        // Open this row
        $.ajax({
            url: "/CustomerCases/GetDetailed",
            data: { caseId: parseInt(tr.attr("caseId")) },

            success: function (data) {
                var response = data;
                row.child(showCaseDetails(data)).show();
                tr.addClass('shown');

                if (!tr.hasClass("greenClass")) {
                    tr.css("background", "");

                }
                var slider = document.getElementById(`listSlider_${data.Id}`);
                noUiSlider.create(slider,
                    {
                        start: [data.Done],
                        connect: [true, false],
                        tooltips: [true],
                        step: 1,
                        range: {
                            'min': [0],
                            'max': [100]
                        }
                    });

                if (data.CustomerContactId != null) {
                    $(`#${caseid}-contact`).text(data.contact.Name);
                }
                $(`#${caseid}-startDate`).text(data.Started);
                if (data.Ended != null) {
                    $(`#${caseid}-endDate`).text(data.Ended);
                }
                $(`#${caseid}-description`).text(data.Description);
                $(`#${caseid}-CaseId`).text(data.Id);
                $(`#${caseid}-slider`).slider("setValue", data.Done);
                $(`#${caseid}-slider`).css("margin-bottom", 0);
                $(`#${caseid}-contact`).text(data.Contact);



            },
            error: function () {
                toastr.error("Something went wrong");
            },
        })

    }
});

$("#caseTableReload").click(function () {
    reloadMainTable();
});

//opens case create modal
$("#createButton").click(function () {
    clearCaseModal();
    $("#caseDeadlineDate").data("DateTimePicker").minDate(new Date());
    $("#caseStartDate").data("DateTimePicker").date(new Date());
    $("#caseStartDate").data("DateTimePicker").maxDate(new Date());
    $("#Status").val('0');

    $.ajax({
        url: "/CustomerCases/GetCasePeople",
        data: { "customerId": $("#hiddenCurrentCustomerId").val() },
        success: function (response) {
            $.each(response.UsersList,
                function (index, item) {
                    var isCurrentUser = "";

                    if ($("#hiddenCurrentUserId").val() === item.Id)
                        isCurrentUser = "selected";
                    $("#caseResponsible")
                        .append(`<option value=${item.Id} selected=${isCurrentUser}>${item.FirstName}</option>`);
                    $("#projectLeader")
                        .append(`<option value=${item.Id} selected=${isCurrentUser}>${item.FirstName}</option>`);
                });
            $.each(response.contactsList,
                function (index, item) {
                    ;
                    $("#contactPerson")
                        .append(`<option value=${item.Id}>${item.Name}</option>`);
                });
            $.each(response.CaseTypesList,
                function (index, item) {
                    ;
                    $("#caseTypes")
                        .append(`<option value=${item.Id}>${item.TypeName}</option>`);
                });

            $caseModal.modal("show");
        },
        error: function (error) {
        }
    });
});

//Case Create modal Are you sure you want to close?
$("#caseModal").on("hide.bs.modal", function (e) {
        if (!jQuery.isEmptyObject($("#caseTitle").val()) || !jQuery.isEmptyObject($("#caseDescription").val())) {

            if (!confirm(resx.Customer.RUSureClose)) {
                return false;
            } else {
                $caseModal.attr("data-caseId", "");
            }
        }
});
var editSlider = document.getElementById('g1-div');

//Could be either create or update
$(document).on("click", "#saveCaseBtn", function () {

    var $btn = $(this).button("loading");
    const caseModal = $("#caseModal");
    const contactPerson = parseInt($("#contactPerson").val()) || null;
    const caseTypeID = parseInt($("#caseTypes").val()) || null;
    const customerID = parseInt($('#customerSelect').val()) || null;
    const customerCase = {
        "Id": parseInt(caseModal.attr("data-caseId")),
        "CustomerCaseTypeId": caseTypeID,
        "CustomerContactId": contactPerson,
        "CustomerId": customerID,
        "UserId": $("#caseResponsible").val(),
        "StartDateTime": $("#caseStartDate").data("DateTimePicker").date().toISOString(),
        "Deadline": $("#caseDeadlineDate").data("DateTimePicker").date().toISOString(),
        "Titel": $("#caseTitle").val(),
        "Description": $("#caseDescription").val(),
        "EstimatedTimeSpanIsoString": moment.duration({
            minutes: $(`#estimatedTimeMinute-spinner`).val(),
            hours: $(`#estimatedTimeHour-spinner`).val()
        }).toISOString(),
        "PercentDone": parseInt(editSlider.noUiSlider.get()),
        "ProjectResponsibleId": $("#projectLeader").val(),
        "Status": $("#Status").val(),
        "Week": getWeekString()
    };
    console.log(customerCase);
   if (caseModal.attr("data-caseId") === "") {

        $.ajax({
            url: "/CustomerCases/Create",
            type: "POST",
            data: { 'customerCase': customerCase },
            success: function (data) {

                emptycaseModal();
                caseModal.modal('hide');
                $btn.button("reset");
                reloadTables();
                
                caseModal.attr("data-caseId", "");
                toastr.success("@General.CaseCreated")
                reloadTables();
                reloadMainTable();

            },
            error: function () {
                $btn.button("reset");
                toastr.error("@Customer.Edit @Customer.failed");
            }
        });
    }
    else {
        $.ajax({
            url: "/CustomerCases/Edit",
            type: "POST",
            data: { 'customerCase': customerCase },
            success: function (data) {
                $btn.button("reset");
                emptycaseModal();
                caseModal.modal('hide');
                reloadTables();

               
                caseModal.attr("data-caseId", "");
                reloadTables();
                reloadMainTable();
            },
            error: function () {
                $btn.button("reset");
                toastr.error("@Customer.Edit @Customer.failed");
            }
        });
    }
});

$(document).off("click", ".fa-thumb-tack");
//Pins Case
$(document).on("click", ".fa-thumb-tack", function () {
    var pin = $(this);
    $.ajax({
        url: "/CustomerCases/PinToggle",
        type: "Post",
        data: { 'caseId': $(this).parent().attr("caseId") },
        success: function (data) {
            if ($(pin).hasClass("blue")) {
                $(pin).removeClass("blue");
            } else {
                $(pin).addClass("blue");
            }
            toastr.success(data.responseText);
        },
        error: function (data) {
            toastr.error("Pinning " + resx.Customer.failed);

        }
    });
});

//Gets detailed view for each case
$(document).off("click", ".mainTable");
$(document).on("click", ".mainTable", function () {

    const tr = $(this).parent()[0];
    var id = $(tr).attr("caseid");
    $(tr).toggleClass("shown");
    if ($(tr).hasClass("shown")) {
        $(`#${id}`).show();
        $.ajax({
            url: "/CustomerCases/GetDetailed",
            data: { "caseId": id },
            success: function (response) {

                if (response.CustomerContactId != null) {
                    $(`#${id}-contact`).text(response.contact.Name);
                }
                $(`#${id}-startDate`).text(response.Started);
                if (response.Ended != null) {
                    $(`#${id}-endDate`).text(response.Ended);
                }
                $(`#${id}-description`).text(response.Description);
                $(`#${id}-CaseId`).text(response.Id);


                var slider = document.getElementById(`listSlider_${response.Id}`);
                if (!slider) {
                    noUiSlider.create(slider,
                        {
                            start: [response.Done],
                            connect: [true, false],
                            tooltips: [true],
                            step: 1,
                            range: {
                                'min': [0],
                                'max': [100]
                            }
                        });
                } else {
                    slider.noUiSlider.set(response.Done);
                }
                

                //$(`#${id}-slider`).slider("setValue", response.Done);
                $(`#${id}-slider`).css("margin-bottom", 0);
            }
        });
    } else {
        $(`#${id}`).hide();
    }
});

// Ends case
$(document).off("click", ".endBtn");
$(document).on("click", ".endBtn", function () {
    const now = moment();
    const caseId = parseInt($(this).attr("data-case-id"));
    $.ajax({
        url: "/CustomerCases/EndCase",
        data: { CaseId: caseId, EndDateTime: now.toISOString() },
        success: function (data) {
            reloadMainTable(); //$("#customerCasesList").html(data);
        },
        error: function (data) {
            toastr.error(resx.Customer.Ending + " " + resx.Customer.failed);

        }
    });
});

// restarts case
$(document).off("click", ".restartBtn");
$(document).on("click", ".restartBtn", function () {
    const caseId = parseInt($(this).attr("data-case-id"));

    $.ajax({
        url: "/CustomerCases/RestartCase",
        data: { CaseId: caseId },
        success: function (data) {
            reloadMainTable(); //$("#customerCasesList").html(data);
        },
        error: function (data) {
            toastr.error(resx.Customer.Restarting + " " + resx.Customer.failed);

        }
    });
});

$(document).off("click", "#estimatedSliderBtn");
$(document).on("click", "#estimatedSliderBtn", function () {
    var caseId = $(this).attr("data-case-id");
    var percentDone = parseInt(window[`listSlider_${caseId}`].noUiSlider.get());
    $.ajax({
        url: "/CustomerCases/UpdatePercentDone",
        type: "Post",
        data: { 'caseId': caseId, "PercentDone": percentDone },
        success: function (data) {
            toastr.success(data.responseText);

            $(`#${caseId}-progressbar`).attr("data-progressbar-value", percentDone);
        },
        error: function (data) {
            toastr.error("Pinning " + resx.Customer.failed);
        }
    });
});

$(document).off("click", ".editBtn");
$(document).on("click", ".editBtn", function () {
    var caseId = $(this).attr("data-case-id");
    var customerId = $(this).attr("data-customer-id");
    emptycaseModal();
    $.ajax({
        url: "/CustomerCases/GetCase",
        data: { 'caseId': parseInt(caseId) },
        dataType: "json",
        success: function (response) {
            $('#customerSelect').val(customerId); // Select the option with a value of '1'
            $('#customerSelect').trigger('change');
            $("#caseModal").attr("data-customerId", customerId);
            $("#caseModal").attr("data-caseId", caseId);
            /*"CustomerCaseTypeId":*/
            $(`#caseTypes`).val(response.Case.CustomerCaseTypeId);
            /*"CustomerContactId":*/
            $(`#contactPerson`).val(response.Case.CustomerContactId);
            /*"UserId": */
            $(`#caseResponsible`).val(response.Case.UserId);
            $(`#projectLeader`).val(response.Case.ProjectResponsibleId);
            $(`#Status`).val(response.Case.Status);
            /*"StartDateTime": */
            if (response.Case.StartDateTime != null) {
                ($("#caseStartDate").data("DateTimePicker")
                    .date(new Date(parseInt((response.Case.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))));
            }
            /*"Deadline": */
            if (response.Case.Deadline != null) {
                $("#caseDeadlineDate").data("DateTimePicker")
                    .date(new Date(parseInt(response.Case.Deadline.replace(/\/Date\((-?\d+)\)\//, "$1"))));
            }
            /*"Titel":*/
            $("#caseTitle").val(response.Case.Titel);
            /*"Description":*/
            $("#caseDescription").val(response.Case.Description);
            /*"EstimatedTimeSpan":*/
            let dur = moment.duration(response.Case.EstimatedTimeSpanIsoString).format("HH:mm", { trim: false }).split(":");
            $("#estimatedTimeHour-spinner").val(dur[0]);
            $("#estimatedTimeMinute-spinner").val(dur[1]);
            /*"PercentDone":*/
            editSlider.noUiSlider.set(response.Case.PercentDone);
            if (response.Case.Week != null) {
                var weekArray = response.Case.Week.split("/");
                setWeekTo(weekArray[0], weekArray[1]);
            }
        },
        error: function () {
            toastr.error("Could not fetch case");
        }
    });
    $("#caseModal").modal("show");
});

$(document).off("click", "#yearOutBtn");
$(document).on("click", "#yearOutBtn", function () {
    $("#caseDeadlineDate").data("DateTimePicker").date(moment("31-12", "DD-MM"));
});

//#endregion
//===============Table Filter & Sort===================//
//===============Case END================//
function Search(sort, direction, page) {

    const searchObject = {};
    const SearchParams = [];
    const searchParamsObj = {};
    const defaultSort = $(".defaultSort").attr("data-searchKey");
    var currentPageNumber = "";
    if (pageNumber != "" && pageNumber != undefined && pageNumber != null)
        currentPageNumber = pageNumber;
    else
        currentPageNumber = $(".pagination").children().children().attr("data-pagenumber");
    if ($("#filterCases").val() != "") {
        searchObject.SearchKey = $("#filterCases").val();
    }
    searchObject.CustomerId = $("#hiddenCurrentCustomerId").val();
    searchObject.PageNumber = parseInt(currentPageNumber);
    searchObject.DefaultOrderBy = defaultSort;
    searchObject.OrderBy = $("#hiddenOrderBy").val();
    searchObject.Direction = $("#hiddenDirection").val();
    searchObject.IsCustomers = $("#hiddenIsCustomers").val();
    $.ajax({
        url: "/CustomerCases/FilterAndSearch",
        data: { "CustomerCaseDatatableViewModel": searchObject },
        type: "POST",
        success: function (data, textStatus, jqXHR) {
            $("#customerCasesList").html(data.tablePartial);
            $("#customerCasePaginationDiv").html(data.pagingPartial);
            initializeSliders();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            toastr.error(resx.Customer.failed);
        }
    });
}
function reloadMainTable(e) {
    Search(true, null);
}
$(document).on("click", ".LiPager", function (e) {
    e.preventDefault();
    pageNumber = $(this).children().attr("data-pagenumber");
    const currentPageNumber = $(this).siblings(".active").children().attr("data-pagenumber");
    if (pageNumber == resx.Customer.Next) {
        pageNumber = String(parseInt(currentPageNumber) + 1);
    } else if (pageNumber == resx.Customer.Previous) {
        pageNumber = String(parseInt(currentPageNumber) - 1);
    }


    Search(true, parseInt(pageNumber));
});
$("#filterCases").keyup(function () { //TODO: Manipulate desc asc for all the others. And find which to order by.

    clearTimeout($.data(this, "timer"));
    $(this).data("timer", setTimeout(Search(true, null), 500));
});
$(".sort").click(function () {
    var newSort = "";
    const currentClass = $(this).attr("class");
    const orderBy = $(this).attr("data-sortByName");
    const currentSort = currentClass.substr(currentClass.indexOf(" ")).trim();
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
    $(".sort").removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting");
    $(".sort").not(this).addClass("sorting");
    $(this).addClass(newSort);
    Search(true, null);
});
//===============Table Filter & Sort END===============//
//#region TimeReg
//Open and closes TimeReg

function getTimeRegtable(caseId) {
    $("#timeRegTBody").empty();
    $.ajax({
        url: "/CustomerCases/GetCaseTimeRegs",
        data: { 'caseId': caseId },
        dataType: "JSON",
        success: function (data) {
            $.each(data.TimeRegsList,
                function (i, item) {
                    var caseAssignmentTitle = "None";
                    if (item.CaseAssignment != null) {
                        caseAssignmentTitle = item.CaseAssignment.Title;
                    };
                    $("#timeRegTable").append(
                        `<tr data-caseid="${item.Id}" class="TimeRegRemove${item.Id} even expandCollapseDetails">
                                    <td id="timeregDateTime-start-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                    <td id="timeregDateTime-end-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.EndDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                    <td>${item.User.FirstName} ${item.User.LastName}</td>
                                    <td id="${item.Id}-interval">${moment.duration(item.Interval).format("hh:mm", { trim: false })}</td>
                                    <td style="width: 15px" class="details-control TimeReg"></td>

                                    <tr class="TimeReg TimeRegRemove${item.Id}" id="timeReg${item.Id}" style="display: none"><td colspan="6">
                                        <table cellpadding="5" cellspacing="0" border="0" class="table table-hover table-condensed">
                                            <tbody>
                                                <tr>
                                                    <td>${resx.Customer.Assignment}:</td>
                                                    <td style="max-width:150px; word-wrap:break-word" id="${item.Id}-assignment" data-editable-${item.Id}>${caseAssignmentTitle}</td>
                                                </tr>
                                                <tr>
                                                    <td>${resx.TimeReg.Description}:</td><td style="max-width:150px; word-wrap:break-word" id="timeregDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                                </tr>
                                                <tr>
                                                    <td id="${item.Id}-actions">
                                                        <button id="${item.Id}-editTimeRegBtn" style="margin-right: 10px" data-timeReg-id="${item.Id}" class="btn btn-xs editTimeRegBtn btn-info pull-left">${resx.TimeReg.Edit}</button>
                                                        <button id="${item.Id}-DeleteBtn" data-timeReg-id="${item.Id}" class="btn btn-xs deleteTimeRegBtn btn-danger pull-left">${resx.TimeReg.Delete}</button>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        </td>
                                    </tr>
                                </tr>`);
                });
        },
        error: function () {
            toastr.error("Could not fetch Time Registration");
        }
    });
}
function clearTimeRegModal() {

    $("#timeRegTitle").val("");
    $("#timeRegDescription").val("");
    $("#timeRegStart").val("");
    $("#timeRegEnd").val("");
    $("#timeRegAssignment").empty();
    $("#timeRegTBody").empty();
}

$(document).off("click", ".TimeReg");
$(document).on("click", ".TimeReg", function () {
    const tr = $(this).parent()[0];
    const id = $(tr).attr("data-caseid");
    $(tr).toggleClass("shown");
    if ($(tr).hasClass("shown")) {
        $(`#timeReg${id}`).show();
    } else {
        $(`#timeReg${id}`).hide();
    }
});

// Opens Timereg Modal
$(document).off("click", ".timeRegBtn");
$(document).on("click", ".timeRegBtn", function () {
    const caseId = $(this).attr("data-case-id");
    $("#TimeRegModal").modal("show");
    $("#TimeRegModal").attr("data-caseId", caseId);
    getTimeRegtable(caseId);
    $("#timeregTableReload").attr("data-caseId", caseId);
    getAssignmentsDropdown(caseId, $("#timeRegAssignment"));
});

$(document).off("click", "#timeregTableReload");
$(document).on("click", "#timeregTableReload", function () {
    let caseId = $("#timeregTableReload").attr("data-caseId");
    getTimeRegtable(parseInt(caseId));
});

//saves new Timereg
$("#saveTimeRegBtn").click(function () {
    var $btn = $(this).button("loading");
    var caseId = $("#TimeRegModal").attr("data-caseId");
    var startTime = $("#timeRegStart").data("DateTimePicker").date().toISOString();
    var endTime = $("#timeRegEnd").data("DateTimePicker").date().toISOString();
    const timeReg = {
        "Id": null,
        "startTime": $("#timeRegStart").data("DateTimePicker").date().toISOString(),
        "endTime": $("#timeRegEnd").data("DateTimePicker").date().toISOString(),
        "CustomerCaseId": caseId,
        "CaseAssignmentId": $("#timeRegAssignment").val(),
        "Description": $("#timeRegDescription").val(),
        "Title": $("#timeRegTitle").val(),
        "UserId": $("#hiddenCurrentUserId").val()
    };
    $.ajax({
        url: "/CustomerCases/CreateTimeReg",
        type: "Post",
        data: { 'timeReg': timeReg, "startTime": startTime, "endTime": endTime },
        success: function (data) {

            const oldTimeEstimate = moment.duration($(`#${caseId}-TotalTimeUsed`).text());
            const newTimeEstimate = oldTimeEstimate.add(moment.duration(moment(endTime).diff(moment(startTime))));
            $(`#${caseId}-TotalTimeUsed`).text(`${moment.duration(newTimeEstimate).format("HH:mm", { trim: false })}`);

            $btn.button("reset");

            toastr.success(resx.TimeReg.Saved);
            getTimeRegtable($("#TimeRegModal").attr("data-caseId"));
            clearTimeRegModal();

        },
        error: function () {
            $btn.button("reset");
            toastr.error(resx.TimeReg.Saving + " " + resx.Customer.failed);
        }
    });
});

//clears modal when closed
$("#TimeRegModal").on("hidden.bs.modal", function () {
    $("#TimeRegModal").attr("data-caseId", "");
    clearTimeRegModal();
});

// Timereg Are you sure you want to close?
$("#TimeRegModal").on("hide.bs.modal", function (e) {
    if (!jQuery.isEmptyObject($("#timeRegTitle").val()) ||
        !jQuery.isEmptyObject($("#timeRegDescription").val())) {

        if (!confirm(resx.Customer.RUSureClose)) return false;
    };
});

// Deletes a timereg
$(document).off("click", ".deleteTimeRegBtn");
$(document).on("click", ".deleteTimeRegBtn", function () {
    var timeRegId = parseInt($(this).attr("data-timeReg-id"));
    var caseId = $("#TimeRegModal").attr("data-caseId");
    var oldTimeEstimate = moment.duration($(`#${caseId}-TotalTimeUsed`).text());
    $.SmartMessageBox({
        title: resx.TimeReg.Delete + "?",
        content: resx.TimeReg.RUSure + " " + resx.TimeReg.TimeRegistration,
        buttons: `[${resx.TimeReg.No}][${resx.TimeReg.Yes}]`
        },
        function (ButtonPressed) {
            if (ButtonPressed === resx.TimeReg.Yes) {

                $.ajax({
                    url: "/CustomerCases/DeleteTimeReg",
                    data: { timeRegId: timeRegId },
                    success: function (data) {
                        $(`.TimeRegRemove${timeRegId}`).fadeOut(300, function () { $(this).remove(); });

                        const newTimeEstimate =
                            oldTimeEstimate.subtract(moment.duration($(`#${timeRegId}-interval`).text()));
                        $(`#${caseId}-TotalTimeUsed`)
                            .text(`${moment.duration(newTimeEstimate).format("HH:mm", { trim: false })}`);
                    },
                    error: function (data) {
                        toastr.error("Delete failed");

                    }
                });
            }
            if (ButtonPressed === resx.TimeReg.No) {
                $.smallBox({
                    iconSmall: "fa fa-times fa-2x fadeInRight animated",
                    timeout: 4000
                });
            }
        }
    );

});

//Makes it so a user can edit a timereg
$(document).off("click", ".editTimeRegBtn");
$(document).on("click", ".editTimeRegBtn", function () {
    var timeRegId = parseInt($(this).attr("data-timeReg-id"));
    var $el = $(`[data-editable-${timeRegId}]`);
    $(this).replaceWith(
        `<button id="${timeRegId}-saveTimeRegBtn" style="margin-right: 10px" data-timeReg-id="${timeRegId
        }" class="btn btn-xs editSaveTimeRegBtn btn-success pull-left">${resx.TimeReg.Save}</button>
                <button id="${timeRegId}-cancelTimeRegBtn" data-timeReg-id="${timeRegId}" style="margin-right: 10px" class="btn btn-xs cancelTimeRegBtn btn-normal pull-left">${resx.TimeReg.Cancel}</button>`);
    $.each($el,
        function (i, item) {
            const $width = $($el[i]).width();
            const $id = $($el[i]).prop("id");
            if ($id === `${timeRegId}-assignment`) {
                $(item).attr(`data-saveable-${timeRegId}`, "");
                $(item).removeAttr(`data-editable-${timeRegId}`);
                const content = $("#timeRegAssignment").clone();
                $(`#${timeRegId}-assignment`).empty();
                $(`#${timeRegId}-assignment`).append(content);
            } else {
                const $td = $(`<td style='width: ${$width}px'> </td>`);
                const $input =
                    $(`<input class="form-control datepicker" id="${$id}" data-saveable-${timeRegId} style="width:${$width
                        }px"/>`).val($(item).text());

                $($td).append($input);
                $(item).replaceWith($td);
            }
        });
    $(`#timeregDateTime-start-${timeRegId}`).datetimepicker({
        locale: "da",
        sideBySide: true
    });

    $(`#timeregDateTime-end-${timeRegId}`).datetimepicker({
        locale: "da",
        sideBySide: true,
    });

    $(`#timeregDateTime-end-${timeRegId}`).on("dp.change",
        function (e) {

            $(`#timeregDateTime-start-${timeRegId}`).data("DateTimePicker").maxDate(e.date);
            const timeStart = $(`#timeregDateTime-start-${timeRegId}`).data("DateTimePicker").date()
                .toISOString();
            const timeEnd = $(`#timeregDateTime-end-${timeRegId}`).data("DateTimePicker").date().toISOString();
            $(`#${timeRegId}-interval`).text(moment
                .utc(moment.duration(moment(timeEnd).diff(moment(timeStart))).asMilliseconds())
                .format("HH:mm"));
        });

    $(`#timeregDateTime-start-${timeRegId}`).on("dp.change",
        function (e) {
            $(`#timeregDateTime-end-${timeRegId}`).data("DateTimePicker").minDate(e.date);
            const timeStart = $(`#timeregDateTime-start-${timeRegId}`).data("DateTimePicker").date()
                .toISOString();
            const timeEnd = $(`#timeregDateTime-end-${timeRegId}`).data("DateTimePicker").date().toISOString();
            $(`#${timeRegId}-interval`).text(moment
                .utc(moment.duration(moment(timeEnd).diff(moment(timeStart))).asMilliseconds())
                .format("HH:mm"));
        });
});

//returns the timereg to normal
$(document).off("click", ".cancelTimeRegBtn");
$(document).on("click", ".cancelTimeRegBtn", function () {
    var timeRegId = parseInt($(this).attr("data-timeReg-id"));
    var $el = $(`[data-saveable-${timeRegId}]`);
    $(this).replaceWith(
        `<button id="${timeRegId}-editTimeRegBtn" data-timeReg-id="${timeRegId
        }" class="btn btn-xs editTimeRegBtn btn-info pull-left">${resx.Customer.Edit}</button> `);
    $(`#${timeRegId}-saveTimeRegBtn`).remove();

    $.each($el,
        function (i, item) {
            const $id = $($el[i]).prop("id");
            if ($id === `${timeRegId}-assignment`) {
                const selected = $(item).children().find(":selected").text();
                $(`#${timeRegId}-assignment`).empty();
                $(`#${timeRegId}-assignment`).append(selected);
                $(item).attr(`data-editable-${timeRegId}`, "");
                $(item).removeAttr(`data-saveable-${timeRegId}`);

            } else {

                const $td = $(`<td data-editable-${timeRegId} id=${$id}>${$(item).val()} </td>`);

                $(item).parent().replaceWith($td);
            }

        });
    $.ajax({
        url: "/Timeregistration/GetTimeReg",
        type: "Get",
        data: { "TimeRegId": timeRegId },
        success: function (data) {
            const timeStart = new Date(parseInt((data.TimeReg.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")));
            const timeEnd = new Date(parseInt((data.TimeReg.EndDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")));
            $(`#timeregDateTime-start-${timeRegId}`).text(
                moment(timeStart)
                    .format("DD.MM.YYYY HH:mm"));
            $(`#timeregDateTime-end-${timeRegId}`)
                .text(moment(timeEnd)
                    .format("DD.MM.YYYY HH:mm"));

            $(`#${timeRegId}-interval`).text(moment
                .utc(moment.duration(moment(timeEnd).diff(moment(timeStart))).asMilliseconds())
                .format("HH:mm"));

            $(`#${timeRegId}-assignment`).text(data.CaseAssignment.Title);
            $(`#timeregDescription-${timeRegId}`).text(data.Description);
        },
        error: function (data) {
            toastr.error("Error");
        }
    });
});

//Saves the edited timereg
$(document).off("click", ".editSaveTimeRegBtn");
$(document).on("click", ".editSaveTimeRegBtn", function () {
    var timeRegId = parseInt($(this).attr("data-timeReg-id"));
    var $el = $(`[data-saveable-${timeRegId}]`);
    $(this).replaceWith(`<button id="${timeRegId}-editTimeRegBtn" 
        data-timeReg-id="${timeRegId} class="btn btn-xs editTimeRegBtn btn-info pull-left">${resx.Customer.Edit}</button>`);
    $(`#${timeRegId}-cancelTimeRegBtn`).remove();

    const timeStart = $(`#timeregDateTime-start-${timeRegId}`).data("DateTimePicker").date().toISOString();
    const timeEnd = $(`#timeregDateTime-end-${timeRegId}`).data("DateTimePicker").date().toISOString();
    const assignmentId = $(`#${timeRegId}-assignment`).children().find(":selected").val();
    $(`#${timeRegId}-interval`).text(moment
        .utc(moment.duration(moment(timeEnd).diff(moment(timeStart))).asMilliseconds()).format("HH:mm"));
    $.each($el,
        function (i, item) {
            const $id = $($el[i]).prop("id");
            if ($id === `${timeRegId}-assignment`) {
                const selected = $(item).children().find(":selected").text();
                $(`#${timeRegId}-assignment`).empty();
                $(`#${timeRegId}-assignment`).append(selected);
                $(item).attr(`data-editable-${timeRegId}`, "");
                $(item).removeAttr(`data-saveable-${timeRegId}`);
            } else {
                const $td = $(`<td data-editable-${timeRegId} id=${$id}>${$(item).val()} </td>`);

                $(item).parent().replaceWith($td);
            }
        });

    const timeReg = {
        "Id": timeRegId,
        "Description": $(`#timeregDescription-${timeRegId}`).text(),
        "CaseAssignmentId": parseInt(assignmentId),
        "StartDateTime": timeStart,
        "EndDateTime": timeEnd,
    };
    $.ajax({
        url: "/CustomerCases/TimeRegEdit",
        type: "Post",
        data: { "timeReg": timeReg },
        success: function (data) {
            reloadMainTable();
            toastr.success("Success");
        },
        error: function (data) {
            toastr.error(resx.Customer.Edit + " " + resx.Customer.failed);

        }
    });

});
    //#endregion
    //===============TimeReg End============//

//===============Assignments=================//
//#region Assignments
function getAssignmentsDropdown(caseId, el) {
    el.append(`<option value=${null}>${resx.Customer.None}</option>`);

    $.ajax({
        url: "/CustomerCases/GetCaseAssignments",
        data: { caseId: caseId },
        success: function (data) {
            debugger

            $.each(data.AssignmentsList,
                function (index, item) {
                    el.append(`<option value=${item.Id}>${item.Title}</option>`);
                });
        },
        error: function (error) {
            debugger;
        }
    });
}
function getAssignmentsTable(caseId) {
    $("#AssignmentsTBody").empty();
    $("#AssignmentsDoneTBody").empty();

    $.ajax({
        url: "/CustomerCases/GetCaseAssignments",
        data: { 'caseId': caseId },
        dataType: "json",
        success: function (data) {
            $("#otherAssignment").append(`<option value="null">${resx.Customer.None}</option>`);
            $.each(data.AssignmentsList,
                function (index, item) {
                    $("#otherAssignment").append(`<option value=${item.Id}>${item.Title}</option>`);
                });
            //Active Assignment List
            $.each(data.AssignmentsList.filter(x => x.Done === null || x.Done === false),
                function (i, item) {

                    var linkedTitle = resx.Customer.None;
                    if (item.LinkedCaseAssignmentId != null) {
                        linkedTitle =
                            (data.AssignmentsList.find(x => item.LinkedCaseAssignmentId === x.Id)).Title;
                    };
                    $("#assignmentsTable").append(
                        `<tr id="${item.Id}-assignment" data-plusesti="${item.AddToCaseEstimate}" data-assignmentId="${item.Id}" data-caseid="${item.CustomerCaseId}" data-ended="false" class="AssignmentRemove${item.Id} even expandCollapseDetails">
                                    <td style="max-width:150px; word-wrap:break-word" id="AssignmentTitle-${item.Id}" data-editable-${item.Id} style="display: block;">${item.Title}</td>
                                    <td data-value="${item.UserId}"data-editable-${item.Id} id="${item.Id}-responsible">${item.User.FirstName} ${item.User.LastName}</td>
                                    <td id="AssignmentDateTime-start-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                    <td id="AssignmentDateTime-Deadline-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.Deadline).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>

                                    <td id="${item.Id}-interval" data-editable-${item.Id}>${moment.duration(item.EstimatedTimeSpan).format("HH:mm", { trim: false })}</td>
                                    <td style="width: 15px" class="details-control Assignment"></td>

                                    <tr class="Assignment AssignmentRemove${item.Id}" id="Assignment${item.Id}" style="display: none">
                                        <td colspan="6">
                                            <table cellpadding="5" cellspacing="0" border="0" class="table table-hover table-condensed">
                                                <tbody>
                                                    <tr>
                                                        <td>${resx.Customer.LinkedAssignment}:</td>
                                                        <td data-value="${item.LinkedCaseAssignmentId}" style="max-width:150px; word-wrap:break-word" id="${item.Id}-linkedAssignment" data-editable-${item.Id}>${linkedTitle}  </td>
                                                    </tr>
                                                    <tr>
                                                        <td>${resx.TimeReg.Description}:</td>
                                                        <td style="max-width:150px; word-wrap:break-word" id="AssignmentDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                                    </tr>
                                                    <tr>
                                                        <td id="${item.Id}-assignmentsActions">
                                                            <button id="${item.Id}-endAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id} "data-case-id="${caseId}" class="btn btn-xs endAssignmentBtn btn-warning pull-left">${resx.Customer.End}</button>
                                                            <button id="${item.Id}-editAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id}" class="btn btn-xs editAssignmentBtn btn-info pull-left">${resx.TimeReg.Edit}</button>
                                                            <button id="${item.Id}-DeleteBtn" data-Assignment-id="${item.Id}" class="btn btn-xs deleteAssignmentBtn btn-danger pull-left">${resx.TimeReg.Delete}</button>
                                                        </td>
                                                        <td></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </tr>`);
                }
            );
            //Ended Assignment list
            $.each(data.AssignmentsList.filter(x => x.Done === true),
                function (i, item) {
                    var endDate;
                    if (item.EndDateTime === null) {
                        endDate = resx.Customer.NotDone;
                    } else {
                        endDate = moment(
                            new Date(parseInt((item.EndDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm");
                    }
                    var linkedTitle = resx.Customer.None;
                    if (item.LinkedCaseAssignmentId != null) {
                        linkedTitle =
                            (data.AssignmentsList.find(x => item.LinkedCaseAssignmentId === x.Id)).Title;
                    };
                    $("#assignmentsDoneTable").append(
                        `<tr id="${item.Id}-assignment" data-plusesti="${item.AddToCaseEstimate}" data-assignmentId="${item.Id}" data-caseid="${item.CustomerCaseId}" data-ended="true" class="AssignmentRemove${item.Id} even expandCollapseDetails">
                                    <td style="max-width:150px; word-wrap:break-word" id="AssignmentTitle-${item.Id}" data-editable-${item.Id} style="display: block;">${item.Title}</td>
                                    <td data-value="${item.UserId}"data-editable-${item.Id} id="${item.Id}-responsible">${item.User.FirstName} ${item.User.LastName}</td>
                                    <td id="AssignmentDateTime-start-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                    <td id="AssignmentDateTime-Ended-${item.Id}" data-editable-${item.Id}>${endDate}</td>

                                    <td id="${item.Id}-interval" data-editable-${item.Id}>${moment.duration(item.EstimatedTimeSpan).format("HH:mm", { trim: false })}</td>
                                    <td style="width: 15px" class="details-control Assignment"></td>

                                    <tr class="Assignment AssignmentRemove${item.Id}" id="Assignment${item.Id}" style="display: none">
                                        <td colspan="6">
                                            <table cellpadding="5" cellspacing="0" border="0" class="table table-hover table-condensed">
                                                <tbody>
                                                    <tr>
                                                        <td>${resx.Customer.Deadline}:</td>
                                                        <td id="AssignmentDateTime-Deadline-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.Deadline).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                                    </tr>
                                                    <tr>
                                                        <td>${resx.Customer.LinkedAssignment}:</td>
                                                        <td data-value="${item.LinkedCaseAssignmentId}" style="max-width:150px; word-wrap:break-word" id="${item.Id}-linkedAssignment" data-editable-${item.Id}>${linkedTitle}  </td>
                                                    </tr>
                                                    <tr>
                                                        <td>${resx.TimeReg.Description}:</td>
                                                        <td style="max-width:150px; word-wrap:break-word" id="AssignmentDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                                    </tr>
                                                    <tr>
                                                        <td id="${item.Id}-assignmentsActions">
                                                            <button id="${item.Id}-restartAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id} "data-case-id="${caseId}" class="btn btn-xs restartAssignmentBtn btn-warning pull-left">${resx.Customer.Restart}</button>
                                                            <button id="${item.Id}-editAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id}" class="btn btn-xs editAssignmentBtn btn-info pull-left">${resx.TimeReg.Edit}</button>
                                                            <button id="${item.Id}-DeleteBtn" data-Assignment-id="${item.Id}" class="btn btn-xs deleteAssignmentBtn btn-danger pull-left">${resx.TimeReg.Delete}</button>
                                                        </td>
                                                        <td></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </tr>`);
                    if (item.Done === true) {
                    }
                }
            );
        },
        error: function () {
            toastr.error("Could not fetch Time Registration");
        }
    });
}
function getUsersNames($el) {
    $.ajax({
        url: "/CustomerCases/GetUsersNames",
        data: {},
        success: function (data) {
            $.each(data.Users,
                function (index, item) {
                    var lastName = "";
                    if (item.LastName === null) {
                        console.log("null lastname");
                    }
                    if (item.LastName != null) {
                        lastName = item.LastName;
                    };

                    $($el).append(
                        `<option value=${item.Id}>${item.FirstName} ${lastName}</option>`
                    );
                });
        },
        error: function (error) {
        }
    });
}
function assignmentTrCancel($el, assignmentId) {
    $(`#${assignmentId}-cancelAssignmentBtn`).replaceWith(
        `<button id="${assignmentId}-editAssignmentBtn" data-Assignment-id="${assignmentId
        }" class="btn btn-xs editAssignmentBtn btn-info pull-left">${resx.TimeReg.Edit}</button> `);
    $(`#${assignmentId}-saveAssignmentBtn`).remove();
    $.each($el, function (i, item) {
        const $id = $($el[i]).prop("id");
        if ($id === `${assignmentId}-linkedAssignment`) {
            const selected = $(item).children().find(":selected").text();
            $(`#${assignmentId}-linkedAssignment`).empty();
            $(`#${assignmentId}-linkedAssignment`).append(selected);
            $(item).attr(`data-editable-${assignmentId}`, "");
            $(item).removeAttr(`data-saveable-${assignmentId}`);
        } else if ($id === `${assignmentId}-responsible`) {
            const selected = $(item).children().find(":selected").text();
            $(`#${assignmentId}-responsible`).empty();
            $(`#${assignmentId}-responsible`).append(selected);
            $(item).attr(`data-editable-${assignmentId}`, "");
            $(item).removeAttr(`data-saveable-${assignmentId}`);
        } else if ($id === `${assignmentId}-interval`) {

            const dur = moment.duration({
                minutes: $(`#${assignmentId}-EditEstimatedMinute-spinner`).val(),
                hours: $(`#${assignmentId}-EditEstimatedHour-spinner`).val()
            }).format("HH:mm", { trim: false });
            $(`#${assignmentId}-interval`).empty();
            $(`#${assignmentId}-interval`).text(dur);
            $(item).attr(`data-editable-${assignmentId}`, "");
            $(item).removeAttr(`data-saveable-${assignmentId}`);
        } else {
            const $td = $(`<td data-editable-${assignmentId} id=${$id}>${$(item).val()} </td>`);
            $(item).parent().replaceWith($td);
        }
    });
}
function clearAssignmentModal() {

    $("#assignmentTitle").val("");
    $("#assignmentDescription").val("");
    $("#assignmentStart").val("");
    $("#otherAssignment").empty();
}
function emptycaseModal() {

    $("#customerSelect").val("null"); // Select the option with a value of "1"
    $("#customerSelect").trigger("change");
    doneSlider.noUiSlider.set(0);
    $("#contactPerson").empty();
    $("#caseResponsible").empty();
    $("#projectLeader").empty();
    $("#caseTypes").empty();
    $(".week-picker").weekPicker("clear");
    $("#caseTitle").val("");
    $("#caseDescription").val("");
    $("#caseStartDate").val("");
    $("#caseDeadlineDate").val("");
    $("#estimatedTimeHour-spinner").val(1);
    $("#estimatedTimeMinute-spinner").val(0);

    $("#caseDeadlineDate").data("DateTimePicker").minDate(new Date());
    $("#caseStartDate").data("DateTimePicker").date(new Date());
    $("#caseStartDate").data("DateTimePicker").maxDate(new Date());
    $("#Status").val("0");
}
function setWeekTo(week, year) {
    $(".week-picker").weekPicker("clear");
    $(".week-picker").weekPicker("toggleWeek", week, year);
    $(".week-picker").weekPicker("updateSelection");
}
function getWeekString() {
    var dates = $(".week-picker").weekPicker("value");
    var weeks = dates.sort(function (a, b) {
        return a > b;
    }).map(function (date) {
        return moment(date, "YYYY-MM-DD").format("W[/]YYYY");
    });
    return weeks[0];
}
//===============Assignments END=================//
//#endregion Assignments
