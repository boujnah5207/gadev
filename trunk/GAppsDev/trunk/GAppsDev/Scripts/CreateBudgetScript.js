var baseAddIncomeDialog;
var baseAddExpenseDialog;

$(function () {

    baseAddIncomeDialog = $(
            ""
        );
});

function expandDiv(button, elementId) {
    $("#" + elementId).slideToggle(500, null);
    button = $(button);

    if (button.val() == "הצג") {
        button.val("הסתר");
    }
    else {
        button.val("הצג");
    }
}

function addIncome() {

}

function addExpense() {

}