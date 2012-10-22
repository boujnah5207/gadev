var splitedItems;
var locationList;

$(function () {

    splitedItems = new Array();
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

function split(id, index, quantity) {
    var oldItem = $("#expandingDiv-" + id + "-" + 0);
    var oldItemTitle = $("#ItemId-" + id).find("legend").html();
    var oldExpandBtn = $("#expandingBtn-" + id + "-" + 0);
    var oldSplitBtn = $("#splitBtn-" + id + "-" + 0);

    for (var i = 0; i < quantity; i++) {
        var newLocationList = locationList.clone();
        newLocationList.attr("id", "#locationList-" + id + "-" + i);
        newLocationList.attr("name", "InventoryItems[" + index + "][" + i + "].LocationId");

        var newItem = $(
            "<fieldset class='originalItem-" + id + "' id='item-" + id + "-" + i + "'>" +
                    "<legend>" + oldItemTitle + " " + (i + 1) + "</legend>" +
                    "<div id='expandingDiv-" + id + "-" + i + "' class='expanding-div' style='display:none;'>" +
                        "<input type='hidden' name='InventoryItems[" + index + "][" + i + "].ItemId' value='" + id + "' />" +
                        "<label>מיקום: </label> " +
                        $('<div>').append(newLocationList.clone()).html()  +
                        "<label>משוייך ל: </label> <input type='text' name='InventoryItems[" + index + "][" + i + "].AssignedTo' />" +
                        "<label>מספר סידורי: </label> <input type='text' name='InventoryItems[" + index + "][" + i + "].SerialNumber' />" +
                        "<br /> <span>תקופת אחריות: </span> <br />" +
                        "<label>מ- </label> <input type='text' name='InventoryItems[" + index + "][" + i + "].WarrentyPeriodStart' /> - <input type='text' name='InventoryItems[" + index + "][" + i + "].WarrentyPeriodEnd' />" +
                        "<label>הערות: </label> <textarea name='InventoryItems[" + index + "][" + i + "].Notes' ></textarea>" +
                        "<label>מצב: </label> <input type='text' name='InventoryItems[" + index + "][" + i + "].Status' />" +
                    "</div>" +
                    "<input id='expandingBtn-" + id + "-" + i + "' type='button' value='הצג פרטים' onClick='expand(" + id + "," + i + ")' />" +
                "</fieldset>"
            );

        newItem.insertBefore(oldItem);
    }

    splitedItems[id] = {};
    splitedItems[id].oldItem = oldItem;
    splitedItems[id].oldExpandBtn = oldExpandBtn;
    splitedItems[id].oldSplitBtn = oldSplitBtn;

    oldItem.remove();
    oldExpandBtn.replaceWith($("<input id='expandingGroupBtn-" + id + "' type='button' value='הסתר קבוצה' onClick='expandGroup(" + id + ")' />"));
    oldSplitBtn.replaceWith($("<input id='unSplitBtn-" + id + "' type='button' value='בטל פיצול' onClick='unSplit(" + id + ")' />"));
}

function unSplit(id) {

    if (typeof splitedItems[id] != 'undefined') {
        var allSplitedItems = $(".originalItem-" + id);
        var groupContainer = $("#expandingGroup-" + id);
        var newExpandBtn = $("#expandingGroupBtn-" + id);
        var newSplitBtn = $("#unSplitBtn-" + id);

        allSplitedItems.remove();
        groupContainer.append(splitedItems[id].oldItem);

        newExpandBtn.replaceWith(splitedItems[id].oldExpandBtn);
        newSplitBtn.replaceWith(splitedItems[id].oldSplitBtn);
    }
}