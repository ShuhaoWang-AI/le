doAjaxAction = function(grid, postUrl, returnUrl, noSelectionMessage)
{
    var selection = [];

    grid.select()
        .each(
            function()
            {
                var dataItem = grid.dataItem($(this));
                selection.push(dataItem);
            }
        );

    if (selection.length > 0)
    {
        $.ajax({
            type: "POST",
            url: postUrl,
            data: JSON.stringify({ returnUrl: returnUrl, items: selection }),
            dataType: "JSON",
            contentType: "application/json; charset=utf-8",
            success: function(returnData)
            {
                if ($.trim(returnData.message).length > 0)
                {
                    $("body")
                        .append('<div aria-labelledby="Ajax Return Message" class="fade modal modal-info" id="AjaxReturnDataMessageModal" role="dialog" tabindex="-1">'
                            + '<div class="modal-dialog" role="document">'
                            + '<div class="alert alert-dismissible alert-info modal-content">'
                            + '<button aria-label="Close" class="close" data-dismiss="modal" type="button">&times;</button>'
                            + '<h4 class="box-title"><i class="fa fa-info icon"></i> Info</h4>'
                            + '<div class="form-horizontal">'
                            + "<p>"
                            + returnData.message
                            + "</p>"
                            + "</div>"
                            + "</div>"
                            + "</div>"
                            + "</div>");

                    $("#AjaxReturnDataMessageModal").modal("toggle");
                }

                setTimeout(function()
                {
                    if (returnData.redirect)
                    {
                        window.location.replace(returnData.newurl);
                    }
                }, 500);
            }
        });
    }
    else
    {
        $("body")
            .append('<div aria-labelledby="No Selection Message" class="fade modal modal-info" id="NoSelectionMessageModal" role="dialog" tabindex="-1">'
                + '<div class="modal-dialog" role="document">'
                + '<div class="alert alert-dismissible alert-danger modal-content">'
                + '<button aria-label="Close" class="close" data-dismiss="modal" type="button">&times;</button>'
                + '<h4 class="box-title">Error</h4>'
                + '<div class="form-horizontal">'
                + "<p>"
                + noSelectionMessage
                + "</p>"
                + "</div>"
                + "</div>"
                + "</div>"
                + "</div>");

        $("#NoSelectionMessageModal").modal("toggle");
    }
};

$(document)
    .on("ajaxError", function(event, jqXHR)
    {
        var json_response = jqXHR.getResponseHeader("X-Responded-JSON");

        if (json_response)
        {
            var json_response_obj = JSON.parse(json_response);

            if (json_response_obj.status == 401)
            {
                $(location).attr("href", json_response_obj.headers.location); //login URL
                return;
            }
        }
    });

error_handler = function(e)
{
    if (e.errors)
    {
        var message = "There are some errors:\n";
        // Create a message containing all errors.
        $.each(e.errors, function(key, value)
        {
            if ("errors" in value)
            {
                $.each(value.errors, function()
                {
                    message += this + "\n";
                });
            }
        });

        // Display the message.
        showPopupMessage(message);

        // Cancel the changes.
        var grid = $(this.gridId).data("kendoGrid");
        grid.cancelChanges();
    }
};

showPopupMessage = function(message)
{
    $("body")
        .append('<div aria-labelledby="Error Handler Message" class="fade modal modal-info" id="ErrorHandlerModal" role="dialog" tabindex="-1">'
            + '<div class="modal-dialog" role="document">'
            + '<div class="alert alert-dismissible alert-danger modal-content">'
            + '<button aria-label="Close" class="close" data-dismiss="modal" type="button">&times;</button>'
            + '<h4 class="box-title">Error</h4>'
            + '<div class="form-horizontal">'
            + "<p>"
            + message
            + "</p>"
            + "</div>"
            + "</div>"
            + "</div>"
            + "</div>");

    $("#ErrorHandlerModal").modal("toggle");
};

