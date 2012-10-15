var expandSearchButton;
var ExpandingSearchBox;

$(function () {
    expandSearchButton = $("#ExpandSearchButton");
    ExpandingSearchBox = $("#ExpandingSearchBox");

    expandSearchButton.click(function () {
        if (expandSearchButton.val() == "הצג") {
            expandSearchButton.val("הסתר")
        }
        else {
            expandSearchButton.val("הצג")
        }

        ExpandingSearchBox.slideToggle(300);
    });

});