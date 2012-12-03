var removedItems = new Array();

$(function () {

});

function addItem() {
    if ($("#budgetAllocations option:selected").length > 0) {
        var allocationId = $("#budgetAllocations").val();
        var allocationText = $("#budgetAllocations option:selected").text();
        var container = $("#budgetContainer");
        var nextNumber = $(".budget").length;

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
                    "<div class='budget' id='permissionAllocation-" + nextNumber + "'>" +
                                "<input type='hidden' class='isActiveField' id='isActiveField-" + nextNumber + "' name='BudgetAllocations.PermissionAllocations[" + nextNumber + "].IsActive' value='true' />" +
                                "<input type='hidden' class='existingAllocations' id='allocationField-" + nextNumber + "' name='BudgetAllocations.PermissionAllocations[" + nextNumber + "].Allocation.BudgetsExpensesToIncomesId' value='" + allocationId + "' />" +
                                "<span>" + allocationText + "<\span>" +
                                "<input type='button'  value='" + local.Delete + "' onClick='removeItem(" + nextNumber + ") '/>" +
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

function removeItem(allocationIndex) {
    var container = $("#permissionAllocation-" + allocationIndex);
    var isActiveField = $("#isActiveField-" + allocationIndex);
    var isActive = $("#isActiveField-" + allocationIndex).val();
    var BudgetId = $("#BudgetField-" + allocationIndex).val();
    var BasketId = $("#PermissionField-" + allocationIndex).val();
    var allocationId = $("#allocationField-" + allocationIndex).val();

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
