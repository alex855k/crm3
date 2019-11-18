$(document).ready(function () {
    $('#dailyReportDatePicker').datepicker({
        format: 'dd/mm/yyyy'
    });

    $('#dailyReportDatePicker').change(function () {

        jumpDailyReport($(this).val(), false)
    });

    $('#previousDailyReport').click(function () {
        jumpDailyReport($('#dailyReportDate').text(), true)
    });

    function jumpDailyReport(date, prevDate) {
        debugger;
       location.href = "/DailyReport/TodayNotesReportByDate/?date=" + date + "&prevDate=" + prevDate + ""
    }
});
