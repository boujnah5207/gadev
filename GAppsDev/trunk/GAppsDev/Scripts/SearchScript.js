var expandSearchButton;
var ExpandingSearchBox;

$(function () {
    expandSearchButton = $("#ExpandSearchButton");
    ExpandingSearchBox = $("#ExpandingSearchBox");

    expandSearchButton.click(function () {
        if (expandSearchButton.val() == local.Show) {
            expandSearchButton.val(local.Hide)
        }
        else {
            expandSearchButton.val(local.Show)
        }

        ExpandingSearchBox.slideToggle(300);
    });

});