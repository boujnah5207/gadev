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
        var totalOrderPrice = parseInt($("#totalOrderPrice").val());

        if (isFutureOrder)
            totalAllocation = parseInt($("#totalFutureAllocation").val());
        else
            totalAllocation = parseInt($("#totalNormalAllocation").val());

        if (totalAllocation > totalOrderPrice) {
            alert(local.error_allocation_exceeds_price);
            return false;
        }
        else if (totalAllocation < totalOrderPrice) {
            alert(local.error_allocation_smaller_then_price);
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

function beginForm() {
    formContainer.slideToggle(0, null);
    form.slideToggle(500, null);
    selectedSupplier = {};
    selectedSupplier.ID = $("#suppliersList option:selected").val();
    hiddenSupplierField.val(selectedSupplier.ID);
    selectedSupplier.Name = $("#suppliersList option:selected").text();

    $("#suppliersList").replaceWith($("<span class='selectedSupplier'>" + selectedSupplier.Name + "</span>"));
    AddSupplierButton.remove();
    supplierButton.remove();

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

function addSupplier() {
    var newSupplierId = 0;

    $.ajax({
        type: "POST",
        url: "/Suppliers/PopOutCreate/",
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
                url: "/Suppliers/AjaxCreate/",
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
                    url: "/Suppliers/GetAll",
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
                isAddItemDialogOpen = false;
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
                            isAddItemDialogOpen = false;
                        }
                    });
                }
            });

            isAddItemDialogOpen = true;
        });
    }
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
        addedItemsContainer.append($("<div id='ItemlistIndex-" + i + "' class='addedItem'><span class='bold'>" + itemList[i].title + ":</span> <span class='bold'>" + local.Quantity + ":</span> " + itemList[i].quantity + " <span class='bold'>" + local.FinalPrice + ":</span> " + itemList[i].finalPrice + "</span> <input class='RemoveItemButton' onClick='removeItem(" + i + ")' type='button' value='" + local.Delete + "'/></div>"));
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
    
    var allocationId = $("#allocationsSelectList option:selected").val();
    var monthId = $("#allocation-" + allocationId + " option:selected").val();
    var monthName = $("#allocation-" + allocationId + " option:selected").text();
    var wantedAmount = $("#allocationAmount").val();
    var remainingMonthAmount = $("#allocation-" + allocationId + " option:selected").data("amount");
    var remainingAllocationAmount = $("#allocationsSelectList option:selected").data("amount");
    var allocationText = $("#allocationsSelectList option:selected").text();
    var allocationExists = false;
    var existingAllocation;
    var existingRemovedAllocation;
    var divClass;
    var monthText;

    if (!isInt(wantedAmount) || wantedAmount <= 0) {
        alert(local.InvalidAmount);
        return;
    }

    var isFutureOrder = $("#isFutureOrder").is(':checked');

    var container;
    var existingAllocations;
    if (isFutureOrder) {
        container = $("#futureAllocationsContainer");
        existingAllocations = $(".existingFutureAllocations");

        if (wantedAmount > remainingMonthAmount) {
            alert(local.AmountExceedsAllocation);
            return;
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
            alert(local.AmountExceedsAllocation);
            return;
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
                    "<input type='button'  value='" + local.Delete + "' onClick='removeAllocation(\"" + divId + "\", " + nextNumber + ") '/>" +
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
            totalFutureAllocation += parseInt($(existingFutureAllocations[i]).find(".amountField").val());
    }

    for (var i = 0; i < existingNormalAllocations.length; i++) {
        if ($(existingNormalAllocations[i]).find(".isActiveField").val() == "true")
            totalNormalAllocation += parseInt($(existingNormalAllocations[i]).find(".amountField").val());
    }

    $("#totalNormalAllocation").val(totalNormalAllocation);
    $("#totalFutureAllocation").val(totalFutureAllocation);
}