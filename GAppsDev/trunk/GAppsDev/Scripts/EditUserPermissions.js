var removedItems = new Array();

$(function () {

});

function addItem() {
    var permissionId = $("#PermissionsList").val();
    var permissionText = $("#PermissionsList option:selected").text();
    var container = $("#PermissionsContainer");

    var existingItems = $(".existingPermissions");
    var nextNumber = existingItems.length;

    var itemExists = false;

    for (var i = 0; i < existingItems.length; i++) {
        if ($(existingItems[i]).val() == permissionId) {
            itemExists = true;
        }
    }

    var existingRemovedItem = getRemovedItem(permissionId);
    if (existingRemovedItem == null) {
        if (!itemExists) {
            var newItem = $(
                "<div id='permission-" + nextNumber + "'>" +
                    "<input type='hidden' class='isActiveField' id='isActiveField-" + nextNumber + "' name='UserPermissions[" + nextNumber + "].IsActive' value='true' />" +
                    "<input type='hidden' class='existingPermissions' id='permissionField-" + nextNumber + "' name='UserPermissions[" + nextNumber + "].Permission.Id' value='" + permissionId + "' />" +
                    "<span>" + permissionText + "<\span>" +
                    "<input type='button'  value='" + local.Delete + "' onClick='removeItem(" + nextNumber + ") '/>" +
                "</div>"
                );

            container.append(newItem);
        }
    }
    else {
        unRemove(permissionId);
    }
}

function removeItem(permissionIndex) {
    var container = $("#permission-" + permissionIndex);
    var isActiveField = $("#isActiveField-" + permissionIndex);
    var isActive = isActiveField.val();
    var PermissionId = $("#permissionField-" + permissionIndex).val();
    isActiveField.val("false");

    var existingRemovedItem = getRemovedItem(PermissionId);
    if (existingRemovedItem == null) {
        var newRemovedItem = { id: PermissionId, oldItem: container };
        removedItems[removedItems.length] = newRemovedItem;

        container.toggle(0);
    }
}

function unRemove(permissionId) {
    var existingRemovedItem = getRemovedItem(permissionId);
    if (existingRemovedItem != null) {
        existingRemovedItem.oldItem.toggle(0);
        existingRemovedItem.oldItem.find(".isActiveField").val("true");

        var itemIndex = null;
        var newRemovedList = new Array();

        for (var i = 0; i < removedItems.length; i++) {

            if (removedItems[i] != existingRemovedItem) {
                newRemovedList[newRemovedList.length] = removedItems[i];
            }
        }

        removedItems = newRemovedList;
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

