
$(document).ready(function () {
    "use strict";
    debugger;
    var calendarAppointmentsData = $("#appointmentsList").val();
    var calendarLocale = $("#hiddenCurrentCulture").val();


    $('#calendar').fullCalendar({
        locale: calendarLocale,
        editable: true,
        droppable: true,
        firstDay: 1,
        //dayRender: function (date, cell) {
        //    debugger
        //},
        select: function (start, end, allDay) {
            var title = prompt('Event Title:');
            if (title) {
                calendar.fullCalendar('renderEvent', {
                    title: title,
                    start: start,
                    end: end,
                    allDay: allDay
                }, true // make the event "stick"
                );
            }
            calendar.fullCalendar('unselect');
        },

        events: function (start, end, timezone, callback) {
            var events = JSON.parse(calendarAppointmentsData);
            callback(events);
        },

        eventRender: function (event, element, icon) {
            if (!event.description == "") {
                element.find('.fc-event-title').append("<br/><span class='ultra-light'>" + event.description +
                    "</span>");
            }
            if (!event.icon == "") {
                element.find('.fc-event-title').append("<i class='air air-top-right fa " + event.icon +
                    " '></i>");
            }
        },

        windowResize: function (event, ui) {
            $('#calendar').fullCalendar('render');
        },

        dayClick: function (date, jsEvent, view) {
            debugger;
            ClearAppointmentForm();
            $("#customersddlDiv").attr("class", "col-md-12");
            $("#navigateCustomerSection").hide();
            var clickedDate = date.format("DD/MM/YYYY");
            $('#appointmentDate').datepicker("setDate", clickedDate);
            $('#createAppointmentModal').modal('show');
        },

        eventClick: function (calEvent, jsEvent, view) {
            debugger;
            $("#customersddlDiv").attr("class", "col-md-10");
            $("#navigateCustomerSection").show();
            $.ajax({
                url: "/Calendar/EditAppointment",
                data: { "id": calEvent.id },
                type: "POST",
                success: (data, textStatus, jqXHR) => {
                    debugger;
                    $("#hiddenAppointmentId").val(data.Id)
                    $("#hiddenCurrentCustomerId").val(data.SelectedCustomerId);
                    $('#customers').val(data.SelectedCustomerId).trigger('change')
                    $('#customers').attr("disabled", true)
                    $("input[name=eventColor][value=" + data.AppointmentColor + "]").prop('checked', true);
                    $("input[name=eventIcon][value=" + data.AppointmentIcon + "]").prop('checked', true);
                    $(".lblEventColor").removeClass("active");
                    $(".lblEventIcon").removeClass("active");
                    $("input[name=eventColor][value=" + data.AppointmentColor + "]").parent().addClass("active")
                    $("input[name=eventIcon][value=" + data.AppointmentIcon + "]").parent().addClass("active")
                    $('#appointmentDate').datepicker("setDate", new Date(data.Date));
                    $("#appointmentStartTimeH").val(pad(data.StartTimeHour, 2));
                    $("#appointmentStartTimeM").val(pad(data.StartTimeMinute, 2));
                    $("#appointmentEndTimeH").val(pad(data.EndTimeHour, 2));
                    $("#appointmentEndTimeM").val(pad(data.EndTimeMinute, 2));
                    $("#appointmentSubject").val(data.Subject);
                    $("#appointmentDesc").val(data.AppointmentDescription);
                    $("#resendEmailDiv").show();
                    $("#saveAppointmentNoteDiv").hide();
                    $("#hiddenCustomerStatusId").val(data.CustomerStatusId);
                    $("#resendEmail").prop('checked', false);
                    if (data.IsSaveAppointmentNote) {
                        $("#saveAppointmentNote").prop("checked", true);
                        $("#appointmentNote").val(data.AppointmentNote);
                        $("#customerNotesReportId").val(data.CustomerNotesReportId)
                        $("#customerNotesVisitTypeId").val(data.CustomerNotesVisitTypeId)
                        $("#customerNotesDemoId").val(data.CustomerNotesDemoId)
                        $("#noteOptions").show();
                    }
                    if (data.AppointmentCustomerEmails.length > 0) {
                        $.each(data.AppointmentCustomerEmails, function (index, item) {
                            $('#appointmentCustomersMail').tagsinput('add', item);
                        })
                    }
                    if (data.AppointmentSalesPersonEmails.length > 0) {
                        $.each(data.AppointmentSalesPersonEmails, function (index, item) {
                            $('#appointmentSalesPersonMail').tagsinput('add', item);
                        })
                    }

                    $('#createAppointmentModal').modal('show');
                },
                error: (jqXHR, textStatus, errorThrown) => {
                    toastr.error("failed open appointment");
                }
            })
        },

        eventDrop: function (event, delta, revertFunc, jsEvent, ui, view) {
            debugger;
            $.ajax({
                url: "/Calendar/EditAppointmentDate",
                data: { "id": event.id, "date": event.start.format() },
                type: "POST",
                success: (data, textStatus, jqXHR) => {
                    debugger;
                    $('#calendar').fullCalendar('removeEvents');
                    $('#calendar').fullCalendar('addEventSource', data.AppointmentsList);
                    $('#calendar').fullCalendar('rerenderEvents');
                },
                error: (jqXHR, textStatus, errorThrown) => {
                    toastr.error("failed edit appointment");
                    revertFunc();
                }
            })
        }
    });

    $("#saveAppointment").click(function () {
        debugger;
        var $btn = $(this).button('loading')
        if ($("#appointmentForm").valid()) {
            var appointment = {};
            appointment.Id = $("#hiddenAppointmentId").val();
            appointment.SelectedCustomerId = $("#customers").val();
            appointment.AppointmentColor = $('input:radio[name="eventColor"]:checked').val();
            appointment.AppointmentIcon = $('input:radio[name="eventIcon"]:checked').val();
            appointment.Date = $("#appointmentDate").val();
            appointment.StartTimeHour = $("#appointmentStartTimeH").val();
            appointment.StartTimeMinute = $("#appointmentStartTimeM").val();
            appointment.EndTimeHour = $("#appointmentEndTimeH").val();
            appointment.EndTimeMinute = $("#appointmentEndTimeM").val();
            appointment.Subject = $("#appointmentSubject").val();
            appointment.AppointmentDescription = $("#appointmentDesc").val();
            if ($("#saveAppointmentNote").is(":checked")) {
                appointment.CustomerNotesReportId = $("#customerNotesReportId").val();
                appointment.CustomerNotesVisitTypeId = $("#customerNotesVisitTypeId").val();
                appointment.CustomerNotesDemoId = $("#customerNotesDemoId").val();
            }
            appointment.IsSaveAppointmentNote = $("#saveAppointmentNote").prop("checked");
            appointment.AppointmentNote = $("#appointmentNote").val();
            appointment.AppointmentCustomerEmails = $("#appointmentCustomersMail").tagsinput('items');
            appointment.AppointmentSalesPersonEmails = $("#appointmentSalesPersonMail").tagsinput('items');
            appointment.IsResendEmails = $("#resendEmail").prop("checked");
            $.ajax({
                url: "/Calendar/CreateAppointment",
                data: { "customerAppointmentViewModel": appointment },
                type: "POST",
                success: (data, textStatus, jqXHR) => {
                    debugger;
                    $('#createAppointmentModal').modal('hide');
                    $('#calendar').fullCalendar('removeEvents');
                    $('#calendar').fullCalendar('addEventSource', data.AppointmentsList);
                    $('#calendar').fullCalendar('rerenderEvents');
                    $btn.button('reset')
                    toastr.success("Appointment Saved");
                },
                error: (jqXHR, textStatus, errorThrown) => {
                    toastr.error("failed create appointment");
                    $btn.button('reset')
                }
            })
        } else {
            $btn.button('reset')
            return;
        }
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


    /* hide default buttons */
    $('.fc-header-right, .fc-header-center').hide();

    $('#calendar-buttons #btn-prev').click(function () {
        $('.fc-button-prev').click();
        return false;
    });

    $('#calendar-buttons #btn-next').click(function () {
        $('.fc-button-next').click();
        return false;
    });

    $('#calendar-buttons #btn-today').click(function () {
        $('.fc-button-today').click();
        return false;
    });

    $('#mt').click(function () {
        $('#calendar').fullCalendar('changeView', 'month');
    });

    $('#ag').click(function () {
        $('#calendar').fullCalendar('changeView', 'agendaWeek');
    });

    $('#td').click(function () {
        $('#calendar').fullCalendar('changeView', 'agendaDay');
    });

    $('#appointmentDate').datepicker({
        format: 'dd/mm/yyyy'
    });

    $('#customers').select2();
    $("#btnOpenModal").click(function () {
        $("#customersddlDiv").attr("class", "col-md-12");
        $("#navigateCustomerSection").hide();
        $('#customers').attr("disabled", false)
        ClearAppointmentForm();
    });
    function ClearAppointmentForm() {
        $("#hiddenAppointmentId").val("");
        $('#customers').val("").trigger('change');
        $("input[name=eventColor][value=bg-color-darken]").prop('checked', true);
        $("input[name=eventIcon][value=fa-info]").prop('checked', true);
        $(".lblEventColor").removeClass("active");
        $(".lblEventIcon").removeClass("active");
        $("input[name=eventColor][value=bg-color-darken]").parent().addClass("active")
        $("input[name=eventIcon][value=fa-info]").parent().addClass("active")
        $("#appointmentDate").val("");
        $("#appointmentStartTimeH").val("00");
        $("#appointmentStartTimeM").val("00");
        $("#appointmentEndTimeH").val("00");
        $("#appointmentEndTimeM").val("00");
        $("#appointmentSubject").val('');
        $("#appointmentDesc").val('');
        $("#appointmentCustomersMail").tagsinput('removeAll');
        $("#appointmentSalesPersonMail").tagsinput('removeAll');
        $("#appointmentNote").val('');
        $("#saveAppointmentNote").prop('checked', false);
        $("#noteOptions").hide();
        $("#resendEmailDiv").hide();
        $("#saveAppointmentNoteDiv").show()
    }

    $("#navigateToCustomer").click(function () {
        var customerId = $("#hiddenCurrentCustomerId").val();
        if ($("#hiddenCustomerStatusId").val() == 1)
            window.location.href = "Customers/Edit/" + customerId + "?LatestCustomersPageNumber=0&isCustomer=True";
        else if($("#hiddenCustomerStatusId").val() == 2) {
            window.location.href = "Customers/Edit/" + customerId + "?LatestCustomersPageNumber=0&isCustomer=False";
        }
    });

    $("#deleteAppointment").click(function () {
        var hiddenAppointmentId = $("#hiddenAppointmentId").val();
        $.ajax({
            url: "/Calendar/RemoveCalendarAppointment",
            data: { "appointmentId": hiddenAppointmentId },
            success: (data, textStatus, jqXHR) => {
                debugger;
                $("#customersddlDiv").attr("class", "col-md-12");
                $("#navigateCustomerSection").hide();
                $('#calendar').fullCalendar('removeEvents');
                $('#calendar').fullCalendar('addEventSource', data.AppointmentsList);
                $('#calendar').fullCalendar('rerenderEvents');
                $('#createAppointmentModal').modal('hide');
                toastr.success("Appointment Deleted");
            },
            error: (jqXHR, textStatus, errorThrown) => {
                toastr.error("failed delete appointment");
            }
        })
    })

    function pad(num, size) {
        var s = num + "";
        while (s.length < size) s = "0" + s;
        return s;
    }
})
