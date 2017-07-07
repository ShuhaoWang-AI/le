 
function onRequestEnd(e) {
    if (e.type && e.type.toLowerCase() === "destroy") {

        if (!e.response.Errors) {
            $("#message").text("Invitation deleted!");
            $("#DeletePendingInvitationResultModal").modal(); 
        } 
    }
};

function confirmDelete(e) {
    e.preventDefault();
    var grid = this;

    $("#DeletePendingInvitationModal").modal();

    $("#YesDelete").click(function () {
        $("#DeletePendingInvitationModal").modal('hide');
        $("#DeletePendingInvitationModal").hide();
        var row = $(e.currentTarget).closest("tr");
        grid.removeRow(row);
    });
} 