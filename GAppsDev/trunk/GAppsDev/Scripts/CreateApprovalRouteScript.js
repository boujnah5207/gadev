var routeStepsList;

$(function () {
    routeStepsList = new Array();

    updateRouteStepsArray();
    updateInsertAfterList();
});

function addStep() {
    var location = $("#insertAfterList option:selected").val();
    var userId = $("#UsersSelectList option:selected").val();
    var userName = $("#UsersSelectList option:selected").text();

    //for (var i = 0; i < routeStepsList.length; i++) {
    //    if (routeStepsList[i].userId == userId) return;
    //}

    addStepToList(location + 1, userId, userName);
    updateForm();
}

function removeStep(index) {
    routeStepsList.splice(index, 1);
    updateForm();
}

function addStepToList(location, userId, userName) {
    var newStep = {
        userId: userId,
        userName: userName
    }

    routeStepsList.splice(location, 0, newStep);
}

function updateForm() {
    updateStepsContainer();
    updateInsertAfterList();
}

function updateStepsContainer() {
    var stepsContainer = $("#StepsContainer");
    var newHtml = "";

    if (routeStepsList.length > 0) {
        for (var i = 0; i < routeStepsList.length; i++) {
            newHtml +=
                "<div id='step-" + i + "' class='existingStep'>" +
                    "<input type='hidden' id='userField-" + i + "' class='userField' name='Steps[" + i + "].UserId' value='" + routeStepsList[i].userId + "' />" +
                    "<input type='hidden' id='userNameField-" + i + "' class='userField' name='Steps[" + i + "].UserName' value='" + routeStepsList[i].userName + "' />" +
                    "<input type='hidden' id='stepField-" + i + "' class='stepField' name='Steps[" + i + "].StepNumber' value='" + (i + 1) + "' />" +
                    "<span>" + local.Step + ": " + (i + 1) + " " + local.OrderApprover + ": " + routeStepsList[i].userName + " </span>" +
                    "<input type='button' value='" + local.Delete + "' onClick='removeStep(" + i + ") '/>" +
                "</div>\n";
        }
    }
    else {
        newHtml += local.RouteHasNoSteps;
    }

    stepsContainer.html(newHtml);
}

function updateRouteStepsArray() {
    var InsertAfterList = $("#insertAfterList");
    var existingSteps = $(".existingStep");

    routeStepsList = new Array();
    for (var i = 0; i < existingSteps.length; i++) {
        var userId = $("#userField-" + i).val();
        var userName = $("#userNameField-" + i).val();

        addStepToList(i, userId, userName);
    }
}

function updateInsertAfterList() {
    var newHtml = "";
    var InsertAfterList = $("#insertAfterList");
    var existingSteps = $(".existingStep");

    newHtml += "<option value='-1'>" + local.ListStartLocation + "</option>\n";
    for (var i = 0; i < existingSteps.length; i++) {
        var stepNumber = $("#stepField-" + i).val();
        var userName = $("#userNameField-" + i).val();

        newHtml += "<option value='" + stepNumber + "'>" + userName + "</option>\n";
    }

    InsertAfterList.html(newHtml);
}