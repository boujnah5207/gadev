var expandSearchButton;
var ExpandingSearchBox;

$(function () {
    expandSearchButton = $("#ExpandSearchButton");
    ExpandingSearchBox = $("#ExpandingSearchBox");
    SearchFormContainer = $("#SearchFormContainer");

    expandSearchButton.click(function () {
        if (expandSearchButton.val() == local.Show) {
            expandSearchButton.val(local.Hide)
        }
        else {
            expandSearchButton.val(local.Show)
        }

        if (ExpandingSearchBox.css('display') == "none") {
            ExpandingSearchBox.toggle();
            SearchFormContainer.slideToggle(300);
        }
        else {
            SearchFormContainer.slideToggle(300).promise().done(function () {
                ExpandingSearchBox.toggle();
            });;
        }

        //ExpandingSearchBox.slideToggle(300).promise().done(function () {
        //    SearchFormContainer.toggleClass("hidden");
        //});;
        
    });

});