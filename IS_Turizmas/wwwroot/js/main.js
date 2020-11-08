$(document).ready(function () {
    var date_input = $('.input-daterange');
    var options = {
        format: "yyyy-mm-dd",
        todayHighlight: true,
        todayBtn: "linked",
        language: "lt",
        autoclose: true,
    };
    date_input.datepicker(options);
});