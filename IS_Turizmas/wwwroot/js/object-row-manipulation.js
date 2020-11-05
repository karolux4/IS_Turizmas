$(document).ready(function () {

    $(document).on('click', '.add-more', function () {
        var $value = $(this).data('value');
        var $hide = $(this).data('hide');
        var $row = $('.' + $value);
        $row.after($row.clone());
        $row.removeClass($value);
        $row.find('.' + $hide).addClass('d-none');
        $('.' + $value).find('input').val('');
    });

    $(document).on('click', '.remove-row', function () {

        var $isLast = false;
        var $value = $(this).data('value');
        var $last = $(this).data('last');
        var $hide = $(this).data('hide');
        if ($('.' + $value).length === 1) {
            $(this).closest('.' + $value).find('input').val('');
        }
        else {
            if ($(this).closest('.' + $value).hasClass($last)) {
                $isLast = true;
            }
            $(this).closest('.' + $value).remove();
            $('.' + $value).last().addClass($last);
            if ($isLast) {
                $('.' + $value).last().find('.' + $hide).removeClass('d-none');
            }
        }
    });
});