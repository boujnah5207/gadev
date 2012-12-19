var splitedItems;
var removedItems;
var locationList;

$(function () {

    splitedItems = new Array();
    removedItems = new Array();
    locationList = $($(".locationList")[0]).clone();

    setAllDatePickers();
});

function expand(id, i) {
    var expandingDiv = $("#expandingDiv-" + id + "-" + i);
    var expandingBtn = $("#expandingBtn-" + id + "-" + i);

    expandingDiv.slideToggle(500, null);
    if (expandingBtn.val() == local.ShowDetails) {
        expandingBtn.val(local.HideDetails);
    }
    else {
        expandingBtn.val(local.ShowDetails);
    }
}

function expandGroup(id) {
    var expandingDiv = $("#expandingGroup-" + id);
    var expandingBtn = $("#expandingGroupBtn-" + id);

    expandingDiv.slideToggle(500, null);
    if (expandingBtn.val() == local.ShowGroup) {
        expandingBtn.val(local.HideGroup);
    }
    else {
        expandingBtn.val(local.ShowGroup);
    }
}

function expandRemoved() {
    var expandingDiv = $("#removedItems");
    var expandingBtn = $("#expandRemovedItems");

    expandingDiv.slideToggle(500, null);

    if (expandingBtn.val() == local.Show) {
        expandingBtn.val(local.Hide);
    }
    else {
        expandingBtn.val(local.Show);
    }
}

function split(id, index, quantity) {
    if (quantity > 50) {
        alert(local.error_cannot_split_over_50);
        return;
    }

    var oldItem = $("#expandingDiv-" + id + "-" + 0);
    var oldItemTitle = $("#ItemName-" + id).val();
    var oldExpandBtn = $("#expandingBtn-" + id + "-" + 0);
    var oldSplitBtn = $("#splitBtn-" + id + "-" + 0);

    console.log("index: " + index);
    var originalLocationId = $("#locationList-" + id + "-" + index + " option:selected").val();

    for (var i = 0; i < quantity; i++) {
        var newLocationList = locationList.clone();
        newLocationList.attr("id", "#locationList-" + id + "-" + i);
        newLocationList.addClass("locationSelectList-" + id);
        newLocationList.attr("name", "InventoryItems[" + index + "].ItemsToAdd[" + i + "].LocationId");
        newLocationList.removeAttr("onChange");

        var newItem = $(
            "<fieldset class='originalItem-" + id + "' id='item-" + id + "-" + i + "'>" +
                    "<legend>" + oldItemTitle + " " + (i + 1) + "</legend>" +
                    "<span>" + local.Location + ": </span>" +
                    $('<div>').append(newLocationList.clone()).html() +
                    "<input id='expandingBtn-" + id + "-" + i + "' type='button' value='" + local.ShowDetails + "' onClick='expand(" + id + "," + i + ")' />" +
                    "<div id='expandingDiv-" + id + "-" + i + "' class='expanding-div' style='display:none;'>" +
                        "<input type='hidden' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].ItemId' value='" + id + "' />" +
                        
                        "<label>" + local.AssignedTo + ": </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].AssignedTo' />" +
                        "<label>" + local.SerialNumber + ": </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].SerialNumber' />" +
                        "<br /> <span class='bold'>" + local.WarrantyPeriod + ": </span> <br />" +
                        "<label style='display:inline;'>" + local.From + "- </label> <input class='dateField' type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].WarrentyPeriodStart' /> <label style='display:inline;'>" + local.To + "</label>- <input class='dateField' type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].WarrentyPeriodEnd' />" +
                        "<label>" + local.State + ": </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].Status' />" +
                        "<label>" + local.Notes + ": </label> <textarea name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].Notes' ></textarea>" +
                    "</div>" +
                "</fieldset>"
            );

        newItem.insertBefore(oldItem);
    }

    var existingSplittedItem = getSplittedItem(id);
    if (existingSplittedItem == null) {
        var newSplittedItem = { id: id, oldItem: oldItem, oldExpandBtn: oldExpandBtn, oldSplitBtn: oldSplitBtn };
        splitedItems[splitedItems.length] = newSplittedItem;
    }
    else {
        existingSplittedItem.item = oldItem;
        existingSplittedItem.oldExpandBtn = oldExpandBtn;
        existingSplittedItem.oldSplitBtn = oldSplitBtn;
    }

    oldItem.remove();
    oldExpandBtn.replaceWith($("<input id='expandingGroupBtn-" + id + "' type='button' value='" + local.HideGroup + "' onClick='expandGroup(" + id + ")' />"));
    oldSplitBtn.replaceWith($("<input id='unSplitBtn-" + id + "' type='button' value='" + local.CancelSplit + "' onClick='unSplit(" + id + ")' />"));

    updateSplittedItems(id, index);
    setAllDatePickers();
}