$(document)
    .ready(function()
    {
        setTelephoneMask();

        $("#LinkoExchangeForm")
            .submit(function(e)
            {
                var isFormValid = true;

                try
                {
                    isFormValid = $(this).valid();
                }
                catch (e)
                {}

                if (isFormValid)
                {
                    $("input[type='submit']").prop("disabled", true);
                    $("input[type='button']").prop("disabled", true);
                    //setTimeout(function()
                    //    {
                    //        $("input[type='submit']").prop('disabled', false);
                    //        $("input[type='button']").prop('disabled', false);
                    //    }
                    //    , 2000);
                }
            });

        // Get all textareas that have a "maxlength" property.
        $("textarea[maxlength]")
            .on("keyup blur", function()
            {
                //Counts all the newline characters (\r = return for macs, \r\n for Windows, \n for Linux/unix)
                var newLineCharacterRegexMatch = /\r?\n|\r/g;

                // Store the maxlength and value of the field.
                var maxlength = $(this).attr("maxlength");
                var val = $(this).val();

                //count newline characters
                var regexResult = val.match(newLineCharacterRegexMatch);
                var newLineCount = regexResult ? regexResult.length : 0;

                //replace newline characters with nothing
                var replacedValue = val.replace(newLineCharacterRegexMatch, "");

                //return the length of text without newline characters + doubled newline character count
                var length = replacedValue.length + (newLineCount * 2);

                // Trim the field if it has content over the maxlength.
                if (length > maxlength)
                {
                    $(this).val(val.slice(0, val.length - (length - maxlength)));
                }
            });
    });

$(document)
    .keypress(function(e)
    {
        // http://www.itorian.com/2012/07/stop-from-submission-posting-on-enter.html
        var evt = (e) ? e : ((event) ? event : null);
        var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null);

        if (evt.which === 13 && node.type.toLowerCase() !== "textarea" && node.type.toLowerCase() !== "button" && node.type.toLowerCase() !== "submit")
        {
            return false;
        }
        return true;
    });


if (!String.prototype.startsWith)
{
    String.prototype.startsWith = function(searchString, position)
    {
        position = position || 0;
        return this.indexOf(searchString, position) === position;
    };
}

function disableAutoComplete()
{
    $("input[type='text'").attr("autocomplete", "off");
    $("input[type='tel'").attr("autocomplete", "off");
}

function setTelephoneMask()
{
    init("tel");

    function init(eleType)
    {
        var key = "input[type='" + eleType + "'";
        $(key)
            .each(function(idx, obj)
            {
                obj.onkeypress = digitalKeysInit;
                obj.onkeyup = phoneNumberMask;
                obj.change = phoneNumberMask;
                $(obj).val(obj.value).change();
            });
    }

    function digitalKeysInit(keyEvent)
    {
        var ret;
        if ((keyEvent.charCode >= 48 && keyEvent.charCode <= 57)
            || keyEvent.keyCode === 9
            || keyEvent.keyCode === 10
            || keyEvent.keyCode === 13
            || keyEvent.keyCode === 8
            || keyEvent.keyCode === 116
            || keyEvent.keyCode === 46
            || (keyEvent.keyCode <= 40 && keyEvent.keyCode >= 37))
        {
            ret = true;
        }
        else
        {
            ret = false;
        }
        return ret;
    }

    function phoneNumberMask()
    {
        var digitalMask10 = "(XXX) XXX-XXXX";
        var digitalMask7 = "XXX-XXXX";

        var textInput = this.value;
        var telephoneNumbers = [];
        var formatOutPut = "";

        for (var i = 0; i < textInput.length; i++)
        {
            if (!isNaN(textInput.charAt(i)) && textInput.charAt(i) !== " ")
            {
                telephoneNumbers.push(textInput.charAt(i));
            }
        }

        // format the string
        if (telephoneNumbers.length === 7)
        {
            for (var j = 0; j < digitalMask7.length; j++)
            {
                if (digitalMask7.charAt(j) === "X")
                {
                    if (telephoneNumbers.length === 0)
                    {
                        formatOutPut = formatOutPut + digitalMask7.charAt(j);
                    }
                    else
                    {
                        formatOutPut = formatOutPut + telephoneNumbers.shift();
                    }
                }
                else
                {
                    formatOutPut = formatOutPut + digitalMask7.charAt(j);
                }
            }
        }
        else if (telephoneNumbers.length === 10)
        {
            for (var j = 0; j < digitalMask10.length; j++)
            {
                if (digitalMask10.charAt(j) === "X")
                {
                    if (telephoneNumbers.length === 0)
                    {
                        formatOutPut = formatOutPut + digitalMask10.charAt(j);
                    }
                    else
                    {
                        formatOutPut = formatOutPut + telephoneNumbers.shift();
                    }
                }
                else
                {
                    formatOutPut = formatOutPut + digitalMask10.charAt(j);
                }
            }
        }
        else
        {
            formatOutPut = telephoneNumbers.join("");
        }

        this.value = formatOutPut;
    }
}