$(document).ready(function () {

    $(".js-sendtoemailpopup").click(function (e) {
        e.preventDefault();
        $(".js-sendtoemailbox").toggle();
    });

    $(".js-sendtoemailclosepopup").click(function (e) {
        e.preventDefault();
        $(".js-sendtoemailbox").hide();
    });

});