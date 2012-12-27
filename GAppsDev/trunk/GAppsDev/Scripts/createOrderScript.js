var formContainer;
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
var isAddItemDialogOpen = false;

$(function () {

    formContainer = $("#formContainer");
    form = $("#mainForm");
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

    $("#allocationsSelectList").change(function () {
        $(".allocationMonthList").css("display", "none");
        $("#allocation-" + $(this).val()).css("display", "inline-block");
    });

    $("#isFutureOrder").change(function () {
        $("#FutureOrderContainer").toggle();
        $("#monthSelectContainer").toggle();
        $("#NormalOrderContainer").toggle();
    });

    $('#mainForm').submit(function () {
        var isFutureOrder = $("#isFutureOrder").is(':checked');

        var totalAllocation;
        var totalOrderPrice = getFloat($("#totalOrderPrice").val(), 3);

        if (isFutureOrder)
            totalAllocation = getFloat($("#totalFutureAllocation").val(), 3);
        else
            totalAllocation = getFloat($("#totalNormalAllocation").val(), 3);

        if (totalAllocation > totalOrderPrice) {
            alert(local.error_allocation_exceeds_price);
            $("[type='submit']").removeAttr('disabled');
            return false;
        }
        else if (totalAllocation < totalOrderPrice) {
            alert(local.error_allocation_smaller_then_price);
            $("[type='submit']").removeAttr('disabled');
            return false;
        }

        if (isFutureOrder) {
            $("#NormalOrderContainer").remove();
        }
        else {
            $("#FutureOrderContainer").remove();
        }

    });
});

