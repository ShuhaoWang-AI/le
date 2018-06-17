$(function () {
    //$("select").select2();

    $(".datepicker").datepicker({
        autoclose: true
    });
    $('input[type="checkbox"], input[type="radio"]').not('[name*="onOffSwitch"]').iCheck({
        checkboxClass: "icheckbox_minimal-blue",
        radioClass: "iradio_minimal-blue"
    });
});