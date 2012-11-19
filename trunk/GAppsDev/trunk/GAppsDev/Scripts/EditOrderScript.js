﻿var formContainer;
var form;
var supplierButton;
var AddSupplierButton;
var suppliersList;
var hiddenSupplierField;
var selectedSupplier;
var totalOrderPriceField;
var itemList = new Array();
var addItemButton;
var itemDropDownList;
var itemQuantityField;
var itemPriceField;
var itemFinalPrice;
var addedItemsContainer;
var hiddenItemField;
var dialogContainer;
var createSupplierDialog;
var createItemDialogContainer;
var createItemDialog;
var removedAllocations = new Array();

$(function () {
    formContainer = $("#formContainer");
    form = $("#formContainer form");
    supplierButton = $("#SupplierButton");
    AddSupplierButton = $("#AddSupplierButton");
    suppliersList = $("#suppliersList");
    hiddenSupplierField = $("#SupplierId");
    addedItemsContainer = $("#orderItems");
    hiddenItemField = $("#ItemsAsString");
    addItemButton = $("#AddNewItemButton");
    itemQuantityField = $("#itemQuantity");
    itemPriceField = $("#itemPrice");
    itemFinalPrice = $("#itemFinalPrice");
    totalOrderPriceField = $("#totalOrderPrice");
});

function beginForm(existingItems) {
    selectedSupplier = {};
    selectedSupplier.ID = hiddenSupplierField.val();

    if (existingItems != "") {
        var items = existingItems.split(";");
        for (var i in items) {
            itemValues = items[i].split(",");
            addNewItem(
                itemValues[0],
                itemValues[1],
                itemValues[2],
                itemValues[3]
            );
        }
    }

    $.ajax({
        type: "GET",
        url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID,
    }).done(function (response) {
        if (response.gotData) {
            InitializeItemsList(response.data);

            addItemButton.click(function () {
                if ($("#ItemDropDownList option:selected") != null) {
                    if (isInt(itemPriceField.val()) && isInt(itemQuantityField.val())) {
                        if (itemQuantityField.val() != 0) {
                            addNewItem(
                                $("#ItemDropDownList option:selected").val(),
                                $("#ItemDropDownList option:selected").text(),
                                itemQuantityField.val(),
                                itemPriceField.val()
                                );
                        }
                        else {
                            alert(local.QuantityIsZero);
                        }
                    }
                    else {
                        alert(local.InvalidQuantityOrPrice);
                    }
                }
                else {
                    alert(local.NoItemSelected);
                }
            });
        }
        else {
            alert(response.message);
        }
    });

    itemPriceField.keyup(updateItemFinalPrice);
    itemQuantityField.keyup(updateItemFinalPrice);
}

function addOrderItem() {
    var newItemId = 0;
    $.ajax({
        type: "POST",
        url: "/OrderItems/PopOutCreate/",
    }).done(function (response) {
        createItemDialogContainer = $(response);
        createItemDialogContainer.find("#submitOrderItem").click(function () {
            var newOrderItem = {};
            newOrderItem.Title = $("#Title").val();
            newOrderItem.SubTitle = $("#SubTitle").val();
            newOrderItem.SupplierId = selectedSupplier.ID;

            $.ajax({
                type: "POST",
                url: "/OrderItems/AjaxCreate/",
                data: newOrderItem
            }).done(function (response) {
                if (response.success) {
                    newItemId = response.newItemId;
                    alert(local.ItemWasCreated);
                }
                else {
                    alert(response.message);
                }
            });

            createItemDialogContainer.dialog("close");
            createItemDialogContainer.dialog("destroy");
            createItemDialogContainer.remove();
        });

        createItemDialog = createItemDialogContainer.dialog({
            title: local.AddItem,
            width: 400,
            height: 400,
            close: function () {
                $.ajax({
                    type: "GET",
                    url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID,
                }).done(function (response) {
                    if (response.gotData) {
                        UpdateItemsList(response.data);
                        $('#ItemDropDownList option[value="' + newItemId + '"]').attr('selected', 'selected');
                        createItemDialogContainer.dialog("destroy");
                        createItemDialogContainer.remove();
                    }
                });
            }
        });
    });
}

function updateItemFinalPrice() {
    var quantity;
    var itemPrice;

    if (isInt(itemQuantityField.val())) {
        quantity = parseInt(itemQuantityField.val(), 10);
    }
    else {
        quantity = 0;
    }

    if (isInt(itemPriceField.val())) {
        itemPrice = parseInt(itemPriceField.val(), 10);
    }
    else {
        itemPrice = 0;
    }

    itemFinalPrice.val(quantity * itemPrice);
}

