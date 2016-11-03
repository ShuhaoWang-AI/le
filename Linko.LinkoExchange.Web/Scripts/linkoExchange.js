doAjaxAction = function (grid, postUrl, returnUrl, noSelectionMessage)
{
    var selection = [];

    grid.select().each(
        function ()
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
            success: function (returnData)
            {
                if ($.trim(returnData.message).length > 0)
                {
                    $('body').append('<div class="popup" data-popup="popup-1">' +
                                        '<div class="popup-inner">' +
                                            '<h4>' + returnData.message + '</h4>' +
                                            '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>' +
                                        '</div>' +
                                    '</div>');

                    $('[data-popup="popup-1"]').fadeIn(350);
                    $('[data-popup="popup-1"]').delay(2500);
                    $('[data-popup="popup-1"]').fadeOut(350);

                    $('[data-popup-close]').on('click', function (e)
                    {
                        $('[data-popup="popup-1"]').fadeOut(350);
                    });
                }

                setTimeout(function ()
                {
                    if (returnData.redirect)
                    {
                        //$(location).attr("href", returnData.newurl);
                        window.location.replace(returnData.newurl);
                    }
                }, 500);
            }
        });
    }
    else
    {
        $('body').append('<div class="popup" data-popup="popup-1">' +
                                        '<div class="popup-inner">' +
                                            '<h4>' + noSelectionMessage + '</h4>' +
                                            '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>' +
                                        '</div>' +
                                    '</div>');

        $('[data-popup="popup-1"]').fadeIn(350);
        $('[data-popup="popup-1"]').fadeOut(3000);

        $('[data-popup-close]').on('click', function (e)
        {
            $('[data-popup="popup-1"]').fadeOut(350);
        });
    }
};

$(document).on('ajaxError', function (event, jqXHR)
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

error_handler = function (e)
{
    if (e.errors)
    {
        var message = "Errors:\n";
        $.each(e.errors, function (key, value)
        {
            if ('errors' in value)
            {
                $.each(value.errors, function ()
                {
                    message += this + "\n";
                });
            }
        });

        showPopupMessage(message);
    }
}

showPopupMessage = function (message)
{

    $('body').append('<div class="popup" data-popup="popup-1">' +
                        '<div class="popup-inner">' +
                            '<h4>' + message + '</h4>' +
                            '<a class="popup-close" data-popup-close="popup-1" href="#">x</a>' +
                        '</div>' +
                    '</div>');

    $('[data-popup="popup-1"]').fadeIn(350);

    $('[data-popup-close]').on('click', function (e)
    {
        $('[data-popup="popup-1"]').fadeOut(350);
    });
}
