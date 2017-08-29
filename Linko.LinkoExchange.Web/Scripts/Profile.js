$(document).ready(function () {

    $(".editabledDiv input").attr("readonly", true);
    $(".editabledDiv select").attr("readonly", "disabled");

    $("#user-info-panel input[type='password'").val("Tiger12345");
    $(".profileDiv input[type='submit'").hide();
    $("#kbq-panel input[type='text'").hide(); 

    $(".box-primary").addClass("collapsed-box");
    $(".box-primary >.box-header").find("i.fa.fa-minus").removeClass('fa-minus');
    $(".box-primary >.box-header").find("i.fa").addClass('fa-plus');

    if (profileCollapsed.toLowerCase() === 'false') {
        $("#user-info-panel .box-primary").removeClass("collapsed-box");
        $("#user-info-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass('fa-plus');
        $("#user-info-panel .box-primary >.box-header").find("i.fa").addClass('fa-minus');
    }
    if (kbqCollapsed.toLowerCase() === 'false') {
        $("#kbq-panel .box-primary").removeClass("collapsed-box");
        $("#kbq-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass('fa-plus');
        $("#kbq-panel .box-primary >.box-header ").find("i.fa").addClass('fa-minus');
    }
    if (sqCollapsed.toLowerCase() === "false") {
        $("#sq-panel .box-primary").removeClass("collapsed-box");
        $("#sq-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass('fa-plus');
        $("#sq-panel .box-primary >.box-header").find("i.fa").addClass('fa-minus');
    }

    $(document).find("#kbq-panel select").on("change", function () {
        $(this).closest(".form-group").next().find("input[type=text]").val("");
    });

    $(document).find("#sq-panel select").on("change", function () {
        $(this).closest(".form-group").next().find("input[type=text]").val("");
    });

    $(document).on('click', ".edit-button", function () {
        $(this).hide();
        if ($(this).attr("id") === 'kbqEditBtn') {
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
    });

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
    });

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
    });

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
    });

    kbqUpdateInit();
    
    // implementations 
    function kbqUpdateInit() {
        $('.fa-save').hide();
        $('.fa-edit').on('click', clickEdit);
        $('.fa-save').on('click', clickSave);
        $('.fa-undo').on('click', clickCancel);

        setKbqAnswerEntryKey();
    }

    function setKbqAnswerEntryKey() {
        var qaDivs = $("#kbq-panel").find(".kbq-div");
        qaDivs.map(function (idx, ele) {
            $(ele).on("keyup", function (evt) {
                if (evt.which === 13) {
                    if ($(evt.target).is(".text-box")) {
                        clickSave.apply($(ele).find('.fa-save'));
                    } else if ($(evt.target).is(".fa-edit")) {
                        clickEdit.apply(evt.target);
                    } 
                }
            });
        });
    };

    function clickCancel() {
        var qaDiv = $(this).closest(".kbq-div"); 
        qaDiv.find("select").attr('readonly', "disabled");
        qaDiv.find("select").attr("disabled", "true");
        qaDiv.find("input[type='text']").attr('disabled', "disabled");
        qaDiv.find("input[type='password']").attr('disabled', "disabled");

        qaDiv.find("input[type='password']").show();
        qaDiv.find("input[type='text']").hide();

        $(this).hide();
        $(this).siblings(".fa-save").hide();
        $(this).siblings(".fa-edit").show();
    }

    function clickSave() {

        var qaDiv = $(this).closest(".kbq-div");
        var questionIndex = qaDiv.find("select")[0].selectedIndex + 1;
        var answer = qaDiv.find("input[type='text']")[0].value;
        var questionAnswerId = qaDiv.find("input[type='hidden']")[0].value;

        if (!answer) {
            return;
        }

        qaDiv.find(".fa-undo").hide();
        qaDiv.find(".fa-spinner").show();

        // update this question, and value   
        var qa = {
            questionId: questionIndex,
            questionAnswerId: questionAnswerId,
            content: answer
        };

        $.post("/User/Profile/UpdateOneKbq", qa, function (data) {
            qaDiv.find(".fa-spinner").hide();
            if (data && data.result === "true") {

                $('.fa-save').hide();
                $('.fa-save').siblings('.fa-edit').show();
                $('.fa-undo').hide(); 

                $("#summaryDiv").empty();

                var succeedDiv = $("#succeedPanel").clone();
                succeedDiv.find('ul').append("<li>" + data.message + "</li>");
                $('#summaryDiv').append(succeedDiv);
                succeedDiv.show();

                // disable the list the text
                qaDiv.find("select").attr('readonly', "disabled");
                qaDiv.find("select").attr("disabled", "true");

                qaDiv.find("input[type='password']").show();
                qaDiv.find("input[type='text']").hide();

            } else {
                // display the error
                $("#summaryDiv").empty();
                var errorPanel = $("#errorPanel").clone();
                errorPanel.find('ul').append("<li>" + data.message + "</li>");
                $('#summaryDiv').append(errorPanel);
                errorPanel.show();
            }
        });
    }

    function clickEdit() {
        var qaDiv = $(this).closest(".kbq-div");

        qaDiv.find("select").attr('readonly', null);
        qaDiv.find("select").attr('disabled', null);

        qaDiv.find("input[type='password']").hide();
        qaDiv.find("input[type='text']").val("");

        qaDiv.find("input[type='text']").show();
        qaDiv.find("input[type='text']").prop('readonly', function (i, value) {
            return !value;
        });
        qaDiv.find("input[type='text']").removeAttr("readonly");
        qaDiv.find("input[type='text']").removeAttr("disabled");
        $(this).hide();
        $(this).siblings('.fa-save').show();
        $(this).siblings('.fa-undo').show();
    }

    $(document).on('change', "select", function () {
        var i = $(this)[0].selectedIndex;
        var ch = $(this).children().each(function (index) {
            $(this).prop('selected', index === i);
            if (index === i) {
                $(this).attr('selected', 'selected');
            } else {
                $(this).removeAttr('selected');
            }
        });
    });

});