function unSplit(id) {
    var existingSplittedItem = getSplittedItem(id);
    if (existingSplittedItem != null) {
        var allSplitedItems = $(".originalItem-" + id);
        var groupContainer = $("#expandingGroup-" + id);
        var newExpandBtn = $("#expandingGroupBtn-" + id);
        var newSplitBtn = $("#unSplitBtn-" + id);

        allSplitedItems.remove();
        groupContainer.append(existingSplittedItem.oldItem);

        newExpandBtn.replaceWith(existingSplittedItem.oldExpandBtn);
        newSplitBtn.replaceWith(existingSplittedItem.oldSplitBtn);
    }

    setAllDatePickers();
}

function remove(id) {
    var oldItem = $("#ItemId-" + id);
    var oldItemHiddenCancel = $("#addToInventory-" + id);
    var oldItemTitle = oldItem.find("legend").html();

    var existingRemovedItem = getRemovedItem(id);
    if (existingRemovedItem == null) {
        var newRemovedItem = { id: id, oldItem: oldItem };
        removedItems[removedItems.length] = newRemovedItem;
    }
    else {
        existingRemovedItem.oldItem = oldItem;
    }

    oldItem.toggle(0);
    oldItemHiddenCancel.val("false");

    $("#removedItems").append($("<div id='removedItem-" + id + "'><span>" + oldItemTitle + "</span><input type='button' value='" + local.ReturnItemToList + "' onClick='unRemove(" + id + ")' /></div>"));
}

function unRemove(id) {
    var existingRemovedItem = getRemovedItem(id);
    if (existingRemovedItem != null) {
        var removedItem = $("#removedItem-" + id);
        var ItemHiddenCancel = $("#addToInventory-" + id);
        var itemsContainer = $("#ItemsContainer");

        removedItem.remove();
        ItemHiddenCancel.val("true");
        existingRemovedItem.oldItem.toggle(0);
    }

    setAllDatePickers();
}

function getRemovedItem(id) {
    for (var index in removedItems) {
        if (removedItems[index].id == id) {
            return removedItems[index];
        }
    }

    return null;
}

function getSplittedItem(id) {
    for (var index in splitedItems) {
        if (splitedItems[index].id == id) {
            return splitedItems[index];
        }
    }

    return null;
}

function updateSplittedItems(id, index) {
    console.log(id);
    console.log($("#locationList-" + id + "-" + index + " option:selected"));
    var originalLocationId = $("#locationList-" + id + "-" + index + " option:selected").val();
    console.log(originalLocationId);

    if (originalLocationId == null || originalLocationId == "") return;

    var value = $(this).find("option:selected").val();

    splittedLocations = $(".locationSelectList-" + id).find("option[value=" + originalLocationId + "]").attr('selected', 'selected');
}

function setAllDatePickers() {
    var dateFields = $(".dateField");
    for (var i = 0; i < dateFields.length; i++) {
        if (!$(dateFields[i]).hasClass("hasDatepicker")) {
            $(dateFields[i]).datepicker($.datepicker.regional["he"]);
            $(dateFields[i]).datepicker("option", "dateFormat", "dd/mm/yy");
        }
    }
}