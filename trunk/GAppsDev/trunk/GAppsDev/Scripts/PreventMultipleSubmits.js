$(function () {
    $('form').submit(function () {
        $(".disableOnSubmit").attr('disabled', "true");
    });
});