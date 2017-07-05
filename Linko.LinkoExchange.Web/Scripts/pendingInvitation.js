function onRequestEnd(e) {
    if (e.type.toLowerCase() === "destroy" && !e.response.Errors) {
        showPopupMessage("Invitation deleted!");
    }
};

function pendingInvitationGridOp(readUrl, deleteUrl) {

    var grid = $("#pendingInvitationGrid").kendoGrid({
        dataSource: {
            batch: true,
            ajax: true,
            sort: { field: "DateInvited", dir: "asc" },
            transport: {
                read: {
                    url: readUrl,
                    type: "GET",
                    dataType: "json",
                    contentType: "application/json; chartset=utf-8"
                },
                destroy: {
                    url: deleteUrl,
                    type: "POST"
                }
            },
            requestEnd: function (e) { 
                if (e.type && e.type.toLowerCase() === "destroy") {
                    if (!e.response.Errors) {
                        $("#message").text("Invitation deleted!");
                    } else {
                        var message = [];
                        for (var key in e.response.Errors) {
                            for (var i = 0; i < e.response.Errors[key].errors.length; i++) {
                                message.push(key + " " + e.response.Errors[key].errors[i]);
                            }
                        }

                        $("#message").text(message.join("\r\n"));
                    }

                    $("#DeletePendingInvitationResultModal").modal();
                }
            },
            pageSize: 10,
            schema: {
                type: "json",
                data: "Data",
                model: {
                    id: "Id",
                    fields: {
                        Id: { editable: false, type: "string" },
                        FirstName: { editable: false, type: "string" },
                        LastName: { editable: false, type: "string" },
                        Email: { type: "string", validation: { required: true, min: 1 } },
                        DateInvited: { type: "string" },
                        InviteExpires: { type: "string", validation: { min: 0, required: true } }
                    }
                }
            }
        },
        autoBind: true,
        sortable: {
            mode: "multiple"
        },
        pageable: {
            pageSizes: true,
            refresh: true,
            input: true,
            numeric: false
        },
        noRecords: {
            template: "<div style='display:inline-block;height:40px; margin-top:20px; text-align: center;'>No pending invitations exist.</div>"
        },

        columns: [
            { field: "FirstName", title: "First Name" },
            { field: "LastName", title: "Last Name" },
            { field: "Email" },
            {
                field: "DateInvited",
                format: "{0:MM/dd/yyyy hh:mm tt}",
                sortable: {
                    initialDirection: "desc"
                }
            },
            { field: "InviteExpires", format: "{0:MM/dd/yyyy hh:mm tt}" },
            {
                command: [
                    {
                        name: "destroy",
                        title: " ",
                        width: 40,
                        template: "<a class='k-grid-destroy k-button'><span class='k-icon k-delete k-i-close'></span></a>",

                        click: function destroy(e) {
                            e.preventDefault();
                            var tr = $(e.target).closest("tr");
                            var data = this.dataItem(tr);

                            $("#DeletePendingInvitationModal").modal();

                            $("#YesDelete").click(function () {
                                $("#DeletePendingInvitationModal").modal('hide');
                                $("#DeletePendingInvitationModal").hide(); 
                                grid.dataSource.remove(data);
                                grid.dataSource.sync();
                            }); 
                        }
                    }
                ]
            }],

        editable: {
            mode: "inline"
        }
    }).data("kendoGrid");
}