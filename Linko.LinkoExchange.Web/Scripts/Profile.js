$(document)
    .ready(function() {

        $(".editabledDiv input").attr("readonly", true);
        $(".editabledDiv select").attr("readonly", "disabled");

        $("#user-info-panel input[type='password'").val("Tiger12345");
        $(".profileDiv input[type='submit'").hide();
        $("#kbq-panel input[type='text'").hide();

        $(".box-primary").addClass("collapsed-box");
        $(".box-primary >.box-header").find("i.fa.fa-minus").removeClass("fa-minus");
        $(".box-primary >.box-header").find("i.fa").addClass("fa-plus");

        if (profileCollapsed.toLowerCase() === "false") {
            $("#user-info-panel .box-primary").removeClass("collapsed-box");
            $("#user-info-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass("fa-plus");
            $("#user-info-panel .box-primary >.box-header").find("i.fa").addClass("fa-minus");
        }
        if (kbqCollapsed.toLowerCase() === "false") {
            $("#kbq-panel .box-primary").removeClass("collapsed-box");
            $("#kbq-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass("fa-plus");
            $("#kbq-panel .box-primary >.box-header ").find("i.fa").addClass("fa-minus");
        }
        if (sqCollapsed.toLowerCase() === "false") {
            $("#sq-panel .box-primary").removeClass("collapsed-box");
            $("#sq-panel .box-primary >.box-header").find("i.fa.fa-plus").removeClass("fa-plus");
            $("#sq-panel .box-primary >.box-header").find("i.fa").addClass("fa-minus");
        }

        $(".question-answer-div")
            .find("select")
            .on("change", function() {

                //check if selected option is duplicated with others 
                var thisOldSelected = $(this).siblings(".lastSelected").val();
                var lastSelecteds = $(this).closest(".editabledDiv").find(".lastSelected");
                var indexes = $.makeArray(lastSelecteds.map(function(e, ele) {
                    return ele.value;
                }));

                var p = indexes.indexOf(thisOldSelected);
                indexes.splice(p, 1);

                var currentIndex = this.options[this.selectedIndex].value;
                var filtered = indexes.filter(function(i) {
                    return i === currentIndex;
                });

                var qaDiv = $(this).closest(".question-answer-div");
                if (filtered.length > 0) {
                    $(this).siblings(".field-validation-error").text("Question cannot be duplicated with others.");
                    qaDiv.find(".fa-save").addClass("disabled");
                    qaDiv.find(".text-box").addClass("disabled");
                    qaDiv.find(".text-box").attr("readonly", "readonly");
                } else {
                    $(this).closest(".form-group").next().find("input[type=text]").val("");
                    $(this).siblings(".field-validation-error").text("");
                    qaDiv.find(".fa-save").removeClass("disabled");
                    $(this).siblings("input").removeClass("disabled");
                    qaDiv.find(".text-box").attr("readonly", null);
                }
            });

        $(document)
            .on("click", ".edit-button", function() {
                $(this).hide();
                if ($(this).attr("id") === "kbqEditBtn") {
                    $(this).closest(".box").find(".kbq-answer").val("");
                    $("#kbq-panel input[type='text'").show();
                    $("#kbq-panel input[type='password'").hide();
                    $("#kbq-panel input[type='text'").show();
                    $("#kbq-panel input[type='text'").val("");
                }

                $(this).next(".profileDiv input[type='submit']").show();
                $(this)
                    .closest(".box")
                    .find(".editabledDiv input")
                    .prop("readonly", function(i, value) {
                        return !value;
                    });
                $(this).closest(".box").find(".editabledDiv select").attr("readonly", null);
                $(this).closest(".box").find(".editabledDiv select").attr("disabled", null);
            });

        $(document)
            .on("click", "#profileSubmit", function() {
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
         
        questionAnswerEventHandlerInit();

        // implementations 
        function questionAnswerEventHandlerInit() {
            $(".fa-save").hide();
            $(".fa-edit").on("click", clickEdit);
            $(".fa-save").on("click", clickSave);
            $(".fa-undo").on("click", clickCancel);

            setKeyEventHandler();
        }

        function setKeyEventHandler() {
            var qaDivs = $(".question-answer-div");
            qaDivs.map(function(idx, ele) {
                $(ele)
                    .on("keyup", function(evt) {
                        if (evt.which === 13) {
                            if ($(evt.target).is(".text-box")) {
                                clickSave.apply($(ele).find(".fa-save"));
                            } else if ($(evt.target).is(".fa-edit")) {
                                clickEdit.apply(evt.target);
                            }
                        } else if ($(evt.target).is(".text-box")) {
                            editChanged.apply(evt.target);
                        }
                    });
            });

            setEditEventHandler();
        }

        function setEditEventHandler() {
            var inputs = $(".question-answer-div").find("input[type='text']");

            inputs.each(function(idx, obj) {
                obj.onchange = editChanged;
                $(obj).val(obj.value).change();
            });
        }
        
        function editChanged() {
            var qaDiv = $(this).closest(".question-answer-div");

            var questionAnswerId = qaDiv.find("#questionAnswerId").val();
            var answerErrorDivId = "#Content" + questionAnswerId + "-error";

            if (!this.value) {
                qaDiv.find(answerErrorDivId).text("Question answer cannot be empty.");
                return;
            } else {
                // check if value is duplicated with others 
                var existingValues = $.makeArray($(this)
                    .closest(".editabledDiv")
                    .find(".lastValue")
                    .map(function(idx, ele) {
                        return ele.value;
                    }));

                var value = this.value;
                var thisExistingValue = $(this).siblings(".lastValue").val();
                var filtered = existingValues.filter(function(i) {
                        return i !== thisExistingValue;
                    })
                    .filter(function(j) {
                        return j === value;
                    });
                if (filtered.length > 0) {
                    $(this).siblings(".field-validation-error").text("Question answer cannot be duplicated with others.");
                    qaDiv.find(".fa-save").addClass("disabled");
                } else {
                    qaDiv.find(answerErrorDivId).text("");
                    qaDiv.find(".fa-save").removeClass("disabled");
                }
            }
        }

        function clickCancel() {
            var qaDiv = $(this).closest(".question-answer-div");
            qaDiv.find("select").attr("readonly", "disabled");
            qaDiv.find("select").attr("disabled", "true");
            qaDiv.find("input[type='text']").attr("disabled", "disabled");
            qaDiv.find("input[type='password']").attr("disabled", "disabled"); 

            // restore the last selected question, and last text box value 
            var lastSelected = qaDiv.find("#lastSelected").val();
            var options = qaDiv.find("select").find("option");
            options.each(function() {
                if (this.value !== lastSelected) {
                    $(this).prop("selected", false);
                } else {
                    $(this).prop("selected", true);
                }
            });

            var password = qaDiv.find("input[type='password']");
            if (password.length > 0) {
                password.show();
                qaDiv.find("input[type='text']").hide();
            } else {
                // restore the textbox lastValue
                var lastValue = qaDiv.find("#lastValue").val();
                qaDiv.find("input[type='text']").val(lastValue);
                qaDiv.find("input[type='text']").show();
            }

            $(this).hide();
            $(this).siblings(".fa-save").hide();
            $(this).siblings(".fa-edit").show();
            qaDiv.find(".field-validation-error").text(""); 
            qaDiv.siblings().find(".fa-edit").removeClass("disabled");
        }

        function clickSave() {

            var qaDiv = $(this).closest(".question-answer-div");
            if ($(this).is(".disabled")) {
                return;
            }

            var select = qaDiv.find("select")[0];
            var options = select.options;
            var questionIndex = options[select.selectedIndex].value;
            var answer = qaDiv.find("input[type='text']")[0].value;
            var questionAnswerId = qaDiv.find("#questionAnswerId").val();

            var questionErrorDivId = "#Question" + questionAnswerId + "-error";
            var answerErrorDivId = "#Content" + questionAnswerId + "-error";

            if (!answer) {
                qaDiv.find(answerErrorDivId).text("Question answer cannot be empty.");
                return;
            }

            qaDiv.find(".fa-spinner").show();

            // update this question, and value   
            var qa = {
                questionId: questionIndex,
                questionAnswerId: questionAnswerId,
                content: answer,
                questionTypeName: "KBQ"
            };

            var inKbq = true;
            var url = "/User/Profile/UpdateQuestionAnswer";
            if ($.contains($("#sq-panel")[0], qaDiv[0])) {
                inKbq = false;
                qa.questionTypeName = "SQ";
            }
            var summaryDiv; 
            $.post(url, qa, function(data) {
                qaDiv.find(".fa-spinner").hide();
                if (data && data.result === "true") {

                    qaDiv.find(".fa-save").hide();
                    qaDiv.find(".fa-save").siblings(".fa-edit").show();
                    qaDiv.find(".fa-undo").hide();

                    $("#succeedPanel").find("ul").empty();
                    var succeedPanel = qaDiv.find(".succeedPanel");
                    if (succeedPanel.length < 1) {
                        succeedPanel = $("#succeedPanel").clone();
                        succeedPanel.removeAttr("id");
                        succeedPanel.addClass("succeedPanel");
                    }

                    succeedPanel.find("ul").append("<li>" + data.message + "</li>");

                    summaryDiv = qaDiv.closest(".box-primary").find(".summaryDiv");
                    summaryDiv.empty();
                    summaryDiv.append(succeedPanel);

                    succeedPanel.show();

                    // disable the list the text
                    qaDiv.find("select").attr("readonly", "disabled");
                    qaDiv.find("select").attr("disabled", "true");
                    qaDiv.find("input[type='text']").attr("readonly", "readonly");
                    if (inKbq) {
                        qaDiv.find("input[type='password']").show();
                        qaDiv.find("input[type='text']").hide();
                    }

                    // update the selected question, and text box value
                    qaDiv.find("#lastValue").val(answer);
                    qaDiv.find("#lastSelected").val(questionIndex);
                    qaDiv.siblings().find(".fa-edit").removeClass("disabled");

                } else {
                    // display the error
                    var errorPanel = qaDiv.find(".errorPanel");
                    if (errorPanel.length < 1) {
                        errorPanel = $("#errorPanel").clone();
                        errorPanel.removeAttr("id");
                        errorPanel.addClass("errorPanel");
                    }

                    var errors = data.answerErrors.concat(data.questionErrors);
                    var messages = errors.map(function(m) {
                        return "<li>" + m + "</li>";
                    });

                    errorPanel.find("ul").append(messages);

                    summaryDiv = qaDiv.closest(".box-primary").find(".summaryDiv");
                    summaryDiv.empty();
                    summaryDiv.append(errorPanel);
                    errorPanel.show();

                    //display error message below the edit textbox   

                    var answerErrors = data.answerErrors.join(",");
                    var questionErrors = data.questionErrors.join(",");
                    if (answerErrors.length > 0) {
                        qaDiv.find(answerErrorDivId).text(answerErrors);
                    }

                    if (questionErrors.length > 0) {
                        qaDiv.find(questionErrorDivId).text(questionErrors);
                    }
                }
            });
        }

        function clickEdit() {
            var qaDiv = $(this).closest(".question-answer-div");
            if ($(this).hasClass('disabled')) {
                return;
            }

            qaDiv.find("select").attr("readonly", null);
            qaDiv.find("select").attr("disabled", null);

            qaDiv.find("input[type='password']").hide();

            if ($.contains($("#kbq-panel")[0], qaDiv[0])) {

                var oldValues = qaDiv.find("input[type='text']").val();
                qaDiv.find("input[type='text']").val("");
                qaDiv.find("#lastValue").val(oldValues);
            }

            qaDiv.find("input[type='text']").show();
            qaDiv.find("input[type='text']")
                .prop("readonly", function(i, value) {
                    return !value;
                });
            qaDiv.find("input[type='text']").removeAttr("readonly");
            qaDiv.find("input[type='text']").removeAttr("disabled");
            qaDiv.siblings().find(".fa-edit").addClass("disabled");
            $(this).hide();
            $(this).siblings(".fa-save").show();
            $(this).siblings(".fa-undo").show();
        }

        $(document)
            .on("change", "select", function() {
                var i = $(this)[0].selectedIndex;
                $(this)
                    .children()
                    .each(function(index) {
                        $(this).prop("selected", index === i);
                        if (index === i) {
                            $(this).prop("selected", true);
                        } else {
                            $(this).prop("selected", false);
                        }
                    });
            });

    });