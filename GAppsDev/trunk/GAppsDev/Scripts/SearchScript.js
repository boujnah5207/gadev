var expandSearchButton;
var ExpandingSearchBox;

$(function () {
    expandSearchButton = $("#ExpandSearchButton");
    ExpandingSearchBox = $("#ExpandingSearchBox");
    SearchFormContainer = $("#SearchFormContainer");

    expandSearchButton.click(function () {
        
        if (ExpandingSearchBox.css('display') == "none") {
            ExpandingSearchBox.toggle();
            SearchFormContainer.slideToggle(300);
        }
        else {
            SearchFormContainer.slideToggle(300).promise().done(function () {
                ExpandingSearchBox.toggle();
            });;
        }

    });

    $("#CreationMax").datepicker($.datepicker.regional["he"]);
    $("#CreationMax").datepicker("option", "dateFormat", "dd/mm/yy");
    $("#CreationMin").datepicker($.datepicker.regional["he"]);
    $("#CreationMin").datepicker("option", "dateFormat", "dd/mm/yy");
});