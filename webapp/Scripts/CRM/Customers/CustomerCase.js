

//=================INIT==================//
    //#region INIT

    function initializeSliders() {
        $.each($(".listSlider"),
            function (i, item) {
                window[item.id] = document.getElementById(item.id);
                let start = "0";
                if ($(`#${item.id}`).attr("data-sliderVal") != null) {
                    start = $(`#${item.id}`).attr("data-sliderVal");
                };
                noUiSlider.create(window[item.id],
                    {
                        start: [start],
                        connect: [true, false],
                        tooltips: [true],
                        step: 1,
                        range: {
                            'min': [0],
                            'max': [100]
                        }

                    });
            });
    }
$(document).ready(function () {
    $("#timeRegStart").datetimepicker({
        locale: "da",
        sideBySide: true
    });

    $("#assignmentStart").datetimepicker({
        locale: "da"
    });
    $("#assignmentDeadlineDate").datetimepicker({
        locale: "da"
    });

    $("#assignmentDeadlineDate").on("dp.change",
        function(e) {
            $("#assignmentStart").data("DateTimePicker").maxDate(e.date);
        });

    $("#assignmentStart").on("dp.change",
        function(e) {
            $("#assignmentDeadlineDate").data("DateTimePicker").minDate(e.date);
        });

    $("#timeRegEnd").datetimepicker({
        locale: "da",
        sideBySide: true
    });

    $("#timeRegEnd").on("dp.change",
        function(e) {
            $("#timeRegStart").data("DateTimePicker").maxDate(e.date);
        });

    $("#timeRegStart").on("dp.change",
        function(e) {
            $("#timeRegEnd").data("DateTimePicker").minDate(e.date);
        });

    $("#caseStartDate").datetimepicker({
        date: new Date(),
        locale: "da"
    });

    $("#caseDeadlineDate").datetimepicker({
        locale: "da"
    });

    $("#caseDeadlineDate").on("dp.change",
        function(e) {
            $("#caseStartDate").data("DateTimePicker").maxDate(e.date);
        });

    $("#caseStartDate").on("dp.change",
        function(e) {
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
var editSlider = document.getElementById('g1-div');
noUiSlider.create(editSlider, {
    start: [0],
    connect: [true, false],
    tooltips: [true],
    step: 1,
    range: {
        'min': [0],
        'max': [100]
    }
    
});

//===============Case===================//
    //#region Case

$("#caseTableReload").click(function() {
    reloadMainTable();
});

    //opens case create modal
    $("#createButton").click(function () {
        emptyeditModal();
        $("#caseDeadlineDate").data("DateTimePicker").minDate(new Date());
        $("#caseStartDate").data("DateTimePicker").date(new Date());
        $("#caseStartDate").data("DateTimePicker").maxDate(new Date());

        $.ajax({
            url: "/CustomerCases/GetCasePeople",
            data: { "customerId": $("#hiddenCurrentCustomerId").val() },
            success: function (data) {
                $.each(data.UsersList,
                    function (index, item) {
                        var isCurrentUser = "";
                        if ($("#hiddenCurrentUserId").val() === item.Id)
                            isCurrentUser = "selected";
                        $("#caseResponsible")
                            .append(`<option value=${item.Id} selected=${isCurrentUser}>${item.FirstName}</option>`);
                    });
                $.each(data.contactsList,
                    function (index, item) {
                        ;
                        $("#contactPerson")
                            .append(`<option value=${item.Id}>${item.Name}</option>`);
                    });
                $.each(data.CaseTypesList,
                    function (index, item) {
                        ;
                        $("#caseTypes")
                            .append(`<option value=${item.Id}>${item.TypeName}</option>`);
                    });
                $("#createModal").modal("show");
            },
            error: function (error) {
            }
        });
    });

    //Case Create modal Are you sure you want to close?
    $("#createModal").on("hide.bs.modal",function (e) {
            ;
            if (!jQuery.isEmptyObject($("#caseTitle").val()) || !jQuery.isEmptyObject($("#caseDescription").val())) {

                if (!confirm("Are you sure, you want to close?")) {
                    return false;
                } else {
                    $("#createModal").attr("data-caseId", "");
                }
            }
    });
    
    //Saves Case also saves EDIT case
    $("#saveCaseBtn").click(function () {
        var $btn = $(this).button("loading");

        const customerCase = {
            "Id": null,
            "CustomerCaseTypeId": parseInt($("#caseTypes").selected().val()),
            "CustomerContactId": $("#contactPerson").selected().val(),
            "UserId": $("#caseResponsible").selected().val(),
            "CustomerId": $("#hiddenCurrentCustomerId").val(),
            "StartDateTime": $("#caseStartDate").data("DateTimePicker").date().toISOString(),
            "Deadline": $("#caseDeadlineDate").data("DateTimePicker").date().toISOString(),
            "Titel": $("#caseTitle").val(),
            "Description": $("#caseDescription").val(),
            "EstimatedTimeSpanIsoString": moment.duration({
                minutes: $(`#estimatedTimeMinute-spinner`).val(),
                hours: $(`#estimatedTimeHour-spinner`).val()
            }).toISOString(),
            "PercentDone": parseInt(editSlider.noUiSlider.get())
        };
        if ($("#createModal").attr("data-caseId") === "") {
            $.ajax({
                url: "/CustomerCases/Create",
                type: "Post",
                data: { 'customerCase': customerCase },
                success: function (data) {
                    emptyeditModal();
                    reloadMainTable();//$("#customerCasesList").html(data);
                    $("#createModal").modal("hide");
                    $btn.button("reset");
                },
                error: function () {
                    toastr.error("Saving failed");
                }
            });
        } else {
            const caseId = $("#createModal").attr("data-caseId");
            customerCase.Id = caseId;
            $.ajax({
                url: "/CustomerCases/Edit",
                type: "Post",
                data: { 'customerCase': customerCase },
                success: function (data) {
                   
                    emptyeditModal();
                    $("#createModal").modal("hide");
                    reloadMainTable();//$("#customerCasesList").html(data);
                    $btn.button("reset");
                    $("#createModal").attr("data-caseId", "");
                },
                error: function () {
                    toastr.error("Edit failed");
                    $btn.button("reset");

                    $("#createModal").on("hidden.bs.modal",
                        function () {
                            $("#createModal").attr("data-caseId", "");
                        });
                    }
            });
        }
    });


$(document).off("click", ".fa-thumb-tack");
    //Pins Case
$(document).on("click",".fa-thumb-tack",function () {
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
                toastr.error("Pinning failed");

            }
        });
    });

    //Gets detailed view for each case
$(document).off("click", ".mainTable");
$(document).on("click",".mainTable",function () {
        
        const tr = $(this).parent()[0];
        var id = $(tr).attr("caseid");
        $(tr).toggleClass("shown");
        if ($(tr).hasClass("shown")) {
            $(`#${id}`).show();
            $.ajax({
                url: "/CustomerCases/GetDetailed",
                data: { "caseId": id },
                success: function (data) {
                    ;
                    if (data.CustomerContactId != null) {
                        $(`#${id}-contact`).text(data.contact.Name);
                    }
                    $(`#${id}-startDate`).text(data.Started);
                    if (data.Ended != null) {
                        $(`#${id}-endDate`).text(data.Ended);
                    }
                    $(`#${id}-description`).text(data.Description);
                    $(`#${id}-CaseId`).text(data.Id);
                    $(`#${id}-slider`).slider("setValue", data.Done);
                    $(`#${id}-slider`).css("margin-bottom", 0);
                }
            });
        } else {
            $(`#${id}`).hide();
        }
    });

function emptyeditModal() {
    editSlider.noUiSlider.set(0);
    $("#contactPerson").empty();
    $("#caseResponsible").empty();
    $("#caseTypes").empty();

    $("#caseTitle").val("");
    $("#caseDescription").val("");
    $("#caseStartDate").val("");
    $("#caseDeadlineDate").val("");
    $("#estimatedTimeHour-spinner").val(1);
    $("#estimatedTimeMinute-spinner").val(0);
}

// Ends case
$(document).off("click", ".endBtn");
$(document).on("click",".endBtn",function () {
        const now = moment();
        const caseId = parseInt($(this).attr("data-case-id"));
        $.ajax({
            url: "/CustomerCases/EndCase",
            data: { CaseId: caseId, EndDateTime: now.toISOString() },
            success: function (data) {
                reloadMainTable(); //$("#customerCasesList").html(data);
            },
            error: function (data) {
                toastr.error("Ending Failed failed");

            }
        });
    });

    // restarts case
$(document).off("click", ".restartBtn");
$(document).on("click",".restartBtn",function () {
        const caseId = parseInt($(this).attr("data-case-id"));

        $.ajax({
            url: "/CustomerCases/RestartCase",
            data: { CaseId: caseId },
            success: function (data) {
                reloadMainTable(); //$("#customerCasesList").html(data);
            },
            error: function (data) {
                toastr.error("Restarting Failed failed");

            }
        });
    });
$(document).off("click", "#estimatedSliderBtn");
$(document).on("click","#estimatedSliderBtn",function () {
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
                toastr.error("Pinning failed");

            }
        });
    });
