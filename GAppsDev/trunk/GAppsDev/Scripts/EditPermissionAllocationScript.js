var removedItems;

$(function () {
    removedItems = {};
});

function expandDiv(button, elementId) {
    $("#" + elementId).slideToggle(500, null);
    button = $(button);

    if (button.val() == local.Show) {
        button.val(local.Hide);
    }
    else {
        button.val(local.Show);
    }
}

function addItem(budgetIndex) {
    if ($("#budgetAllocations-" + budgetIndex + " option:selected").length > 0) {
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
            if (!itemExists) {
                var newItem = $(
                    "<div class='budget-" + budgetIndex + "' id='permissionAllocation-" + budgetIndex + "-" + nextNumber + "'>" +
                                "<input type='hidden' class='isActiveField' id='isActiveField-" + budgetIndex + "-" + nextNumber + "' name='BudgetAllocationsList[" + budgetIndex + "].PermissionAllocations[" + nextNumber + "].IsActive' value='true' />" +
                                "<input type='hidden' class='existingAllocations' id='allocationField-" + budgetIndex + "-" + nextNumber + "' name='BudgetAllocationsList[" + budgetIndex + "].PermissionAllocations[" + nextNumber + "].Allocation.BudgetsExpensesToIncomesId' value='" + allocationId + "' />" +
                                "<span>" + allocationText + "<\span>" +
                                "<input type='button'  value='" + local.Delete + "' onClick='removeItem(" + budgetIndex + "," + nextNumber + ") '/>" +
                            "</div>"
                    );

                container.append(newItem);
            }
        }
        else {
            unRemove(allocationId);
        }
    }
}

function removeItem(budgetIndex, allocationIndex) {
    var container = $("#permissionAllocation-" + budgetIndex + "-" + allocationIndex);
    var isActiveField = $("#isActiveField-" + budgetIndex + "-" + allocationIndex);
    var isActive = $("#isActiveField-" + budgetIndex + "-" + allocationIndex).val();
    var BudgetId = $("#BudgetField-" + budgetIndex + "-" + allocationIndex).val();
    var PermissionId = $("#PermissionField-" + budgetIndex + "-" + allocationIndex).val();
    var allocationId = $("#allocationField-" + budgetIndex + "-" + allocationIndex).val();

    isActiveField.val("false");

    /*
    var existingRemovedItem = getRemovedItem(allocationIndex);
    if (existingRemovedItem == null) {
        removedItems[removedItems.length] = {};
        removedItems[removedItems.length].id = allocationId;
        removedItems[removedItems.length].oldItem = container;

        container.toggle(0);
    }
    */

    container.remove();
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
        console.log(existingRemovedItem.oldItem.find(".isActiveField"));
        existingRemovedItem.oldItem.find(".isActiveField").val("true");

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
