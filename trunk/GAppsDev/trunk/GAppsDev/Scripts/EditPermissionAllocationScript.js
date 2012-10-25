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
    var allocationId = $("#budgetAllocations-" + budgetIndex).val();
    var allocationText = $("#budgetAllocations-" + budgetIndex + " option:selected").text();
    var container = $("#budgetContainer-" + budgetIndex);
    var nextNumber = $(".budget-" + budgetIndex).length;

    var existingItems = $(".existingAllocations");
    console.log(existingItems);
    var itemExists = false;

    for (var i = 0; i < existingItems.length; i++) {
        console.log($(existingItems[i]).val() + "=" + allocationId);
        if ($(existingItems[i]).val() == allocationId) {
            itemExists = true;
        }
    }

    var existingRemovedItem = getRemovedItem(allocationId);
    if (existingRemovedItem == null) {
        if(!itemExists) {
            var newItem = $(
                "<div class='budget-" + budgetIndex + "' id='permissionAllocation-" + budgetIndex + "-" + nextNumber + "'>" +
                            "<input type='hidden' id='isActiveField-" + budgetIndex + "-" + nextNumber + "' name='BudgetAllocationsList[" + budgetIndex + "].PermissionAllocations[" + nextNumber + "].IsActive' value='true' />" +
                            "<input type='hidden' class='existingAllocations' id='allocationField-" + budgetIndex + "-" + nextNumber + "' name='BudgetAllocationsList[" + budgetIndex + "].PermissionAllocations[" + nextNumber + "].Allocation.BudgetsExpensesToIncomeId' value='" + allocationId + "' />" +
                            "<span>" + allocationText + "<\span>" +
                            "<input type='button'  value='הסר' onClick='removeItem(" + budgetIndex + "," + nextNumber + ") '/>" +
                        "</div>"
                );

            container.append(newItem);
        }
    }
    else {
        unRemove(allocationId);
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

function unRemove(allocationId) {
    var existingRemovedItem = getRemovedItem(allocationId);
    if (existingRemovedItem != null) {
        existingRemovedItem.oldItem.toggle(0);

        console.log(removedItems.length);
        var itemIndex = null;
        for (var i = 0; i < removedItems.length; i++) {
            if (removedItems[i] == existingRemovedItem) {
                itemIndex = i;
                break;
            }
        }
        console.log(itemIndex);

        $(removedItems).splice(itemIndex, 1);
        console.log(removedItems);
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
