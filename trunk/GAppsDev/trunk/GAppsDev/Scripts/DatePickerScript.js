
$(function () {
    setAllDatePickers();
});

function setAllDatePickers() {
    var dateFields = $(".dateField");
    for (var i = 0; i < dateFields.length; i++) {
        if (!$(dateFields[i]).hasClass("hasDatepicker")) {
            $(dateFields[i]).datepicker($.datepicker.regional["he"]);
            $(dateFields[i]).datepicker("option", "dateFormat", "dd/mm/yy");
        }
    }
}