function beginCreate() {
    formContainer.slideToggle(0, null);
    form.slideToggle(500, null);
    selectedSupplier = {};
    selectedSupplier.ID = $("#suppliersList option:selected").val();
    hiddenSupplierField.val(selectedSupplier.ID);
    selectedSupplier.Name = $("#suppliersList option:selected").text();

    $("#suppliersList").replaceWith($("<span class='selectedSupplier'>" + selectedSupplier.Name + "</span>"));
    AddSupplierButton.remove();
    supplierButton.remove();
    $("#AddOrderItemButton").toggle();

    $.ajax({
        type: "GET",
        url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID + "?unique=" + Math.random(),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function (response) {
        if (response.gotData) {
            InitializeItemsList(response.data);

            addItemButton.click(function () {
                if ($("#ItemDropDownList option:selected") != null) {
                    if (isNumber(itemPriceField.val()) && isNumber(itemQuantityField.val())) {
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

function beginEdit(existingItems) {
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
        url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID + "?unique=" + Math.random(),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function (response) {
        if (response.gotData) {
            InitializeItemsList(response.data);

            addItemButton.click(function () {
                if ($("#ItemDropDownList option:selected") != null) {
                    if (isNumber(itemPriceField.val()) && isNumber(itemQuantityField.val())) {
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
    updateTotalAllocation();
}

function addSupplier() {
    var newSupplierId = 0;

    $.ajax({
        type: "POST",
        url: "/Suppliers/PopOutCreate/" + "?unique=" + Math.random(),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function (response) {
        dialogContainer = $(response);
        dialogContainer.find("#submitSupplier").click(function () {
            var newSupplier = {};
            newSupplier.Name = $("#Name").val();
            newSupplier.VAT_Number = $("#VAT_Number").val();
            newSupplier.Phone_Number = $("#Phone_Number").val();
            newSupplier.Address = $("#Address").val();
            newSupplier.City = $("#City").val();
            newSupplier.Customer_Number = $("#Customer_Number").val();
            newSupplier.Additional_Phone = $("#Additional_Phone").val();
            newSupplier.EMail = $("#EMail").val();
            newSupplier.Fax = $("#Fax").val();
            newSupplier.Activity_Hours = $("#Activity_Hours").val();
            newSupplier.Branch_line = $("#Branch_line").val();
            newSupplier.Presentor_name = $("#Presentor_name").val();
            newSupplier.Crew_Number = $("#Crew_Number").val();
            newSupplier.Notes = $("#Notes").val();

            $.ajax({
                type: "POST",
                url: "/Suppliers/AjaxCreate/" + "?unique=" + Math.random(),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: newSupplier
            }).done(function (response) {
                if (response.success) {
                    newSupplierId = response.newSupplierId;
                    alert(local.SupplierAdded);
                }
                else {
                    alert(response.message);
                }
            });

            dialogContainer.dialog("close");
            dialogContainer.dialog("destroy");
            dialogContainer.remove();
        });

        createSupplierDialog = dialogContainer.dialog({
            title: local.AddSupplier,
            width: 400,
            height: 540,
            close: function () {
                $.ajax({
                    type: "GET",
                    url: "/Suppliers/GetAll" + "?unique=" + Math.random(),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json"
                }).done(function (response) {
                    if (response.gotData) {
                        UpdateSupplierList(response.data);

                        $('#suppliersList option[value="' + newSupplierId + '"]').attr('selected', 'selected');
                    }
                });
            }
        });
    });
}

function addOrderItem() {
    if (!isAddItemDialogOpen) {
        var newItemId = 0;
        $.ajax({
            type: "POST",
            url: "/OrderItems/PopOutCreate/"
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

                        $.ajax({
                            type: "GET",
                            url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID + "?unique=" + Math.random(),
                            contentType: "application/json; charset=utf-8",
                            dataType: "json"
                        }).done(function (response) {
                            if (response.gotData) {
                                UpdateItemsList(response.data);
                                $('#ItemDropDownList option[value="' + newItemId + '"]').attr('selected', 'selected');
                                createItemDialogContainer.dialog("destroy");
                                createItemDialogContainer.remove();
                                isAddItemDialogOpen = false;
                            }
                        });

                        alert(local.ItemWasCreated);
                    }
                    else {
                        alert(response.message);
                    }
                });

                createItemDialogContainer.dialog("close");
                createItemDialogContainer.dialog("destroy");
                createItemDialogContainer.remove();
                isAddItemDialogOpen = false;
            });

            createItemDialog = createItemDialogContainer.dialog({
                title: local.AddSupplierItem,
                width: 400,
                height: 400,
                close: function () {
                    isAddItemDialogOpen = false;
                    createItemDialogContainer.dialog("destroy");
                    createItemDialogContainer.remove();
                }
            });

            isAddItemDialogOpen = true;
        });
    }
}

function updateItemFinalPrice() {
    var quantity;
    var itemPrice;

    if (isNumber(itemQuantityField.val())) {
        quantity = getFloat(itemQuantityField.val(), 3);
    }
    else {
        quantity = 0;
    }

    if (isNumber(itemPriceField.val())) {
        itemPrice = getFloat(itemPriceField.val(), 3);
    }
    else {
        itemPrice = 0;
    }

    itemFinalPrice.val(getFloat(quantity * itemPrice, 3));
}

function UpdateSupplierList(newSupplierList) {
    selectText = "";
    selectText += "<select id='suppliersList' name='SupplierId'>";

    for (var i = 0; i < newSupplierList.length; i++) {
        selectText += "<option value=" + newSupplierList[i].Id + ">" + newSupplierList[i].Name + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#suppliersList").replaceWith(dropDownList);
}

function InitializeItemsList(newItemList) {
    selectText = "";
    selectText += "<select class='supplierItemsSelectList' id='ItemDropDownList' name='ItemId'>";

    for (var i = 0; i < newItemList.length; i++) {
        var subTitle;

        if (typeof (newItemList[i].SubTitle) != "string")
            subTitle = "";
        else
            subTitle = " - " + newItemList[i].SubTitle;

        selectText += "<option title='" + newItemList[i].Title + subTitle + "' value=" + newItemList[i].Id + ">" + newItemList[i].Title + subTitle + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#loadingMessage").replaceWith(dropDownList);
}

function UpdateItemsList(newItemList) {
    selectText = "";
    selectText += "<select class='supplierItemsSelectList' id='ItemDropDownList' name='ItemId'>";

    for (var i = 0; i < newItemList.length; i++) {
        var subTitle;

        if (typeof (newItemList[i].SubTitle) != "string")
            subTitle = "";
        else
            subTitle = " - " + newItemList[i].SubTitle;

        selectText += "<option title='" + newItemList[i].Title + subTitle + "' value=" + newItemList[i].Id + ">" + newItemList[i].Title + subTitle + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#ItemDropDownList").replaceWith(dropDownList);
}

function addNewItem(itemId, itemName, quantity, price) {

    if (isNaN(itemId)) {
        alert(local.NoItemSelected);
        return;
    }

    quantity = getFloat(quantity, 3);
    price = getFloat(price, 3);

    var itemToInsert = { id: itemId, title: itemName, quantity: quantity, price: price, finalPrice: getFloat(price * quantity, 3) };
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
            itemList[doubleIndex].quantity = (getFloat(itemList[doubleIndex].quantity, 3)) + (getFloat(itemToInsert.quantity, 3));
            itemList[doubleIndex].price = itemToInsert.price;
            itemList[doubleIndex].finalPrice = (getFloat(itemList[doubleIndex].price, 3)) * (getFloat(itemList[doubleIndex].quantity, 3));
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
        addedItemsContainer.append($("<div id='ItemlistIndex-" + i + "' class='addedItem'><span class='bold'>" + itemList[i].title + ":</span> <span class='bold'>" + local.Quantity + ":</span> " + itemList[i].quantity + " <span class='bold'>" + local.SingleItemPrice + ":</span> " + itemList[i].price + " <span class='bold'>" + local.FinalPrice + ":</span> " + itemList[i].finalPrice + "</span> <input class='RemoveItemButton' onClick='removeItem(" + i + ")' type='button' value='" + local.Delete + "'/></div>"));
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
    totalOrderPriceField.val(getFloat(totalPrice, 3));
}

function removeItem(index) {
    $("#ItemlistIndex-" + index).remove();
    itemList.splice(index, 1);
    updateItems();
}

function isNumber(value) {

    //if (value == null) return false;
    //if (value == "") return false;
    //return !isNaN(value);

    //return typeof value === "Number";
    
    var intRegex = /^(?:\d+|\d*\.\d+)$/ // /^\d+$/;
    if (intRegex.test(value)) {
        return true;
    }
    else {
        return false;
    }
}

function addAllocation() {
    
    var allocationId = $("#allocationsSelectList option:selected").val();
    var monthId = $("#allocation-" + allocationId + " option:selected").val();
    var monthName = $("#allocation-" + allocationId + " option:selected").text();
    var wantedAmount = getFloat($("#allocationAmount").val() * 1000) / 1000;
    var remainingMonthAmount = getFloat($("#allocation-" + allocationId + " option:selected").data("amount"), 3);
    var remainingAllocationAmount = getFloat($("#allocationsSelectList option:selected").data("amount"), 3);
    var allocationText = $("#allocationsSelectList option:selected").text();
    var allocationExists = false;
    var existingAllocation;
    var existingRemovedAllocation;
    var divClass;
    var monthText;

    var isExeedingAllocation = false;
    var exeedingMark = "";

    if (!isNumber(wantedAmount) || wantedAmount <= 0) {
        alert(local.InvalidAmount);
        return;
    }

    wantedAmount = getFloat(wantedAmount, 3);

    var isFutureOrder = $("#isFutureOrder").is(':checked');

    var container;
    var existingAllocations;
    if (isFutureOrder) {
        container = $("#futureAllocationsContainer");
        existingAllocations = $(".existingFutureAllocations");

        if (wantedAmount > remainingMonthAmount) {
            isExeedingAllocation = true;
            //alert(local.AmountExceedsAllocation);
            //return;
        }

        for (var i = 0; i < existingAllocations.length; i++) {
            if ($(existingAllocations[i]).find(".allocationIdField").val() == allocationId && $(existingAllocations[i]).find(".monthIdField").val() == monthId) {
                allocationExists = true;
                existingAllocation = $(existingAllocations[i]);
            }
        }

        divClass = "existingFutureAllocations";
        divId = "futureAllocation";
        monthText = "<span class='bold'>" + local.Month + ":</span> " + monthName + " ";
    }
    else {
        container = $("#normalAllocationsContainer");
        existingAllocations = $(".existingNormalAllocations");

        if (wantedAmount > remainingAllocationAmount) {
            isExeedingAllocation = true;
            //alert(local.AmountExceedsAllocation);
            //return;
        }

        for (var i = 0; i < existingAllocations.length; i++) {
            if ($(existingAllocations[i]).find(".allocationIdField").val() == allocationId) {
                allocationExists = true;
                existingAllocation = $(existingAllocations[i]);
            }
        }

        monthId = '';
        divClass = "existingNormalAllocations";
        divId = "normalAllocation";
        monthText = "";
    }
    
    if (isExeedingAllocation) {
        divClass += " exeedingAllocation";
        exeedingMark += " <span class='exeedingMark' style='color:red;'> (" + local.ExeedingAllocation + ") </span> ";
    }

    var nextNumber = existingAllocations.length;

    if (nextNumber == 0)
        container.html("");
    
    existingRemovedAllocation = getRemovedAllocation(allocationId, monthId);

    if (existingRemovedAllocation == null) {
        if (!allocationExists) {
            var newAllocation = $(
                "<div id='" + divId + "-" + nextNumber + "' class='" + divClass + "'>" +
                    "<input type='hidden' class='isActiveField' id='" + divId + "-isActiveField-" + nextNumber + "' name='Allocations[" + nextNumber + "].IsActive' value='true' />" +
                    "<input type='hidden' class='allocationIdField' id='" + divId + "-allocationIdField-" + nextNumber + "' name='Allocations[" + nextNumber + "].AllocationId' value='" + allocationId + "' />" +
                    "<input type='hidden' class='monthIdField' id='" + divId + "-monthIdField-" + nextNumber + "' name='Allocations[" + nextNumber + "].MonthId' value='" + monthId + "' />" +
                    "<input type='hidden' class='amountField' id='" + divId + "-amountField-" + nextNumber + "' name='Allocations[" + nextNumber + "].Amount' value='" + wantedAmount + "' />" +
                    "<span class='allocationText'> <span class='bold'>" + local.Allocation + ": </span>" + allocationText + " " + monthText + "<span class='bold'>" + local.Amount + ":</span> <span class='amountText'>" + wantedAmount + "</span></span>" +
                    "<span class='markContainer'>" + exeedingMark + "</span>" +
                    "<input type='button'  value='" + local.Delete + "' onClick='removeAllocation(\"" + divId + "\", " + nextNumber + ") '/>" + 
                "</div>"
                );

            container.append(newAllocation);
        }
        else {
            existingAllocation.find(".amountField").val(wantedAmount);
            existingAllocation.find(".amountText").html(wantedAmount);
            if (!isExeedingAllocation) {
                existingAllocation.find(".exeedingMark").remove();
                existingAllocation.removeClass("exeedingAllocation");
            }
        }
    }
    else {
        unRemove(allocationId, monthId);
        existingAllocation.find(".amountField").val(wantedAmount);
        existingAllocation.find(".amountText").html(wantedAmount);
        if (!isExeedingAllocation) {
            existingAllocation.find(".exeedingMark").remove();
            existingAllocation.removeClass("exeedingAllocation");
        }
    }

    if (allocationExists) {
        if (isExeedingAllocation) {
            var markContainer = existingAllocation.find(".markContainer");
            markContainer.html("");
            markContainer.append(exeedingMark);
            existingAllocation.addClass("exeedingAllocation");
        }
        else {
            existingAllocation.find(".exeedingMark").remove();
            existingAllocation.removeClass("exeedingAllocation");
        }
    }

    updateTotalAllocation();
    $("#allocationAmount").val("");
}

function removeAllocation(divId, allocationIndex) {
    var container = $("#" + divId + "-" + allocationIndex);
    var isActiveField = $("#" + divId + "-isActiveField-" + allocationIndex);
    var isActive = isActiveField.val();
    var allocationId = $("#" + divId + "-allocationIdField-" + allocationIndex).val();
    var monthId = $("#" + divId + "-monthIdField-" + allocationIndex).val();
    isActiveField.val("false");

    var existingRemovedAllocation = getRemovedAllocation(allocationId, monthId);
    if (existingRemovedAllocation == null) {
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

        var itemIndex = null;

        var newRemovedList = new Array();

        for (var i = 0; i < removedAllocations.length; i++) {
            if (removedAllocations[i] != existingRemovedAllocation) {
                newRemovedList[newRemovedList.length] = removedAllocations[i];
            }
        }

        removedAllocations = newRemovedList;
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
    var existingFutureAllocations = $(".existingFutureAllocations");
    var existingNormalAllocations = $(".existingNormalAllocations");
    var totalFutureAllocation = 0;
    var totalNormalAllocation = 0;

    for (var i = 0; i < existingFutureAllocations.length; i++) {
        if ($(existingFutureAllocations[i]).find(".isActiveField").val() == "true")
            totalFutureAllocation += getFloat($(existingFutureAllocations[i]).find(".amountField").val(), 3);
    }

    for (var i = 0; i < existingNormalAllocations.length; i++) {
        if ($(existingNormalAllocations[i]).find(".isActiveField").val() == "true")
            totalNormalAllocation += getFloat($(existingNormalAllocations[i]).find(".amountField").val(), 3);
    }

    $("#totalNormalAllocation").val(getFloat(totalNormalAllocation, 3));
    $("#totalFutureAllocation").val(getFloat(totalFutureAllocation, 3));
}

function getFloat(string, precision) {
    var multiply = 1;
    for (var i = 0; i < precision; i++) {
        multiply *= 10;
    }

    return Math.floor(parseFloat(string) * multiply) / multiply;
}
