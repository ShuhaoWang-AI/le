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
            type: "POST"
            , url: postUrl
            , data: JSON.stringify({ returnUrl: returnUrl, items: selection })
            , dataType: "JSON"
            , contentType: "application/json; charset=utf-8"
            , success: function(returnData)
            {
                if ($.trim(returnData.message).length > 0)
                {
                    $("body")
                        .append('<div class="popup" data-popup="popup-1">'
                            + '<div class="popup-inner">'
                            + "<h4>"
                            + returnData.message
                            + "</h4>"
                            + '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>'
                            + "</div>"
                            + "</div>");

                    $('[data-popup="popup-1"]').fadeIn(350);
                    $('[data-popup="popup-1"]').delay(2500);
                    $('[data-popup="popup-1"]').fadeOut(350);

                    $("[data-popup-close]")
                        .on("click"
                            , function(e)
                            {
                                $('[data-popup="popup-1"]').fadeOut(350);
                            });
                }

                setTimeout(function()
                    {
                        if (returnData.redirect)
                        {
                            //$(location).attr("href", returnData.newurl);
                            window.location.replace(returnData.newurl);
                        }
                    }
                    , 500);
            }
        });
    }
    else
    {
        $("body")
            .append('<div class="popup" data-popup="popup-1">'
                + '<div class="popup-inner">'
                + "<h4>"
                + noSelectionMessage
                + "</h4>"
                + '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>'
                + "</div>"
                + "</div>");

        $('[data-popup="popup-1"]').fadeIn(350);
        $('[data-popup="popup-1"]').fadeOut(3000);

        $("[data-popup-close]")
            .on("click"
                , function(e)
                {
                    $('[data-popup="popup-1"]').fadeOut(350);
                });
    }
};

$(document)
    .on("ajaxError"
        , function(event, jqXHR)
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
        $.each(e.errors
            , function(key, value)
            {
                if ("errors" in value)
                {
                    $.each(value.errors
                        , function()
                        {
                            message += this + "\n";
                        });
                }
            });

        // Display the message.
        showPopupMessage(message);

        // Cancel the changes.
        var grid = $("[name^='grid']").data("kendoGrid");
        grid.cancelChanges();
    }
};
showPopupMessage = function(message)
{
    $("body")
        .append('<div class="popup" data-popup="popup-1">'
            + '<div class="popup-inner">'
            + "<h4>"
            + message
            + "</h4>"
            + '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>'
            + "</div>"
            + "</div>");

    $('[data-popup="popup-1"]').fadeIn(350);

    $("[data-popup-close]")
        .on("click"
            , function(e)
            {
                $('[data-popup="popup-1"]').fadeOut(350);
            });
};
$(document)
    .ready(function()
    {
        $("#LinkoExchangeForm")
            .submit(function(e)
            {
                var $form = $(this);
                if ($form.valid())
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
            .on("keyup blur"
                , function()
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