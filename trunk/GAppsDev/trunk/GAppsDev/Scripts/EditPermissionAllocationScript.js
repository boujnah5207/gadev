var removedItems;

$(function () {
    removedItems = {};
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

function addItem(budgetIndex) {
    var allocationId = $("#budgetAllocationsList-" + budgetIndex).val();
    var container = $("#budgetContainer-" + budgetIndex);
    var nextNumber = $(".budget-" + budgetIndex).length;

    var existingRemovedItem = getRemovedItem(allocationId);
    if (existingRemovedItem == null) {
        var newItem = $(
            "<div class='budget-" + budgetIndex + "' id='permissionAllocation-" + budgetIndex + "-" + nextNumber + "'>" +
                        "<input type='hidden' id='isActiveField-" + budgetIndex + "-" + nextNumber + "' name='BudgetAllocationsList[@i].PermissionAllocations[@i2].IsActive' value='@Model.BudgetAllocationsList[i].PermissionAllocations[i2].IsActive.ToString()' />" +
                        "<input type='hidden' id='allocationField-" + budgetIndex + "-" + nextNumber + "' name='BudgetAllocationsList[@i].PermissionAllocations[@i2].Allocation.BudgetsExpensesToIncomeId' value='@Model.BudgetAllocationsList[i].PermissionAllocations[i2].Allocation.BudgetsExpensesToIncomesId' />" +

                        "<span>@Model.BudgetAllocationsList[i].PermissionAllocations[i2].Allocation.Budgets_ExpensesToIncomes.Amount</span> -" +
                        "<span>@Model.BudgetAllocationsList[i].PermissionAllocations[i2].Allocation.Budgets_ExpensesToIncomes.Budgets_Incomes.CustomName</span> --->" +
                        "<span>@Model.BudgetAllocationsList[i].PermissionAllocations[i2].Allocation.Budgets_ExpensesToIncomes.Budgets_Expenses.CustomName</span>" +
                        
                        "<input type='button'  value='הסר' onClick='removeItem(" + budgetIndex + "," + nextNumber + ") '/>" +
                    "</div>"
            );

        container.append(newItem);
    }
    else {
        unRemove(allocationIndex);
    }
}

function removeItem(budgetIndex, allocationIndex) {
    var container = $("#permissionAllocation-" + budgetIndex + "-" + allocationIndex);
    var isActive = $("#isActiveField-" + budgetIndex + "-" + allocationIndex).val();
    var BudgetId = $("#BudgetField-" + budgetIndex + "-" + allocationIndex).val();
    var PermissionId = $("#PermissionField-" + budgetIndex + "-" + allocationIndex).val();
    var allocationId = $("#allocationField-" + budgetIndex + "-" + allocationIndex).val();

    var existingRemovedItem = getRemovedItem(allocationIndex);
    if (existingRemovedItem == null) {
        removedItems[removedItems.length] = {};
        removedItems[removedItems.length].id = allocationId;
        removedItems[removedItems.length].oldItem = container;

        container.toggle(0);
    }
}

function unRemove(allocationIndex) {
    var existingRemovedItem = getRemovedItem(allocationIndex);
    if (existingRemovedItem != null) {
        existingRemovedItem.oldItem.toggle(0);
    }
}

function getRemovedItem(id) {
    for (var index in removedItems) {
        if (removedItems[index].id == id) {
            return removedItems[index];
        }
    }

    return null;
}
