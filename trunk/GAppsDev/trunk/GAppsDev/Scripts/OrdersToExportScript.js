
$(function () {

    $('.orderCheckbox').attr('checked', false);

    $('#mainForm').submit(function () {
        
        var checkedCheckboxes = $('.orderCheckbox:checked');
        var rows = checkedCheckboxes.parentsUntil("tbody");

        //this.submit();
        //checkedCheckboxes.attr('checked', false);
        //rows.hide(0);

    });

});