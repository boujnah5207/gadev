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

function beginForm() {
    formContainer.slideToggle(0, null);
    supplierButton.slideToggle(0, null);
    form.slideToggle(500, null);
    supplierButton.attr("disabled", true);
    suppliersList.attr("disabled", true);

    selectedSupplier = {};
    selectedSupplier.ID = suppliersList.val();
    hiddenSupplierField.val(selectedSupplier.ID);
    console.log(hiddenSupplierField);
    console.log(hiddenSupplierField.val());
    selectedSupplier.Name = $("#suppliersList option:selected").text();

    $("#suppliersList").replaceWith($("<span class='selectedSupplier'>" + selectedSupplier.Name + "</span>"))
    AddSupplierButton.remove();

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

function addSupplier() {
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

            console.log("newSupplier created");
            console.log(newSupplier);

            $.ajax({
                type: "POST",
                url: "/Suppliers/AjaxCreate/",
                data: newSupplier
            }).done(function (response) {
                if (response.success) {
                    alert("הספק נוצר בהצלחה.");
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
            title: "הוסף ספק",
            width: 400,
            height: 540,
            close: function () {
                $.ajax({
                    type: "GET",
                    url: "/Suppliers/GetAll",
                }).done(function (response) {
                    if (response.gotData) {
                        UpdateSupplierList(response.data);

                        $('#suppliersList option:last-child').attr('selected', 'selected');
                    }
                });
            }
        })
    });
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
                    alert("success");
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
        value += itemList[i].id + "," + itemList[i].quantity + "," + itemList[i].price + ";";
        totalPrice += itemList[i].quantity * itemList[i].price;
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