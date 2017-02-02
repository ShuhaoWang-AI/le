
$(document).ready(function () {

    $("#sendInviteBtn").click(function () {
        //Prevent empty (just white space) strings
        $("#FirstName").val($("#FirstName").val().trim());
        $("#LastName").val($("#LastName").val().trim());
        var form = $(this).parents('form:first');
        form.submit();
    });

    $("#searchEmailBtn").click(function (event) {
        event.preventDefault();
        if (!$('#EmailAddress').valid())
        {
            return false;
        }

        $.post("/Invite/InviteCheckEmail",
        {
            emailAddress: $("#EmailAddress").val(),
            orgRegProgramIdString: $("#OrgRegProgramId").val()
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
                    $("#BusinessName").val(data.BusinessName);
                    $("#PhoneNumber").val(data.PhoneNumber);

                    $("#boxFoundUser").show();

                    if (data.IsExistingProgramUser) {
                        $("#foundUserInviteButton").hide();
                    }
                    else {
                        $("#foundUserInviteButton").show();
                    }
                    $("#searchEmailBtn").hide();

                }
            }
            else {

                $("#searchEmailBtn").hide();
                $("#boxFoundUser").hide();
                $("#boxFoundUsers").show();
                $("#foundUserInDifferentProgramsMessage").html(data.DisplayMessage);

                $('#FoundUser_FirstName').val(data.ExistingUsers[0].FirstName);
                $('#FoundUser_LastName').val(data.ExistingUsers[0].LastName);
                $('#FoundUser_OrganizationName').val(data.ExistingUsers[0].BusinessName);
                $('#FoundUser_PhoneNumber').val(data.ExistingUsers[0].PhoneNumber);

                var orgRegProgramUserId = data.ExistingUsers[0].OrgRegProgramUserId;
                var invitationType = $('#InvitationType').val();
                var industryOrgRegProgramId = $('#OrgRegProgramId').val();

                var inviteExistingUrl = "/Invite/InviteExistingUser?orgRegProgUserIdString=" + orgRegProgramUserId + "&industryOrgRegProgramId=" + industryOrgRegProgramId + "&invitationType=" + invitationType;
                $('#sendToExistingInviteBtn').attr("formaction", inviteExistingUrl);
            }

            return false;
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
        $("#BusinessName").val(null);
        $("#PhoneNumber").val(null);
    }

});
