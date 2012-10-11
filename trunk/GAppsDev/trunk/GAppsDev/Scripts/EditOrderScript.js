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
    console.log(existingItems);

    selectedSupplier = {};
    selectedSupplier.ID = hiddenSupplierField.val();

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

    $.ajax({
        type: "GET",
        url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID,
    }).done(function (response) {
        if (response.gotData) {
            InitializeItemsList(response.data);

            itemDropDownList = $("#ItemDropDownList");
            addItemButton.click(function () {
                if (isInt(itemPriceField.val()) && isInt(itemQuantityField.val())) {
                    addNewItem(
                        itemDropDownList.val(),
                        itemDropDownList.find(" :selected").text(),
                        itemQuantityField.val(),
                        itemPriceField.val()
                        );
                }
                else {
                    alert("אנא הכנס כמות ומחיר ונסה שוב.");
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
            console.log($("#Title"));
            console.log("newOrderItem created");
            console.log(newOrderItem);

            $.ajax({
                type: "POST",
                url: "/OrderItems/AjaxCreate/",
                data: newOrderItem
            }).done(function (response) {
                if (response.success) {
                    alert("הפריט נוצר בהצלחה.");
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
            title: "הוסף פריט",
            width: 400,
            height: 400,
            close: function () {
                $.ajax({
                    type: "GET",
                    url: "/OrderItems/GetBySupplier/" + selectedSupplier.ID,
                }).done(function (response) {
                    if (response.gotData) {
                        UpdateItemsList(response.data);

                        createItemDialogContainer.dialog("destroy");
                        createItemDialogContainer.remove();
                    }
                });
            }
        })
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
        var doubleItemDialog = $("<div><span>הפריט כבר נמצא בהזמנה, מה ברצונך לעשות?</span></div>");
        doubleItemDialog.dialog({
            title: "כפילות פריטים",
            width: 400,
            height: 150,
            buttons: 
                    {
                        "מזג": function () {
                            itemList[doubleIndex].quantity = parseInt(itemList[doubleIndex].quantity, 10) + parseInt(itemToInsert.quantity, 10);
                            itemList[doubleIndex].price = itemToInsert.price;
                            itemList[doubleIndex].finalPrice = parseInt(itemList[doubleIndex].price, 10) * parseInt(itemList[doubleIndex].quantity, 10);
                            itemPriceField.val("");
                            itemQuantityField.val("");
                            itemFinalPrice.val("0");
                            updateItems();
                            $(this).dialog("close");
                        },
                        "החלף": function () {
                            itemList[doubleIndex] = itemToInsert;
                            itemPriceField.val("");
                            itemQuantityField.val("");
                            itemFinalPrice.val("0");
                            updateItems();
                            $(this).dialog("close");
                        },
                        "ביטול": function () {
                            $(this).dialog("close")
                        }
                    }
        });
    }
}

function updateItems() {
    addedItemsContainer.html("");

    var value = "";
    var totalPrice = 0;
    for (var i in itemList) {
        addedItemsContainer.append($("<div id='ItemlistIndex-" + i + "' class='addedItem'><span> " + itemList[i].title + " כמות: " + itemList[i].quantity + " מחיר סופי: " + itemList[i].finalPrice + "</span> <input class='RemoveItemButton' onClick='removeItem(" + i + ")' type='button' value='הסר'/></div>"));
        value += itemList[i].id + "," + itemList[i].quantity + "," + itemList[i].price + ";";
        totalPrice += itemList[i].quantity * itemList[i].price;
    }
    if (itemList.length == 0) {
        addedItemsContainer.append($("<span>אין פריטים בהזמנה.</span>"));
    }
    if (value != "") {
        value = value.slice(0, value.length - 1);
    }
    hiddenItemField.val(value);
    console.log(value);
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