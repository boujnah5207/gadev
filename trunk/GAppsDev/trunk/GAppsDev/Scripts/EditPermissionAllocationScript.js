var removedItems = new Array();

$(function () {

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
        var itemExists = false;

        for (var i = 0; i < existingItems.length; i++) {
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

    var existingRemovedItem = getRemovedItem(allocationIndex);
    if (existingRemovedItem == null) {
        var newRemovedItem = { id: allocationId, oldItem: container};
        removedItems[removedItems.length] = newRemovedItem;

        container.toggle(0);
    }
}

function unRemove(allocationId) {
    var existingRemovedItem = getRemovedItem(allocationId);
    if (existingRemovedItem != null) {
        existingRemovedItem.oldItem.toggle(0);
        existingRemovedItem.oldItem.find(".isActiveField").val("true");

        var itemIndex = null;

        var newRemovedList = new Array();
        
        for (var i = 0; i < removedItems.length; i++) {

            if (removedItems[i] != existingRemovedItem) {
                newRemovedList[newRemovedList.length] = removedItems[i];
            }
        }

        removedItems = newRemovedList;
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
