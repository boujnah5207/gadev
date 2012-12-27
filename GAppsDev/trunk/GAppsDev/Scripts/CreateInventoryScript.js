var isSupplierDialogOpen = false;
var isAddItemDialogOpen = false;
$(function () {
    $("#suppliersDropDownList").change(getSupplierItems);
    $("#addSupplierButton").click(addSupplier);
    $("#addItemButton").click(addOrderItem);
    getSupplierItems();
})

function getSupplierItems() {
    var supplierId = $("#suppliersDropDownList option:selected").val();
    $.ajax({
        type: "GET",
        url: "/OrderItems/GetBySupplier/" + supplierId,
    }).done(function (response) {
        if (response.gotData) {
            UpdateItemsList(response.data);
        }
    });
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

function addSupplier() {
    var newSupplierId = 0;
    if (isSupplierDialogOpen)
        return;
    isSupplierDialogOpen = true;

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
            isSupplierDialogOpen = false;

        });

        dialogContainer.find("#submitSupplier").click(function () {

        });

        createSupplierDialog = dialogContainer.dialog({
            title: local.AddSupplier,
            width: 400,
            height: 540,
            close: function () {
                $.ajax({
                    type: "GET",
                    url: "/Suppliers/GetAll?firstNull=true",
                }).done(function (response) {
                    if (response.gotData) {
                        UpdateSupplierList(response.data);

                        $('#suppliersDropDownList option[value="' + newSupplierId + '"]').attr('selected', 'selected');
                    }
                });
                isSupplierDialogOpen = false;

            }
        });

    });
}

function UpdateSupplierList(newSupplierList) {
    selectText = "";
    selectText += "<select id='suppliersDropDownList' name='sid'>";

    for (var i = 0; i < newSupplierList.length; i++) {
        selectText += "<option value=" + newSupplierList[i].Id + ">" + newSupplierList[i].Name + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#suppliersDropDownList").replaceWith(dropDownList);
}

function addOrderItem() {
    if (!isAddItemDialogOpen) {
        var supplierId = $("#suppliersDropDownList option:selected").val();
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
                newOrderItem.SupplierId = supplierId;

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
                        url: "/OrderItems/GetBySupplier/" + supplierId,
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