function InitializeItemsList(newItemList) {
    selectText = "";
    selectText += "<select id='ItemDropDownList' name='ItemId'>";

    for (var i = 0; i < newItemList.length; i++) {
        selectText += "<option value=" + newItemList[i].Id + ">" + newItemList[i].Title + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#loadingMessage").replaceWith(dropDownList);
}

function UpdateItemsList(newItemList) {
    selectText = "";
    selectText += "<select id='ItemDropDownList' name='ItemId'>";

    for (var i = 0; i < newItemList.length; i++) {
        selectText += "<option value=" + newItemList[i].Id + ">" + newItemList[i].Title + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#ItemDropDownList").replaceWith(dropDownList);
}

function addNewItem(itemId, itemName, quantity, price) {
    var itemToInsert = { id: itemId, title: itemName, quantity: quantity, price: price, finalPrice: price * quantity };

    var isInArray = false;
    var doubleIndex;
    for (var i in itemList) {
        if (itemList[i].id == itemId) {
            isInArray = true;
            doubleIndex = i;
            break;
        }
    }

    if (!isInArray) {
        itemList[itemList.length] = itemToInsert;
        itemPriceField.val("");
        itemQuantityField.val("");
        itemFinalPrice.val("0");
        updateItems();
    }
    else {
        var doubleItemDialog = $("<div><span>" + local.DuplicateItemFound + "</span></div>");
        var dialog_buttons = {};

        dialog_buttons[local.Merge] = function () {
            itemList[doubleIndex].quantity = parseInt(itemList[doubleIndex].quantity, 10) + parseInt(itemToInsert.quantity, 10);
            itemList[doubleIndex].price = itemToInsert.price;
            itemList[doubleIndex].finalPrice = parseInt(itemList[doubleIndex].price, 10) * parseInt(itemList[doubleIndex].quantity, 10);
            itemPriceField.val("");
            itemQuantityField.val("");
            itemFinalPrice.val("0");
            updateItems();
            $(this).dialog("close");
        }
        dialog_buttons[local.Replace] = function () {
            itemList[doubleIndex] = itemToInsert;
            itemPriceField.val("");
            itemQuantityField.val("");
            itemFinalPrice.val("0");
            updateItems();
            $(this).dialog("close");
        }
        dialog_buttons[local.Cancel] = function () {
            $(this).dialog("close");
        }

        doubleItemDialog.dialog({
            title: local.ItemDuplication,
            width: 400,
            height: 150,
            buttons: dialog_buttons
        });
    }
}

function updateItems() {
    addedItemsContainer.html("");

    var value = "";
    var totalPrice = 0;
    for (var i in itemList) {
        addedItemsContainer.append($("<div id='ItemlistIndex-" + i + "' class='addedItem'><span> " + itemList[i].title + " " + local.Quantity + ": " + itemList[i].quantity + " " + local.FinalPrice + ": " + itemList[i].finalPrice + "</span> <input class='RemoveItemButton' onClick='removeItem(" + i + ")' type='button' value='" + local.Delete + "'/></div>"));
        value += itemList[i].id + "," + itemList[i].quantity + "," + itemList[i].price + ";";
        totalPrice += itemList[i].quantity * itemList[i].price;
    }
    if (itemList.length == 0) {
        addedItemsContainer.append($("<span>" + local.NoItemsInOrder + ".</span>"));
    }
    if (value != "") {
        value = value.slice(0, value.length - 1);
    }
    hiddenItemField.val(value);
    totalOrderPriceField.val(totalPrice);
}

function removeItem(index) {
    $("#ItemlistIndex-" + index).remove();
    itemList.splice(index, 1);
    updateItems();
}

function isInt(value) {
    var intRegex = /^\d+$/;
    if (intRegex.test(value)) {
        return true;
    }
    else {
        return false;
    }
}


function addAllocation() {
    var allocationId = $("#allocationSelectList").val();
    var monthId = $("#allocation-" + allocationId + " option:selected").val();
    var monthName = $("#allocation-" + allocationId + " option:selected").text();
    var wantedAmount = $("#allocationAmount").val();
    var remainingAmount = $("#allocation-" + allocationId + " option:selected").data("amount");
    var allocationText = $("#allocationSelectList option:selected").text();
    var container = $("#orderAllocations");

    if (!isInt(wantedAmount) || wantedAmount <= 0) {
        alert(local.InvalidAmount);
        return;
    }
    if (wantedAmount > remainingAmount) {
        alert(local.AmountExceedsAllocation);
        return;
    }

    var existingAllocations = $(".existingAllocations");
    var nextNumber = existingAllocations.length;

    if (nextNumber == 0)
        $("#orderAllocations").html("");

    console.log(existingAllocations);
    var allocationExists = false;
    var existingAllocation;

    for (var i = 0; i < existingAllocations.length; i++) {
        console.log($(existingAllocations[i]).val() + "=" + allocationId);
        if ($(existingAllocations[i]).find(".allocationIdField").val() == allocationId && $(existingAllocations[i]).find(".monthIdField").val() == monthId) {
            allocationExists = true;
            existingAllocation = $(existingAllocations[i]);
        }
    }

    var existingRemovedAllocation = getRemovedAllocation(allocationId, monthId);
    if (existingRemovedAllocation == null) {
        if (!allocationExists) {
            var newAllocation = $(
                "<div id='allocation-" + nextNumber + "' class='existingAllocations'>" +
                    "<input type='hidden' class='isActiveField' id='isActiveField-" + nextNumber + "' name='Allocations[" + nextNumber + "].IsActive' value='true' />" +
                    "<input type='hidden' class='allocationIdField' id='allocationIdField-" + nextNumber + "' name='Allocations[" + nextNumber + "].AllocationId' value='" + allocationId + "' />" +
                    "<input type='hidden' class='monthIdField' id='monthIdField-" + nextNumber + "' name='Allocations[" + nextNumber + "].MonthId' value='" + monthId + "' />" +
                    "<input type='hidden' class='amountField' id='amountField-" + nextNumber + "' name='Allocations[" + nextNumber + "].Amount' value='" + wantedAmount + "' />" +
                    "<span class='allocationText'> Allocation: " + allocationText + " Month: " + monthName + " Amount: <span class='amountText'>" + wantedAmount + "</span></span>" +
                    "<input type='button'  value='" + local.Delete + "' onClick='removeAllocation(" + nextNumber + ") '/>" +
                "</div>"
                );

            container.append(newAllocation);
        }
        else {
            existingAllocation.find(".amountField").val(wantedAmount);
            existingAllocation.find(".amountText").html(wantedAmount);
        }
    }
    else {
        unRemove(allocationId, monthId);
        existingAllocation.find(".amountField").val(wantedAmount);
        existingAllocation.find(".amountText").html(wantedAmount);
    }

    updateTotalAllocation();
    $("#allocationAmount").val("");
}

function removeAllocation(allocationIndex) {
    var container = $("#allocation-" + allocationIndex);
    var isActiveField = $("#isActiveField-" + allocationIndex);
    var isActive = isActiveField.val();
    var allocationId = $("#allocationIdField-" + allocationIndex).val();
    var monthId = $("#monthIdField-" + allocationIndex).val();
    console.log("index: " + allocationIndex);
    console.log("id: " + allocationId);
    isActiveField.val("false");

    var existingRemovedAllocation = getRemovedAllocation(allocationId, monthId);
    if (existingRemovedAllocation == null) {
        console.log(removedAllocations.length);
        var newRemovedAllocation = { allocationId: allocationId, monthId: monthId, oldItem: container };
        removedAllocations[removedAllocations.length] = newRemovedAllocation;

        container.toggle(0);
    }

    updateTotalAllocation();
}

function unRemove(allocationId, monthId) {
    var existingRemovedAllocation = getRemovedAllocation(allocationId, monthId);
    if (existingRemovedAllocation != null) {
        existingRemovedAllocation.oldItem.toggle(0);
        existingRemovedAllocation.oldItem.find(".isActiveField").val("true");

        console.log("old list: " + removedAllocations.length);
        var itemIndex = null;

        var newRemovedList = new Array();

        for (var i = 0; i < removedAllocations.length; i++) {
            console.log("index: " + i);

            if (removedAllocations[i] != existingRemovedAllocation) {
                newRemovedList[newRemovedList.length] = removedAllocations[i];
            }
        }

        console.log("new list: " + newRemovedList.length);

        removedAllocations = newRemovedList;

        console.log(removedAllocations);
    }
}

function getRemovedAllocation(allocationId, monthId) {
    for (var index in removedAllocations) {
        if (removedAllocations[index].allocationId == allocationId && removedAllocations[index].monthId == monthId) {
            return removedAllocations[index];
        }
    }

    return null;
}

function updateTotalAllocation() {
    var existingAllocations = $(".existingAllocations");
    var totalAllocation = 0;

    for (var i = 0; i < existingAllocations.length; i++) {
        if ($(existingAllocations[i]).find(".isActiveField").val() == "true")
            totalAllocation += parseInt($(existingAllocations[i]).find(".amountField").val());
    }

    $("#totalAllocation").val(totalAllocation);
}