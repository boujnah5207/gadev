$(function () {
    $('form').submit(function () {
        $("[type='submit']").attr('disabled', "true");
        return true;
    });
});