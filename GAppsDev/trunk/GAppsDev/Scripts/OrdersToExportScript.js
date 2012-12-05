
$(function () {

    $('.orderCheckbox').attr('checked', false);

    $('#mainForm').submit(function () {
        
        var checkedCheckboxes = $('.orderCheckbox:checked');
        var rows = checkedCheckboxes.parentsUntil("tbody");

        console.log(checkedCheckboxes);
        console.log(rows);

        //this.submit();
        //checkedCheckboxes.attr('checked', false);
        //rows.hide(0);

    });

});