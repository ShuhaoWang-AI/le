﻿
$(document).ready(function () {
    $("input[type='checkbox'").prop('checked', true);
    if (registrationType == 'ReRegistration') {
        // User can not change any thing 
        $("input").attr("readonly", true);
        $("input[type='checkbox'").prop('checked', true);
        $(".editabledDiv select").attr("readonly", "disabled");

    }
    else if (registrationType == 'ResetRegistration') {
        // Everything should be editable except email and username
        $("#UserProfile_Email").attr("readonly", true);
        $("#UserProfile_UserName").attr("readonly", true);

    } else if (registrationType == 'NewRegistration') {
        // All inputs are open to user
    }

    $(".profileDiv input[type='submit'").hide();

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
  

    $(document).on("click", "#register", function () {

        
    })

});