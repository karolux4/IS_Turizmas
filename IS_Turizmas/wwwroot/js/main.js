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

    $(document).on('click', '.deleteLink', function () {
        $('.deleteForm').find('#deleteID').val($(this).data('id'));
    });
});