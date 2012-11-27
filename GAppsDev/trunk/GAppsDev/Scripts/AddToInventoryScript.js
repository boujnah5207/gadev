﻿var splitedItems;
var removedItems;
var locationList;

$(function () {

    splitedItems = new Array();
    removedItems = new Array();
    locationList = $($(".locationList")[0]).clone();
});

function expand(id, i) {
    var expandingDiv = $("#expandingDiv-" + id + "-" + i);
    var expandingBtn = $("#expandingBtn-" + id + "-" + i);

    expandingDiv.slideToggle(500, null);
    if (expandingBtn.val() == "הצג פרטים") {
        expandingBtn.val("הסתר פרטים");
    }
    else {
        expandingBtn.val("הצג פרטים");
    }
}

function expandGroup(id) {
    var expandingDiv = $("#expandingGroup-" + id);
    var expandingBtn = $("#expandingGroupBtn-" + id);

    expandingDiv.slideToggle(500, null);
    if (expandingBtn.val() == "הצג קבוצה") {
        expandingBtn.val("הסתר קבוצה");
    }
    else {
        expandingBtn.val("הצג קבוצה");
    }
}

function expandRemoved() {
    var expandingDiv = $("#removedItems");
    var expandingBtn = $("#expandRemovedItems");

    expandingDiv.slideToggle(500, null);

    if (expandingBtn.val() == "הצג פריטים שהוסרו") {
        expandingBtn.val("הסתר פריטים שהוסרו");
    }
    else {
        expandingBtn.val("הצג פריטים שהוסרו");
    }
}

function split(id, index, quantity) {
    var oldItem = $("#expandingDiv-" + id + "-" + 0);
    var oldItemTitle = $("#ItemId-" + id).find("legend").html();
    var oldExpandBtn = $("#expandingBtn-" + id + "-" + 0);
    var oldSplitBtn = $("#splitBtn-" + id + "-" + 0);

    for (var i = 0; i < quantity; i++) {
        var newLocationList = locationList.clone();
        newLocationList.attr("id", "#locationList-" + id + "-" + i);
        newLocationList.attr("name", "InventoryItems[" + index + "].ItemsToAdd[" + i + "].LocationId");

        var newItem = $(
            "<fieldset class='originalItem-" + id + "' id='item-" + id + "-" + i + "'>" +
                    "<legend>" + oldItemTitle + " " + (i + 1) + "</legend>" +
                    "<div id='expandingDiv-" + id + "-" + i + "' class='expanding-div' style='display:none;'>" +
                        "<input type='hidden' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].ItemId' value='" + id + "' />" +
                        "<label>מיקום: </label> " +
                        $('<div>').append(newLocationList.clone()).html()  +
                        "<label>משוייך ל: </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].AssignedTo' />" +
                        "<label>מספר סידורי: </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].SerialNumber' />" +
                        "<br /> <span>תקופת אחריות: </span> <br />" +
                        "<label>מ- </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].WarrentyPeriodStart' /> - <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].WarrentyPeriodEnd' />" +
                        "<label>הערות: </label> <textarea name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].Notes' ></textarea>" +
                        "<label>מצב: </label> <input type='text' name='InventoryItems[" + index + "].ItemsToAdd[" + i + "].Status' />" +
                    "</div>" +
                    "<input id='expandingBtn-" + id + "-" + i + "' type='button' value='הצג פרטים' onClick='expand(" + id + "," + i + ")' />" +
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
    oldExpandBtn.replaceWith($("<input id='expandingGroupBtn-" + id + "' type='button' value='הסתר קבוצה' onClick='expandGroup(" + id + ")' />"));
    oldSplitBtn.replaceWith($("<input id='unSplitBtn-" + id + "' type='button' value='בטל פיצול' onClick='unSplit(" + id + ")' />"));
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

    $("#removedItems").append($("<div id='removedItem-" + id + "'><span>" + oldItemTitle + "</span><input type='button' value='החזר פריט לרשימה' onClick='unRemove(" + id + ")' /></div>"));
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