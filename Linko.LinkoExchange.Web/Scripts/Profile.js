
$(document).ready(function () {

    $(".editabledDiv  input").attr("readonly", true);
    $(".editabledDiv select").attr("readonly", "disabled");

    $("input[type='submit'").hide();
 
    $(document).on('click', ".edit-button", function () {
        $(this).hide();
        $(this).next("input[type='submit']").show();

        $(this).closest(".box").find(".editabledDiv input").prop('readonly', function (i, value) {
            return !value;
        })
        $(this).closest(".box").find(".editabledDiv select").attr('readonly', null);
    })

    $(document).on("click", "#kbqSubmit", function () {
        var data = $("form").serialize();
        console.log(data);
    }) 
});