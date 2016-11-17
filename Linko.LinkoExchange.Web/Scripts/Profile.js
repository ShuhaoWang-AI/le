
$(document).ready(function () {

    $(".editabledDiv input").attr("readonly", true);
    $(".editabledDiv select").attr("readonly", "disabled");

    $(".profileDiv input[type='submit'").hide();
    $("#kbq-panel input[type='text'").hide();
 
    $(document).on('click', ".edit-button", function () {
        $(this).hide();
        $("#kbq-panel input[type='password'").hide();
        $("#kbq-panel input[type='text'").show();
        $(this).next(".profileDiv input[type='submit']").show();
        $(this).closest(".box").find(".editabledDiv input").prop('readonly', function (i, value) {
            return !value;
        })
        $(this).closest(".box").find(".editabledDiv select").attr('readonly', null);
        $(this).closest(".box").find(".editabledDiv select").attr('disabled', null);
    })

    $(document).on("click", "#kbqSubmit", function () {
        $("#kbq-panel input[type='password'").show();
        $("#kbq-panel input[type='text'").hide();
    }) 
});