$(document).off("click", ".editBtn");
$(document).on("click", ".editBtn", function () {
    var caseId = $(this).attr("data-case-id");
    emptyeditModal();
    $.ajax({
        url: "/CustomerCases/GetCasePeople",
        data: { "customerId": $("#hiddenCurrentCustomerId").val() },
        success: function (data) {
            debugger;
            $.each(data.UsersList,
                function (index, item) {
                    var isCurrentUser = "";
                    if ($("#hiddenCurrentUserId").val() === item.Id)
                        isCurrentUser = "selected";
                    $("#caseResponsible").append(`<option value=${item.Id}>${item.FirstName}</option>`);
                });
            $.each(data.contactsList,
                function (index, item) {
                    ;
                    $("#contactPerson").append(`<option value=${item.Id}>${item.Name}</option>`);
                });
            $.each(data.CaseTypesList,
                function (index, item) {
                    ;
                    $("#caseTypes").append(`<option value=${item.Id}>${item.TypeName}</option>`);
                });
            $("#createModal").modal("show");
        },
        error: function (error) {
        }
    });
    $.ajax({
        url: "/CustomerCases/GetCase",
        data: { 'caseId': parseInt(caseId) },
        dataType: "json",
        success: function (data) {
            ;
            $("#createModal").attr("data-caseId", caseId);
            /*"CustomerCaseTypeId":*/
            $(`#caseTypes option[value = ${data.Case.CustomerCaseTypeId}]`).prop("selected", true);
            /*"CustomerContactId":*/
            $(`#contactPerson option[value=${data.Case.CustomerContactId}]`).prop("selected", true);
            /*"UserId": */
            ;
            $(`#caseResponsible option[value=${data.Case.UserId}]`).prop("selected", true);
            /*"StartDateTime": */
            ($("#caseStartDate").data("DateTimePicker")
                .date(new Date(parseInt((data.Case.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))));
            /*"Deadline": */
            $("#caseDeadlineDate").data("DateTimePicker")
                .date(new Date(parseInt(data.Case.Deadline.replace(/\/Date\((-?\d+)\)\//, "$1"))));
            /*"Titel":*/
            $("#caseTitle").val(data.Case.Titel);
            /*"Description":*/
            $("#caseDescription").val(data.Case.Description);
            /*"EstimatedTimeSpan":*/
            let dur = moment.duration(data.Case.EstimatedTimeSpanIsoString).format("HH:mm", { trim: false }).split(":");
            $("#estimatedTimeHour-spinner").val(dur[0]);
            $("#estimatedTimeMinute-spinner").val(dur[1]);
                
            /*"PercentDone":*/
            //$('#g1').slider().slider("setValue", data.Case.PercentDone);


            $("#createModal").modal("show");
        },
        error: function () {
            toastr.error("Could not fetch case");
        }
    });
});
$(document).off("click", "#yearOutBtn");
$(document).on("click","#yearOutBtn",function () {
        $("#caseDeadlineDate").data("DateTimePicker").date(moment("31-12", "DD-MM"));
    });

//#endregion
    //===============Case END================//

    //============Case Assignments==========//
    //#region Case Assignments
    //Assignment open close
    $(document).off("click", ".Assignment");
    $(document).on("click",".Assignment",function() {
            const tr = $(this).parent()[0];
        const id = $(tr).attr("data-assignmentId");
            $(tr).toggleClass("shown");
            if ($(tr).hasClass("shown")) {
                $(`#Assignment${id}`).show();
            } else {
                $(`#Assignment${id}`).hide();
            }
        });

    //Assignment modal open, load and INIT
    $(document).off("click", ".assignmentsBtn");
    $(document).on("click",".assignmentsBtn",function() {
            
            const caseId = $(this).attr("data-case-id");
            $("#AssignmentModal").modal("show");
            $("#AssignmentModal").attr("data-caseId", $(this).attr("data-case-id"));
            $("#assignmentEstimatedHour-spinner").spinner();
            $("#assignmentEstimatedMinute-spinner").spinner();
            $("#assignmentEstimatedHour-spinner").spinner({
                step: 1.0,
                numberFormat: "n",
                min: 0
            });
            $("#assignmentEstimatedMinute-spinner").spinner({
                step: 5.0,
                numberFormat: "n",
                min: 0,
                max: 59
            });
            $("#assignmentEstimatedMinute-spinner").parent().css("width", "46%");
            $("#assignmentEstimatedHour-spinner").parent().css("width", "46%");
            clearAssignmentModal();
            $("#assignmentResponsible").empty();
            $("#otherAssignment").empty();


        $("#assignmentTableReload").attr("data-caseId", caseId);
            getAssignmentsTable(caseId);
        getUsersNames($("#assignmentResponsible"));
        });


    function getUsersNames($el) {
        $.ajax({
            url: "/CustomerCases/GetUsersNames",
            data: {},
            success: function(data) {
                $.each(data.Users,
                    function(index, item) {
                        var lastName = "";
                        if (item.LastName === null) {
                            console.log("null lastname");
                        }
                        if (item.LastName != null) {
                            lastName = item.LastName;
                        };

                        $($el)
                            .append(`<option value=${item.Id}>${item.FirstName} ${lastName}</option>`);
                    });
            },
            error: function(error) {
            }
        });
    }

    function clearAssignmentModal() {

        $("#assignmentTitle").val("");
        $("#assignmentDescription").val("");
        $("#assignmentStart").val("");
        $("#otherAssignment").empty();
    }

    //Saves Assignment
    $("#saveAssignmentBtn").click(function() {
        var $btn = $(this).button("loading");
        var caseId = $("#AssignmentModal").attr("data-caseId");
        var estimatedTimeSpan = moment
            .duration(`PT${parseInt($("#assignmentEstimatedHour-spinner").val())}H${parseInt($("#assignmentEstimatedMinute-spinner").val())}M`).toISOString();
        const assignment = {
            Id: null,
            StartDateTime: $("#assignmentStart").data("DateTimePicker").date().toISOString(),
            Deadline: $("#assignmentDeadlineDate").data("DateTimePicker").date().toISOString(),
            CustomerCaseId: caseId,
            LinkedCaseAssignmentId: $("#otherAssignment").val(),
            Description: $("#assignmentDescription").val(),
            Title: $("#assignmentTitle").val(),
            AddToCaseEstimate: $("#addToEstimate").prop("checked"),
            UserId: $("#assignmentResponsible").selected().val(),
            Urgent: $("#urgent").prop("checked")
        };;
        $.ajax({
            url: "/CustomerCases/CreateAssignment",
            type: "Post",
            data: { 'assignment': assignment, "EstimatedTimeSpan": estimatedTimeSpan },
            success: function(data) {
                $btn.button("reset");

                if ($("#addToEstimate").prop("checked")) {
                    const oldTimeEstimate = moment.duration($(`#${caseId}-EstimatedTimeSpan`).text());
                    const newTimeEstimate = oldTimeEstimate.add(moment.duration(estimatedTimeSpan));
                    $(`#${caseId}-EstimatedTimeSpan`)
                        .text(`${moment.duration(newTimeEstimate).format("HH:mm", { trim: false })}`);
                }
                toastr.success("Saved");

                clearAssignmentModal();

            },
            error: function() {
                $btn.button("reset");
                toastr.error("Saving failed");
            }
        });
});

$("#assignmentTableReload").click(function() {
    let id = parseInt($("#assignmentTableReload").attr("data-caseId"));
    getAssignmentsTable(id);
});
$("#assignmentDoneTableReload").click(function () {
    let id = parseInt($("#assignmentTableReload").attr("data-caseId"));
    getAssignmentsTable(id);
});
    // Gets and makes tables in the modal.
    function getAssignmentsTable(caseId) {
        $("#AssignmentsTBody").empty();
        $("#AssignmentsDoneTBody").empty();

        $.ajax({
            url: "/CustomerCases/GetCaseAssignments",
            data: { 'caseId': caseId },
            dataType: "json",
            success: function(data) {
                $("#otherAssignment").append(`<option value="null">None</option>`);
                $.each(data.AssignmentsList,
                    function(index, item) {
                        $("#otherAssignment").append(`<option value=${item.Id}>${item.Title}</option>`);
                    });
                //Active Assignment List
                $.each(data.AssignmentsList.filter(x => x.Done === null || x.Done === false),
                    function(i, item) {
                        
                        var linkedTitle = "None";
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
                                                        <td>Linked Assigntment:</td>
                                                        <td data-value="${item.LinkedCaseAssignmentId}" style="max-width:150px; word-wrap:break-word" id="${item.Id}-linkedAssignment" data-editable-${item.Id}>${linkedTitle}  </td>
                                                    </tr>
                                                    <tr>
                                                        <td>Description:</td>
                                                        <td style="max-width:150px; word-wrap:break-word" id="AssignmentDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                                    </tr>
                                                    <tr>
                                                        <td id="${item.Id}-assignmentsActions">
                                                            <button id="${item.Id}-endAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id} "data-case-id="${caseId}" class="btn btn-xs endAssignmentBtn btn-warning pull-left">End</button>
                                                            <button id="${item.Id}-editAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id}" class="btn btn-xs editAssignmentBtn btn-info pull-left">Edit</button>
                                                            <button id="${item.Id}-DeleteBtn" data-Assignment-id="${item.Id}" class="btn btn-xs deleteAssignmentBtn btn-danger pull-left">Delete</button>
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
                    function(i, item) {
                        var endDate;
                        if (item.EndDateTime === null) {
                            endDate = "Not Done";
                        } else {
                            endDate = moment(
                                    new Date(parseInt((item.EndDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm");
                        }
                        var linkedTitle = "None";
                        if (item.LinkedCaseAssignmentId != null) {
                            linkedTitle =
                                (data.AssignmentsList.find(x => item.LinkedCaseAssignmentId === x.Id)).Title;
                        };
                        $("#assignmentsDoneTable").append(
                            `<tr id="${item.Id}-assignment" data-plusesti="${item.AddToCaseEstimate}" data-caseid="${item.CustomerCaseId}" data-ended="true" class="AssignmentRemove${item.Id} even expandCollapseDetails">
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
                                                        <td>Deadline:</td>
                                                        <td id="AssignmentDateTime-Deadline-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.Deadline).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                                    </tr>
                                                    <tr>
                                                        <td>Linked Assigntment:</td>
                                                        <td data-value="${item.LinkedCaseAssignmentId}" style="max-width:150px; word-wrap:break-word" id="${item.Id}-linkedAssignment" data-editable-${item.Id}>${linkedTitle}  </td>
                                                    </tr>
                                                    <tr>
                                                        <td>Description:</td>
                                                        <td style="max-width:150px; word-wrap:break-word" id="AssignmentDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                                    </tr>
                                                    <tr>
                                                        <td id="${item.Id}-assignmentsActions">
                                                            <button id="${item.Id}-endAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id} "data-case-id="${caseId}" class="btn btn-xs endAssignmentBtn btn-warning pull-left">End</button>
                                                            <button id="${item.Id}-editAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${item.Id}" class="btn btn-xs editAssignmentBtn btn-info pull-left">Edit</button>
                                                            <button id="${item.Id}-DeleteBtn" data-Assignment-id="${item.Id}" class="btn btn-xs deleteAssignmentBtn btn-danger pull-left">Delete</button>
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
            error: function() {
                toastr.error("Could not fetch Time Registration");
            }
        });
    }

        //ends assignment
$(document).off("click", ".endAssignmentBtn");
    $(document).on("click",".endAssignmentBtn",function() {
            const now = moment();
            const id = parseInt($(this).attr("data-Assignment-id"));
            const caseId = parseInt($(this).attr("data-case-id"));

            $.ajax({
                url: "/CustomerCases/EndCaseAssignment",
                data: { AssignmentId: id, EndDateTime: now.toISOString() },
                success: function(data) {
                    getAssignmentsTable(caseId);
                },
                error: function(data) {
                    toastr.error("Ending Failed failed");
                }
            });
        });

    //restarts assignment
$(document).off("click", ".restartAssignmentBtn");
    $(document).on("click",".restartAssignmentBtn",function() {
            const id = parseInt($(this).attr("data-Assignment-id"));
            const caseId = parseInt($(this).attr("data-case-id"));

            $.ajax({
                url: "/CustomerCases/RestartCaseAssignment",
                data: { AssignmentId: id },
                success: function(data) {
                    getAssignmentsTable(caseId);
                },
                error: function(data) {
                    toastr.error("Restarting Failed failed");

                }
            });
        });

    //Deletes assignment
$(document).off("click", ".deleteAssignmentBtn");
    $(document).on("click",".deleteAssignmentBtn",function() {
            var AssignmentId = parseInt($(this).attr("data-Assignment-id"));
            $.SmartMessageBox({
                    title: "Delete?",
                    content: "Are you sure you want to Delete this time registration",
                    buttons: "[No][Yes]"
                },
                function(ButtonPressed) {
                    if (ButtonPressed === "Yes") {

                        $.ajax({
                            url: "/CustomerCases/DeleteCaseAssignment",
                            data: { AssignmentId: AssignmentId },
                            success: function(data) {
                                //getAssignmenttable($("#AssignmentModal").attr("data-caseId"));
                                $(`.AssignmentRemove${AssignmentId}`).fadeOut(300, function() { $(this).remove(); });
                            },
                            error: function(data) {
                                toastr.error("Delete failed");

                            }
                        });
                    }
                    if (ButtonPressed === "No") {
                        $.smallBox({
                            iconSmall: "fa fa-times fa-2x fadeInRight animated",
                            timeout: 4000
                        });
                    }
                }
            );
        });

//Makes it so the user can edit an assignment
$(document).off("click", ".editAssignmentBtn");
$(document).on("click",".editAssignmentBtn",function() {
        var assignmentId = parseInt($(this).attr("data-Assignment-id"));
        var $el = $(`[data-editable-${assignmentId}]`);
        $(this).replaceWith(`<button id="${assignmentId}-saveAssignmentBtn" style="margin-right: 10px" data-Assignment-id="${
            assignmentId}" class="btn btn-xs editSaveAssignmentBtn btn-success pull-left">Save</button>
                 <button id="${assignmentId}-cancelAssignmentBtn" data-Assignment-id="${assignmentId
            }" style="margin-right: 10px" class="btn btn-xs cancelAssignmentBtn btn-normal pull-left">Cancel</button>`);

        $.each($el,
            function(i, item) {
                const $width = $($el[i]).width();
                const $id = $($el[i]).prop("id");
                if ($id === `${assignmentId}-linkedAssignment`) {
                    let val = parseInt($(`#${assignmentId}-linkedAssignment`).data("value"));
                    $(item).attr(`data-saveable-${assignmentId}`, "");
                    $(item).removeAttr(`data-editable-${assignmentId}`);
                    const content = $("#otherAssignment").clone();
                    content.prop("id", `${assignmentId}-linkedDropdown`);
                    $(`#${assignmentId}-linkedAssignment`).empty();
                    $(`#${assignmentId}-linkedAssignment`).append(content);
                    $(`#${assignmentId}-linkedDropdown`).val(val);
                } else if ($id === `${assignmentId}-responsible`) {
                    let val = parseInt($(`#${assignmentId}-linkedAssignment`).data("value"));

                    $(item).attr(`data-saveable-${assignmentId}`, "");
                    $(item).removeAttr(`data-editable-${assignmentId}`);
                    const content = $("#assignmentResponsible").clone();
                    $(`#${assignmentId}-responsible`).empty();
                    $(`#${assignmentId}-responsible`).append(content);
                    $(`#${assignmentId}-linkedDropdown`).val(val);
                } else if ($id === `${assignmentId}-interval`) {

                    $(item).attr(`data-saveable-${assignmentId}`, "");
                    $(item).removeAttr(`data-editable-${assignmentId}`);
                    const oldContent = $(`#${assignmentId}-interval`).text().split(":");
                    const content =
                        `<input class=" col-md-2 form-control spinner-left" id="${assignmentId
                            }-EditEstimatedHour-spinner" name="spinner" value="1"> <text> : </text>
                                        <input class="col-md-2 form-control spinner" id="${assignmentId
                            }-EditEstimatedMinute-spinner" name="spinner" value="0">`;
                    $(`#${assignmentId}-interval`).empty();
                    $(`#${assignmentId}-interval`).append(content);

                    $(`#${assignmentId}-EditEstimatedHour-spinner`).spinner({
                        step: 1.0,
                        numberFormat: "n",
                        min: 0
                    });
                    $(`#${assignmentId}-EditEstimatedMinute-spinner`).spinner({
                        step: 5.0,
                        numberFormat: "n",
                        min: 0,
                        max: 59
                    });
                    $(`#${assignmentId}-EditEstimatedMinute-spinner`).parent().css("width", "58px");
                    $(`#${assignmentId}-EditEstimatedHour-spinner`).parent().css("width", "58px");

                    $(`#${assignmentId}-EditEstimatedMinute-spinner`).val(oldContent[1]);
                    $(`#${assignmentId}-EditEstimatedHour-spinner`).val(oldContent[0]);
                } else {
                    const $td = $(`<td style='width: ${$width}px'> </td>`);
                    const $input =
                        $(`<input class="datepicker form-control" id="${$id}" data-saveable-${assignmentId
                            } style="width:${$width}px"/>`).val($(item).text());

                    $($td).append($input);
                    $(item).replaceWith($td);
                }
            });

        $(`#AssignmentDateTime-start-${assignmentId}`).datetimepicker({
            locale: "da",
            sideBySide: true
        });
        $(`#AssignmentDateTime-Deadline-${assignmentId}`).datetimepicker({
            locale: "da",
            sideBySide: true
        });
        if ($(`#${assignmentId}-assignment`).data("ended")) {
           $(`#AssignmentDateTime-Ended-${assignmentId}`).datetimepicker({
                locale: "da",
                sideBySide: true
            });
            }
        });

function assignmentTrCancel($el, assignmentId) {
    $(`#${assignmentId}-cancelAssignmentBtn`).replaceWith(
        `<button id="${assignmentId}-editAssignmentBtn" data-Assignment-id="${assignmentId
        }" class="btn btn-xs editAssignmentBtn btn-info pull-left">Edit</button> `);
    $(`#${assignmentId}-saveAssignmentBtn`).remove();
    $.each($el,function(i, item) {
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
        }
    );
};

// returns the assignment to normal
$(document).off("click", ".cancelAssignmentBtn");
    $(document).on("click", ".cancelAssignmentBtn", function () {
            var AssignmentId = parseInt($(this).attr("data-Assignment-id"));
            var $el = $(`[data-saveable-${AssignmentId}]`);


        assignmentTrCancel($el, AssignmentId);

        $.ajax({
            url: "/CustomerCases/GetAssignment",
            data: { "AssignmentId": AssignmentId },
            success: function (data) {
                $(`#timeregTitle-${AssignmentId}`).text(data.Title);
                $(`#timeregDateTime-start-${AssignmentId}`).text(moment(new Date(parseInt((data.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm"));
                $(`#timeregDateTime-end-${AssignmentId}`).text(moment(new Date(parseInt((data.EndDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm"));
                $(`#${AssignmentId}-assignment`).text(data.CaseAssignment.Title);
                $(`#timeregDescription-${AssignmentId}`).text(data.Description);
            },
            error: function (data) {
                toastr.error("Error");
            }
        });
    });

    // Saves the edited assignment
$(document).off("click", ".editSaveAssignmentBtn");
$(document).on("click", ".editSaveAssignmentBtn", function () {
    var assignmentId = parseInt($(this).attr("data-Assignment-id"));
    var $el = $(`[data-saveable-${assignmentId}]`);
    var endDate;
    if ($(`#${assignmentId}-assignment`).data("ended")) {
        endDate = $(`#AssignmentDateTime-Ended-${assignmentId}`).data("DateTimePicker").date().toISOString();
    }
    const Assignment = {
                "Id": assignmentId,
                "Title": $(`#AssignmentTitle-${assignmentId}`).val(),
                "UserId": $(`#${assignmentId}-responsible`).children().find(":selected").val() ,
                "StartDateTime": $(`#AssignmentDateTime-start-${assignmentId}`).data("DateTimePicker").date().toISOString(),
                "Deadline": $(`#AssignmentDateTime-Deadline-${assignmentId}`).data("DateTimePicker").date().toISOString(),
                "LinkedCaseAssignmentId": parseInt($(`#${assignmentId}-linkedAssignment`).children().find(":selected").val()),
                "EstimatedTimeIsoString": moment.duration({ minutes: $(`#${assignmentId}-EditEstimatedMinute-spinner`).val(), hours: $(`#${assignmentId}-EditEstimatedHour-spinner`).val()}).toISOString(),
                "Description": $(`#AssignmentDescription-${assignmentId}`).val(),
                "EndDateTime": endDate
            };
    $.ajax({
                url: "/CustomerCases/AssignmentEdit",
                type: "Post",
                data: { "Assignment": Assignment },
                success: function(data) {
                    toastr.success("Success");
                    assignmentTrCancel($el, assignmentId);

                    if ($(`#${assignmentId}-assignment`).data("plusesti")) {
                        var caseId = $(`#${assignmentId}-assignment`).data("caseid");
                        $(`#${caseId}-EstimatedTimeSpan`)
                            .text(`${moment.duration(data.newCaseEstimate).format("HH:mm", { trim: false })}`);
                    }
                },
                error: function(data) {
                    toastr.error("Edit failed");

                }
            });

        });
    //#endregion
    //============Case Assignments END=======//

    //===============Time Reg===============//
    //#region TimeReg
    //Open and closes TimeReg
$(document).off("click", ".TimeReg");
    $(document).on("click", ".TimeReg", function() {
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
    $(document).on("click",".timeRegBtn",function() {
            const caseId = $(this).attr("data-case-id");
            $("#TimeRegModal").modal("show");
            $("#TimeRegModal").attr("data-caseId", $(this).attr("data-case-id"));
        let id = parseInt($(this).attr("data-case-id"));
        getTimeRegtable(id);
        $("#timeregTableReload").attr("data-caseId", id);
        getAssignmentsDropdown(caseId, $("#timeRegAssignment"));
        });
$(document).off("click", "#timeregTableReload");
$(document).on("click", "#timeregTableReload", function () { getTimeRegtable(parseInt($("#timeregTableReload").attr("data-caseId")));});

    function getTimeRegtable(caseId) {
        $("#timeRegTBody").empty();
        $.ajax({
            url: "/CustomerCases/GetCaseTimeRegs",
            data: { 'caseId': caseId },
            dataType: "json",
            success: function(data) {
                $.each(data.TimeRegsList,
                    function(i, item) {
                        var caseAssignmentTitle = "None";
                        if (item.CaseAssignment != null) {
                            caseAssignmentTitle = item.CaseAssignment.Title;
                        };
                        $("#timeRegTable").append(
                            `<tr data-caseid="${item.Id}" @*role="row"*@ class="TimeRegRemove${item.Id} even expandCollapseDetails">
                                <td style="max-width:150px; word-wrap:break-word" id="timeregTitle-${item.Id}" data-editable-${item.Id} style="display: block;">${item.Title}</td>
                                <td id="timeregDateTime-start-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.StartDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                <td id="timeregDateTime-end-${item.Id}" data-editable-${item.Id}>${moment(new Date(parseInt((item.EndDateTime).replace(/\/Date\((-?\d+)\)\//, "$1")))).format("DD.MM.YYYY HH:mm")}</td>
                                <td>${item.User.FirstName} ${item.User.LastName}</td> 
                                <td id="${item.Id}-interval">${moment.duration(item.Interval).format("hh:mm", { trim: false })}</td>
                                <td style="width: 15px" class="details-control TimeReg"></td>

                                <tr class="TimeReg TimeRegRemove${item.Id}" id="timeReg${item.Id}" style="display: none"><td colspan="6">
                                    <table cellpadding="5" cellspacing="0" border="0" class="table table-hover table-condensed">
                                        <tbody>
                                            <tr>
                                                <td>Assignment:</td>
                                                <td style="max-width:150px; word-wrap:break-word" id="${item.Id}-assignment" data-editable-${item.Id}>${caseAssignmentTitle}</td>
                                            </tr>
                                            <tr>
                                                <td>Description:</td><td style="max-width:150px; word-wrap:break-word" id="timeregDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                            </tr>
                                            <tr>
                                                <td id="${item.Id}-actions">
                                                    <button id="${item.Id}-editTimeRegBtn" style="margin-right: 10px" data-timeReg-id="${item.Id}" class="btn btn-xs editTimeRegBtn btn-info pull-left">Edit</button>
                                                    <button id="${item.Id}-DeleteBtn" data-timeReg-id="${item.Id}" class="btn btn-xs deleteTimeRegBtn btn-danger pull-left">Delete</button>
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
            error: function() {
                toastr.error("Could not fetch Time Registration");
            }
        });
    }

    //saves new Timereg
    $("#saveTimeRegBtn").click(function() {
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
            success: function(data) {

                const oldTimeEstimate = moment.duration($(`#${caseId}-TotalTimeUsed`).text());
                const newTimeEstimate = oldTimeEstimate.add(moment.duration(moment(endTime).diff(moment(startTime))));
                $(`#${caseId}-TotalTimeUsed`).text(`${moment.duration(newTimeEstimate).format("HH:mm", { trim: false })}`);

                $btn.button("reset");

                toastr.success("Saved");
                getTimeRegtable($("#TimeRegModal").attr("data-caseId"));
                clearTimeRegModal();

            },
            error: function() {
                $btn.button("reset");
                toastr.error("Saving failed");
            }
        });
    });

    //clears modal when closed
    $("#TimeRegModal").on("hidden.bs.modal", function() {
            $("#TimeRegModal").attr("data-caseId", "");
            clearTimeRegModal();
    });

    // Timereg Are you sure you want to close?
    $("#TimeRegModal").on("hide.bs.modal", function (e) {
        if (!jQuery.isEmptyObject($("#timeRegTitle").val()) ||
        !jQuery.isEmptyObject($("#timeRegDescription").val())) {

            if (!confirm("Are you sure, you want to close?")) return false;
            };
});

    function clearTimeRegModal() {

        $("#timeRegTitle").val("");
        $("#timeRegDescription").val("");
        $("#timeRegStart").val("");
        $("#timeRegEnd").val("");
        $("#timeRegAssignment").empty();
        $("#timeRegTBody").empty();
    }

    // Deletes a timereg
$(document).off("click", ".deleteTimeRegBtn");
    $(document).on("click",".deleteTimeRegBtn",
        function() {
            var timeRegId = parseInt($(this).attr("data-timeReg-id"));
            var caseId = $("#TimeRegModal").attr("data-caseId");
            var oldTimeEstimate = moment.duration($(`#${caseId}-TotalTimeUsed`).text());
            $.SmartMessageBox({
                    title: "Delete?",
                    content: "Are you sure you want to Delete this time registration",
                    buttons: "[No][Yes]"
                },
                function(ButtonPressed) {
                    if (ButtonPressed === "Yes") {

                        $.ajax({
                            url: "/CustomerCases/DeleteTimeReg",
                            data: { timeRegId: timeRegId },
                            success: function(data) {
                                $(`.TimeRegRemove${timeRegId}`).fadeOut(300, function() { $(this).remove(); });

                                const newTimeEstimate =
                                    oldTimeEstimate.subtract(moment.duration($(`#${timeRegId}-interval`).text()));
                                $(`#${caseId}-TotalTimeUsed`)
                                    .text(`${moment.duration(newTimeEstimate).format("HH:mm", { trim: false })}`);
                            },
                            error: function(data) {
                                toastr.error("Delete failed");

                            }
                        });
                    }
                    if (ButtonPressed === "No") {
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
    $(document).on("click",".editTimeRegBtn",function() {
            var timeRegId = parseInt($(this).attr("data-timeReg-id"));
            var $el = $(`[data-editable-${timeRegId}]`);
            $(this).replaceWith(
                `<button id="${timeRegId}-saveTimeRegBtn" style="margin-right: 10px" data-timeReg-id="${timeRegId
                }" class="btn btn-xs editSaveTimeRegBtn btn-success pull-left">Save</button>
                <button id="${timeRegId}-cancelTimeRegBtn" data-timeReg-id="${timeRegId}" style="margin-right: 10px" class="btn btn-xs cancelTimeRegBtn btn-normal pull-left">Cancel</button>`);
            $.each($el,
                function(i, item) {
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
    $(document).on("click",".cancelTimeRegBtn",function() {
            var timeRegId = parseInt($(this).attr("data-timeReg-id"));
            var $el = $(`[data-saveable-${timeRegId}]`);
            $(this).replaceWith(
                `<button id="${timeRegId}-editTimeRegBtn" data-timeReg-id="${timeRegId
                }" class="btn btn-xs editTimeRegBtn btn-info pull-left">Edit</button> `);
            $(`#${timeRegId}-saveTimeRegBtn`).remove();

            $.each($el,
                function(i, item) {
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
                    $(`#timeregTitle-${timeRegId}`).text(data.Title);
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
    $(document).on("click",".editSaveTimeRegBtn",
        function() {
            var timeRegId = parseInt($(this).attr("data-timeReg-id"));
            var $el = $(`[data-saveable-${timeRegId}]`);
            $(this).replaceWith(`<button id="${timeRegId}-editTimeRegBtn" data-timeReg-id="${timeRegId
                }" class="btn btn-xs editTimeRegBtn btn-info pull-left">Edit</button> `);
            $(`#${timeRegId}-cancelTimeRegBtn`).remove();

            const timeStart = $(`#timeregDateTime-start-${timeRegId}`).data("DateTimePicker").date().toISOString();
            const timeEnd = $(`#timeregDateTime-end-${timeRegId}`).data("DateTimePicker").date().toISOString();
            const assignmentId = $(`#${timeRegId}-assignment`).children().find(":selected").val();
            $(`#${timeRegId}-interval`).text(moment
                .utc(moment.duration(moment(timeEnd).diff(moment(timeStart))).asMilliseconds()).format("HH:mm"));
            $.each($el,
                function(i, item) {
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
                "Title": $(`#timeregTitle-${timeRegId}`).text(),
                "Description": $(`#timeregDescription-${timeRegId}`).text(),
                "CaseAssignmentId": parseInt(assignmentId),
                "StartDateTime": timeStart,
                "EndDateTime": timeEnd,
            };
            $.ajax({
                url: "/CustomerCases/TimeRegEdit",
                type: "Post",
                data: { "timeReg": timeReg },
                success: function(data) {
                    toastr.success("Success");
                },
                error: function(data) {
                    toastr.error("Edit failed");

                }
            });

        });
    //#endregion
    //===============TimeReg End============//

    //===============Casetype=================//
    //#region CaseType
$(document).off("click", ".CaseType");
$(document).on("click", ".CaseType", function () {
    const tr = $(this).parent()[0];
    const id = $(tr).attr("data-CaseTypeId");
    $(tr).toggleClass("shown");
    if ($(tr).hasClass("shown")) {
        $(`#CaseType${id}`).show();
    } else {
        $(`#CaseType${id}`).hide();
    }
});

$("#createCaseTypeButton").click(function() {
    $("#caseTypeModal").modal("show");
    getUsersNames($("#repeatPlanner"));
    getCaseTypeTable();
});

$("#repeats").click( function () {
    if ($("#repeats").prop("checked")) {
        $("#repeatDiv").fadeIn(200,
            function() {
                $("#repeatDiv").css("display", "block");
            });
    } else {
        $("#repeatDiv").fadeOut(200, function () { $("#repeatDiv").css("display", "none")});
    }

});

function getCaseTypeTable() {
    $("#caseTypeTBody").empty();

    $.ajax({
        url: "/CustomerCases/GetCaseTypeTable",
        data: {},
        dataType: "json",
        success: function(data) {

            //Active Assignment List
            $.each(data.CaseTypeList,
                function(i, item) {
                    var repeatTR;
                    var invoiced;
                    var repeats;
                    var prettyRepeat;
                    if (item.Invoiced) {
                        invoiced = '<i style= "color:green" class="fa fa-check"></i>';
                    } else {
                        invoiced = '<i style= "color:red" class="fa fa-times"></i>';
                    }
                    if (item.Repeats) {
                        switch (item.RepeatsSpanIsoString) {
                            case"P1D" :
                                prettyRepeat = "Every day";
                            case "P2D":
                                prettyRepeat = "Every 2nd day";
                            case "P7D":
                                prettyRepeat = "Every 7th day";
                            case "P14D":
                                prettyRepeat = "Every 14th day";
                            case "P1M":
                                prettyRepeat = "Every Month";
                            case "P2M":
                                prettyRepeat = "Every 2nd Month";
                            case "P3M":
                                prettyRepeat = "Every 3rd Month";
                            case "P4M":
                                prettyRepeat = "Every 4th Month";
                            case "P6M":
                                prettyRepeat = "Every 6th Month";
                            case "P1Y":
                                prettyRepeat = "Every Year";
                            case "P2Y":
                                prettyRepeat = "Every 2nd Year";
                            case "P3Y":
                                prettyRepeat = "Every 3rd Year";
                        }
                        repeats = '<i style= "color:green" class="fa fa-check"></i>';
                        repeatTR = `<tr>
                                        <td>Who plans next repeat?:</td>
                                        <td data-value="${item.UserId}" style="max-width:150px; word-wrap:break-word" id="${item.Id}-repeatsPlanner" data-editable-${item.Id}>${item.User.FirstName} ${item.User.LastName}</td>
                                    </tr>
                                    <tr>
                                        <td>Repeats:</td>
                                        <td>${prettyRepeat}</td>
                                    </tr>
                                    <tr>
                                        <td><i class="fa fa-bell"></i> days before repeat:</td>
                                        <td>${moment.duration(item.DaysBeforeIsoString).asDays()}</td>
                                    </tr>`;
                    } else {
                        repeats = '<i style= "color:red" class="fa fa-times"></i>';
                    }
                    $("#caseTypeTable").append(
                        `<tr data-caseid="${item.Id}" data-CaseTypeId="${item.Id}" class="CaseTypeRemove${item.Id} even expandCollapseDetails">
                                    <td style="max-width:150px; word-wrap:break-word" id="CaseTypeTitle-${item.Id}" data-editable-${item.Id} style="display: block;">${item.TypeName}</td>
                                    <td data-editable-${item.Id} id="${item.Id}-repeats" data-repeats=${item.Repeats}>${repeats}</td>
                                    <td data-editable-${item.Id} id="${item.Id}-invoiced" data-invoiced=${item.Invoiced}>${invoiced}</td>

                                    <td style="width: 15px"  data-id="CaseType${item.Id}" class="details-control CaseType"></td>

                                    <tr class="CaseType CaseTypeRemove${item.Id}" id="CaseType${item.Id}" style="display: none"><td colspan="6">
                                            <table cellpadding="5" cellspacing="0" border="0" class="table table-hover table-condensed">
                                                <tbody>

                                                    <tr>
                                                        <td>Description:</td>
                                                        <td style="max-width:150px; word-wrap:break-word" id="CaseTypeDescription-${item.Id}" data-editable-${item.Id}>${item.Description}</td>
                                                    </tr>
                                                    ${repeatTR}
                                                    <tr>
                                                        <td id="${item.Id}-CaseTypesActions">
                                                            <!--<button id="${item.Id}-editCaseTypeBtn" style="margin-right: 10px" data-CaseType-id="${item.Id}" class="btn btn-xs editCaseTypeBtn btn-info pull-left">Edit</button>
                                                            <button id="${item.Id}-DeleteBtn" data-CaseType-id="${item.Id}" class="btn btn-xs deleteCaseTypeBtn btn-danger pull-left">Delete</button>-->
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

        },
        error: function() {
            toastr.error("Could not fetch CaseTypes");
        }
    });
}

$("#saveCaseTypeBtn").click(function () {
    var $btn = $(this).button("loading");
    const caseType = {
        "Id": null,
        "TypeName": $("#typeName").val(),
        "Description": $("#CaseTypeDescription").val(),
        "Repeats": $("#repeats").prop("checked"),
        "Invoiced": $("#invoiced").prop("checked"),
        "RepeatsSpanIsoString": $("#repeatDate").val(),
        "UserId": $("#repeatPlanner").val(),
        "DaysBeforeIsoString": $("#planedDaysBefore").val()
    };
    $.ajax({
        url: "/CustomerCases/CreateCaseType",
        data: caseType,
        type: "Post",
        success: function (data) {

            $btn.button("reset");

            toastr.success("Saved");
            getCaseTypeTable();
            clearCaseTypeModal();

        },
        error: function () {
            $btn.button("reset");
            toastr.error("Saving failed");
        }
    });
});

//clears modal when closed
$("#caseTypeModal").on("hidden.bs.modal", function () {
    clearCaseTypeModal();
});

function clearCaseTypeModal() {
    $("#typeName").val("");
    $("#CaseTypeDescription").val("");
    $("#repeats").prop("checked", false);
    $("#invoiced").prop("checked", false);
    $("#repeatDate").empty();
    $("#CaseTypeTBody").empty();
}

//#endregion
    //===============Casetype END=================//

    //===============Generic=================//
    //#region Generic
    function getAssignmentsDropdown(caseId, el) {
        el.append(`<option value=${null}>None</option>`);

        $.ajax({
            url: "/CustomerCases/GetCaseAssignments",
            data: { caseId },
            success: function(data) {


                $.each(data.AssignmentsList,
                    function(index, item) {
                        el.append(`<option value=${item.Id}>${item.Title}</option>`);
                    });
            },
            error: function(error) {
            }
        });
    }

//Table Filter sort pagination

    $(".sort").click(function() {
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
    $("#filterCases").keyup(function() { //TODO: Manipulate desc asc for all the others. And find which to order by.

        clearTimeout($.data(this, "timer"));
        $(this).data("timer", setTimeout(Search(true, null), 500));
});
var pageNumber;
$(document).on("click", ".LiPager", function (e) {
        e.preventDefault();
        pageNumber = $(this).children().attr("data-pagenumber");
        const currentPageNumber = $(this).siblings(".active").children().attr("data-pagenumber");
        if (pageNumber == "Next") {
            pageNumber = String(parseInt(currentPageNumber) + 1);
        } else if (pageNumber == "Previous") {
            pageNumber = String(parseInt(currentPageNumber) - 1);
        }
        

        Search(true, parseInt(pageNumber));
    });

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
        debugger;
        $.ajax({
            url: "/CustomerCases/FilterAndSearch",
            data: { "customerCaseViewModel": searchObject },
            type: "POST",
            success: function(data, textStatus, jqXHR) {
                $("#customerCasesList").html(data.tablePartial);
                $("#customerCasePaginationDiv").html(data.pagingPartial);
                initializeSliders();
            },
            error: function(jqXHR, textStatus, errorThrown) {
                toastr.error("failed");
            }
        });
    }

function reloadMainTable(e) {
    debugger

    
    Search(true, null);
}
//#endregion
    //===============Generic END=============//