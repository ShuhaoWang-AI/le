
$(document).ready(function () {

    $("#searchEmailBtn").click(function () {
        $.post("./InviteCheckEmail",
        {
            emailAddress: $("#EmailAddress").val()
        },
        function (data, status) {
            //alert("Data: " + data + "\nStatus: " + status);

            $("#EmailAddress").prop('readonly', true);

            if (data.ExistingUsers == null) {
                if (data.FirstName == null && data.LastName == null) {
                    $("#noUserFoundMessage").show();
                    $("#divNameContainer").show();
                    $("#cancelInviteBtn").show();
                    $("#sendInviteBtn").show();
                    $("#searchEmailBtn").hide();
                }
                else {
                    $("#foundUserMessage").html(data.DisplayMessage);
                    //$("#OrgRegProgramUserId").val(data.OrgRegProgramUserId);
                    $("[id=FirstName]").val(data.FirstName);
                    $("[id=LastName]").val(data.LastName);
                    $("#FacilityName").val(data.FacilityName);
                    $("#PhoneNumber").val(data.PhoneNumber);

                    $("#boxFoundUser").show();

                    if (data.IsExistingProgramUser) {
                        $("#foundUserInviteButton").hide();
                    }
                    else {
                        $("#foundUserInviteButton").show();
                    }

                }
            }
            else {
                $("#searchEmailBtn").hide();
                $("#boxFoundUser").hide();
                $("#boxFoundUsers").show();

            }

        });
    });

    $("[id=cancelInviteBtn]").click(function () {
        $("#sendInviteBtn").hide();
        $("#EmailAddress").prop('readonly', false);
        $("#cancelInviteBtn").hide();
        $("#divNameContainer").hide();
        $("#searchEmailBtn").show();
        $("#EmailAddress").val(null);
        $("#noUserFoundMessage").hide();
        $("#boxFoundUser").hide();
        $("#boxFoundUsers").hide();
        clearFields();
    });

    function clearFields() {
        $("#foundUserMessage").html("");
        //$("#OrgRegProgramUserId").val(null);
        $("[id=FirstName]").val(null);
        $("[id=LastName]").val(null);
        $("#FacilityName").val(null);
        $("#PhoneNumber").val(null);
    }

});