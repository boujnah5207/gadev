
$(function () {
    setAllDatePickers();
});

function setAllDatePickers() {
    var dateFields = $(".dateField");
    for (var i = 0; i < dateFields.length; i++) {
        var currentItem = $(dateFields[i]);
        if (!currentItem.hasClass("hasDatepicker")) {
            var datepicker_default_val = currentItem.val();
            console.log(datepicker_default_val);
            currentItem.datepicker($.datepicker.regional["he"]);
            currentItem.datepicker("option", "dateFormat", "dd/mm/yy");
            currentItem.val(datepicker_default_val.substring(0, 10));
        }
    }
}