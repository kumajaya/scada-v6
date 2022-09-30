﻿const BUTTON_LOCK_DURATION = 3000; // ms
const MS_PER_DAY = 24 * 60 * 60 * 1000;

var maxReportPeriod = 0; // set in HistDataReport.cshtml

function hideErrorMessage() {
    $("#divErrorMessage").addClass("hidden");
}

function reportValidityExtra() {
    let errors = [];

    // time range
    let startTimeMs = Date.parse($("#txtStartTime").val());
    let endTimeMs = Date.parse($("#txtEndTime").val());

    if (startTimeMs > endTimeMs) {
        errors.push("Start date must be less or equal than end date.");
    } else if (endTimeMs - startTimeMs > maxReportPeriod * MS_PER_DAY) {
        errors.push(ScadaUtils.formatString("The maximum period length is {0} days.", maxReportPeriod));
    }

    // channel numbers
    let cnlNums = ScadaUtils.parseRange($("#txtCnlNums").val());

    if (!(cnlNums && cnlNums.length > 0)) {
        errors.push("Invalid channel numbers.");
    }

    if (errors.length > 0) {
        $("#divErrorMessage")
            .html(errors.join("<br />"))
            .removeClass("hidden");
        return false;
    } else {
        return true;
    }
}

function lockGenerateButton() {
    $("#btnGenerateReport").prop("disabled", true);
    $("#divWaitHint").removeClass("hidden");

    setTimeout(function () {
        $("#btnGenerateReport").prop("disabled", false);
        $("#divWaitHint").addClass("hidden");
    }, BUTTON_LOCK_DURATION);
}

$(document).ready(function () {
    $("#btnGenerateReport").click(function () {
        hideErrorMessage();

        if ($("#frmReportArgs")[0].reportValidity() && reportValidityExtra()) {
            lockGenerateButton();
            let reportUrl = "Print/PrintHistDataReport" +
                "?startTime=" + $("#txtStartTime").val() +
                "&endTime=" + $("#txtEndTime").val() +
                "&archive=" + ($("#selArchive option:selected").val() ?? "") +
                "&cnlNums=" + $("#txtCnlNums").val();
            location = reportUrl;
        }

        return false;
    });

    $("#btnSelectCnls").click(function () {
        let dialogs = new Dialogs("../../");
        let txtCnlNums = $("#txtCnlNums");

        dialogs.selectChannels(txtCnlNums.val(), function (result) {
            if (result) {
                txtCnlNums.val(result.cnlNums);
            }
        });

        return false;
    });
});
