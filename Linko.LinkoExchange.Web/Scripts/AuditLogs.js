
$(document).ready(function ()
{
    $("#grid .k-filter-row").hide();

    var exportFlag = false;
    var grid = $("#grid").data("kendoGrid");
    grid.bind("excelExport", function(e) {
        if (!exportFlag) {
            e.sender.showColumn("CromerrAuditLogId");
            e.sender.showColumn("RegulatorName");
            e.sender.showColumn("OrganizationName");
            e.sender.showColumn("RegulatoryProgramName");
            e.sender.showColumn("UserProfileId");
            e.sender.showColumn("UserName");
            e.sender.showColumn("FirstName");
            e.sender.showColumn("LastName");
            e.sender.showColumn("IPAddress");
            e.sender.showColumn("HostName");
            e.sender.showColumn("Comment");

            e.preventDefault();
            exportFlag = true;
            setTimeout(function () {
                e.sender.saveAsExcel();
            });
        } else {
            e.sender.hideColumn("CromerrAuditLogId");
            e.sender.hideColumn("RegulatorName");
            e.sender.hideColumn("OrganizationName");
            e.sender.hideColumn("RegulatoryProgramName");
            e.sender.hideColumn("UserProfileId");
            e.sender.hideColumn("UserName");
            e.sender.hideColumn("FirstName");
            e.sender.hideColumn("LastName");
            e.sender.hideColumn("IPAddress");
            e.sender.hideColumn("HostName");
            e.sender.hideColumn("Comment");

            exportFlag = false;
        }

        //Uncomment this for color formatting in Excel worksheet.
        //var sheet = e.workbook.sheets[0];
        //for (var rowIndex = 0; rowIndex < sheet.rows.length; rowIndex++) {
        //    var row = sheet.rows[rowIndex];
        //    if (rowIndex == 0) {
        //        for (var cellIndex = 0; cellIndex < row.cells.length; cellIndex++) {
        //            row.cells[cellIndex].background = "#0070C0";
        //            row.cells[cellIndex].color = "#FFFFFF";
        //            row.cells[cellIndex].bold = true;
        //        }
        //    }
 
        //    if (rowIndex > 0 && rowIndex % 2 == 0) {
        //        for (var cellIndex = 0; cellIndex < row.cells.length; cellIndex++) {
        //            row.cells[cellIndex].background = "#DCE6F1";
        //        }
        //    }
        //}            
    });
});

$("[name='onOffSwitchSearchControl']").bootstrapSwitch();
$("[name='onOffSwitchSearchControl']").on('switchChange.bootstrapSwitch', function (event, state)
{
    $("[id='basicSearch']").toggle(500);
    $("#grid .k-filter-row").toggle(500);

    //console.log($("#grid .k-filter-row")); // DOM element
    //console.log(this); // DOM element
    //console.log(event); // jQuery event
    //console.log(state); // true | false
});

getLogEntryDetails = function ()
{
    var grid = $("#grid").data("kendoGrid");
    var selecedItem = grid.dataItem(grid.select());
    $('#details-cell-comment').text(selecedItem.Comment);
    $('#details-cell-username').text(selecedItem.UserName);
    $('#details-cell-eventcategory').text(selecedItem.EventCategory);
    $('#details-cell-userid').text(selecedItem.UserProfileId);
    $('#details-cell-eventtype').text(selecedItem.EventType);
    $('#details-cell-logdatetime').text(selecedItem.LogDateTimeUtcDetailString);
    $('#details-cell-emailaddress').text(selecedItem.EmailAddress);
    $('#details-cell-eventcode').text(selecedItem.CromerrAuditLogId);
    $('#details-cell-fullname').text(selecedItem.FirstName + ' ' + selecedItem.LastName);
    $('#details-cell-authority').text(selecedItem.RegulatorName);
    $('#details-cell-ipaddress').text(selecedItem.IPAddress);
    $('#details-cell-facility').text(selecedItem.OrganizationName);
    $('#details-cell-ipname').text(selecedItem.HostName);
    $('#details-cell-regulatoryprogramname').text(selecedItem.RegulatoryProgramName);
            
    //Don't fetch from server unless grid performance is poor
    //var postUrl = "/Authority/AuditLogs_Select";
    //doAjaxGetLogDetails(grid, postUrl, "", "Select a log entry");
}

doAjaxGetLogDetails = function (grid, postUrl, returnUrl, noSelectionMessage) {
    var cromerrAuditLogId = -1;

    grid.select().each(
        function () {
            var dataItem = grid.dataItem($(this));
            cromerrAuditLogId = dataItem.CromerrAuditLogId;
        }
    );

    if (cromerrAuditLogId != -1) {
        $.ajax({
            type: "POST",
            url: postUrl,
            data: JSON.stringify({ returnUrl: returnUrl, cromerrAuditLogId: cromerrAuditLogId }),
            dataType: "JSON",
            contentType: "application/json; charset=utf-8",
            success: function (returnData) {
                if ($.trim(returnData.message).length > 0) {
                    $('body').append('<div class="popup" data-popup="popup-1">' +
                                        '<div class="popup-inner">' +
                                            '<h4>' + returnData.message + '</h4>' +
                                            '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>' +
                                        '</div>' +
                                    '</div>');

                    $('[data-popup="popup-1"]').fadeIn(350);
                    $('[data-popup="popup-1"]').delay(2500);
                    $('[data-popup="popup-1"]').fadeOut(350);

                    $('[data-popup-close]').on('click', function (e) {
                        $('[data-popup="popup-1"]').fadeOut(350);
                    });
                }

                //alert(returnData.details);
                $('#details-cell-comment').text(returnData.details);
            }
        });
    }
    else {
        $('body').append('<div class="popup" data-popup="popup-1">' +
                                        '<div class="popup-inner">' +
                                            '<h4>' + noSelectionMessage + '</h4>' +
                                            '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>' +
                                        '</div>' +
                                    '</div>');

        $('[data-popup="popup-1"]').fadeIn(350);
        $('[data-popup="popup-1"]').fadeOut(3000);

        $('[data-popup-close]').on('click', function (e) {
            $('[data-popup="popup-1"]').fadeOut(350);
        });
    }
};

