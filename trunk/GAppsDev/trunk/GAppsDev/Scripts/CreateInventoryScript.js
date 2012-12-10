var isSupplierDialogOpen = false;

$(function () {
    $("#suppliersDropDownList").change(getSupplierItems);
    $("#addSupplierButton").click(addSupplier);
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
    selectText += "<select id='supplierItems' name='ItemId'>";

    for (var i = 0; i < newItemList.length; i++) {
        selectText += "<option value=" + newItemList[i].Id + ">" + newItemList[i].Title + "</option>";
    }

    selectText += "</select>";

    var dropDownList = $(selectText);
    $("#supplierItems").replaceWith(dropDownList);
}

function addSupplier() {
    var newSupplierId = 0;
    console.log(isSupplierDialogOpen);
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
