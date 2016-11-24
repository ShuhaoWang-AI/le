
$(document).ready(function () {

    $(".editabledDiv input").attr("readonly", true);
    $(".editabledDiv select").attr("readonly", "disabled");

    $(".profileDiv input[type='submit'").hide();
    $("#kbq-panel input[type='text'").hide();

    $(".box-primary").addClass("collapsed-box");
    $(".box-primary >.box-header").find("i.fa.fa-minus").removeClass('fa-minus');
    $(".box-primary >.box-header").find("i.fa").addClass('fa-plus');

    if (profileCollapsed.toLowerCase() == 'false') {
        $("#user-info-panel .box-primary").removeClass("collapsed-box");
        $("#user-info-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass('fa-plus');
        $("#user-info-panel .box-primary >.box-header").find("i.fa").addClass('fa-minus');
    }
    if (kbqCollapsed.toLowerCase() == 'false') {
        $("#kbq-panel .box-primary").removeClass("collapsed-box");
        $("#kbq-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass('fa-plus');
        $("#kbq-panel .box-primary >.box-header ").find("i.fa").addClass('fa-minus');
    }
    if (sqCollapsed.toLowerCase() == "false") {
        $("#sq-panel .box-primary").removeClass("collapsed-box");
        $("#sq-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass('fa-plus');
        $("#sq-panel .box-primary >.box-header").find("i.fa").addClass('fa-minus');
    }

    $(document).find("#kbq-panel select").on("change", function () {
        $(this).closest(".form-group").next().find("input[type=text]").val("");
    })

    $(document).find("#sq-panel select").on("change", function () {
        $(this).closest(".form-group").next().find("input[type=text]").val("");
    })

    $(document).on('click', ".edit-button", function () {
        $(this).hide();
        if ($(this).attr("id") == 'kbqEditBtn') {
            $(this).closest(".box").find(".kbq-answer").val("");
            $("#kbq-panel input[type='text'").show();
            $("#kbq-panel input[type='password'").hide();
            $("#kbq-panel input[type='text'").show();
            $("#kbq-panel input[type='text'").val("");
        }

        $(this).next(".profileDiv input[type='submit']").show();
        $(this).closest(".box").find(".editabledDiv input").prop('readonly', function (i, value) {
            return !value;
        })
        $(this).closest(".box").find(".editabledDiv select").attr('readonly', null);
        $(this).closest(".box").find(".editabledDiv select").attr('disabled', null);
    })

    $(document).on("click", "#profileSubmit", function () {
        $("input[id=profileCollapsed").val("false");
        var collapsedSQ = $("#sq-panel .box-primary.collapsed-box");
        if (!collapsedSQ || collapsedSQ.length < 0) {
            $("input[id=sqCollapsed").val("false");
        } else {
            $("input[id=sqCollapsed").val("true");
        }

        var collapsedKBQ = $("#kbq-panel .box-primary.collapsed-box");
        if (!collapsedKBQ || collapsedKBQ.length < 0) {
            $("input[id=kbqCollapsed").val("false");
        } else {
            $("input[id=kbqCollapsed").val("true");
        }
    })

    $(document).on("click", "#kbqSubmit", function () {
        $("#kbq-panel input[type='password'").show();
        $("#kbq-panel input[type='text'").hide();

        $("input[id=kbqCollapsed").val("false");

        var collapsedSQ = $("#sq-panel .box-primary.collapsed-box");
        if (!collapsedSQ || collapsedSQ.length < 1) {
            $("input[id=sqCollapsed").val("false");
        } else {
            $("input[id=sqCollapsed").val("true");
        }

        var profileCollapsed = $("#user-info-panel .box-primary.collapsed-box");
        if (!profileCollapsed || profileCollapsed.length < 1) {
            $("input[id=profileCollapsed").val("false");
        } else {
            $("input[id=profileCollapsed").val("true");
        }
    })

    $(document).on("click", "#sqSubmit", function () {
        $("input[id=sqCollapsed").val("false");
        var collapsedKBQ = $("#kbq-panel .box-primary.collapsed-box");
        if (!collapsedKBQ || collapsedKBQ.length < 1) {
            $("input[id=kbqCollapsed").val("false");
        } else {
            $("input[id=kbqCollapsed").val("true");
        }

        var profileCollapsed = $("#user-info-panel .box-primary.collapsed-box");
        if (!profileCollapsed || profileCollapsed.length < 1) {
            $("input[id=profileCollapsed").val("false");
        } else {
            $("input[id=profileCollapsed").val("true");
        }
    })

});