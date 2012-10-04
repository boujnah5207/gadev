var formContainer;
var form;
var supplierButton;
var suppliersList;
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

$(function () {
    formContainer = $("#formContainer");
    form = $("#formContainer form");
    supplierButton = $("#SupplierButton");
    suppliersList = $("#suppliersList");
    addedItemsContainer = $("#orderItems");
    hiddenItemField = $("#ItemsAsString");
    addItemButton = $("#AddNewItemButton");
    itemQuantityField = $("#itemQuantity");
    itemPriceField = $("#itemPrice");
    itemFinalPrice = $("#itemFinalPrice");
    totalOrderPriceField = $("#totalOrderPrice");
});

function beginForm() {
    formContainer.slideToggle(0, null);
    supplierButton.slideToggle(0, null);
    form.slideToggle(500, null);
    supplierButton.attr("disabled", true);
    suppliersList.attr("disabled", true);

    selectedSupplier = {};
    selectedSupplier.ID = suppliersList.val();
    selectedSupplier.Name = $("#suppliersList option:selected").text();

    suppliersList.replaceWith($("<span class='selectedSupplier'>" + selectedSupplier.Name + "</span>"))

    $.ajax({
        type: "GET",
        url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID,
    }).done(function (response) {
        if (response.gotData) {
            UpdateItemsList(response.data);

            itemDropDownList = $("#ItemDropDownList");
            addItemButton.click(function () {
                addNewItem(
                    itemDropDownList.val(),
                    itemDropDownList.find(" :selected").text(),
                    itemQuantityField.val(),
                    itemPriceField.val()
                    );

                itemPriceField.val("");
                itemQuantityField.val("");
                itemFinalPrice.val("");
            });
        }
        else {
            alert(response.message);
        }
    });

    itemPriceField.keyup(updateItemFinalPrice);
    itemQuantityField.keyup(updateItemFinalPrice);
}

function updateItemFinalPrice() {
    var quantity;
    var itemPrice;

    if (itemQuantityField.val() != "") {
        quantity = parseInt(itemQuantityField.val(), 10);
    }
    else {
        quantity = 0;
    }

    if (itemPriceField.val() != "") {
        itemPrice = parseInt(itemPriceField.val(), 10);
    }
    else {
        itemPrice = 0;
    }

    itemFinalPrice.val(quantity * itemPrice);
}

function UpdateItemsList(newItemList) {
    selectText = "";
    selectText += "<select id='ItemDropDownList' name='ItemId'>";

    for (var i = 0; i < newItemList.length - 1; i++) {
        selectText += "<option value=" + newItemList[i].Id + ">" + newItemList[i].Title + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#loadingMessage").replaceWith(dropDownList);
}

function addNewItem(itemId, itemName, quantity, price) {
    var itemToInsert = { id: itemId, title: itemName, quantity: quantity, price: price, finalPrice: price * quantity };

    var isInArray = false;
    for (var i in itemList) {
        if (itemList[i].id == itemId) {
            isInArray = true;
        }
    }

    if (!isInArray) {
        itemList[itemList.length] = itemToInsert;
        updateItems();
    }
}

function updateItems() {
    addedItemsContainer.html("");

    var value = "";
    var totalPrice = 0;
    for (var i in itemList) {
        addedItemsContainer.append($("<div id='ItemlistIndex-" + i + "' class='addedItem'><span> " + itemList[i].title + " כמות: " + itemList[i].quantity + " מחיר סופי: " + itemList[i].finalPrice + "</span> <input class='RemoveItemButton' onClick='removeItem(" + i + ")' type='button' value='הסר'/></div>"));
        value += itemList[i] + ";";
        totalPrice += itemList[i].quantity * itemList[i].price;
    }
    if (value != "") {
        value = value.slice(0, value.length - 1);
    }
    hiddenItemField.val(value);
    totalOrderPriceField.val(totalPrice);
    console.log(value);
}

function removeItem(index) {
    $("#ItemlistIndex-" + index).remove();
    itemList.splice(index, 1);
    updateItems();
}