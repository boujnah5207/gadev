var submitted;

$(function () {
    submitted = false;

    $('form').submit(function () {

        if (submitted) return false;
        submitted = true;

        //$(".disableOnSubmit").attr('disabled', "true");
    });
});