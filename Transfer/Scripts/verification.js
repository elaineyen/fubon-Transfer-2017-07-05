(function (window, undefind) {
    var verified = {};
    var created = {};
    var dateFormat;
    dateFormat = /^((?!0000)[0-9]{4}[/]((0[1-9]|1[0-2])[/](0[1-9]|1[0-9]|2[0-8])|(0[13-9]|1[0-2])[/](29|30)|(0[13578]|1[02])[/]31)|([0-9]{2}(0[48]|[2468][048]|[13579][26])|(0[48]|[2468][048]|[13579][26])00)[/]02[/]29)$/;

    window.verified = verified;
    window.created = created;

    verified.minlength = function (formid, elementid, value, msg) {
        value = value || 10;
        msg = msg || message.minlength(value);
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                error.appendTo(element.parent());
            }
        });
        $('#' + elementid).rules('add', {
            minlength: value,
            messages: {
                minlength: msg,
            }
        })
    }
    verified.maxlength = function (formid, elementid, value, msg) {
        value = value || 10;
        msg = msg || message.maxlength(value);
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                error.appendTo(element.parent());
            }
        });
        $('#' + elementid).rules('add', {
            maxlength: value,
            messages: {
                maxlength: msg,
            }
        })
    }

    verified.required = function (formid, elementid, message) {
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                error.appendTo(element.parent());
            }
        });
        $('#' + elementid).rules('add', {
            required: true,
            messages: {
                required: message,
            }
        })
    }

    verified.datepicker = function (formid, datepickerid, reportDateFlag) {
        reportDateFlag = reportDateFlag || false;

        $("#" + formid).validate({
            //rules: {
            //    datepicker: { dateFormate: date }
            //},
            errorPlacement: function (error, element) {
                error.appendTo(element.parent());
            }
        })

        //#region 客製化驗證
        $.validator.addMethod("reportDateFormate",
        function (value, element, arg) {
            return verified.isDate(value, true);
        }, message.reportDate);

        $.validator.addMethod("dateFormate",
        function (value, element, arg) {
            return verified.isDate(value, false);
        }, message.date);
        //#endregion
        if (reportDateFlag)
        $('#' + datepickerid).rules('add', {
            reportDateFormate: true,
        })
        else
            $('#' + datepickerid).rules('add', {
                dateFormate: true,
            })
    }

    created.createDatepicker = function (datepickerid, reportDateFlag, date) {
        reportDateFlag = reportDateFlag || false;
        var d = null;
        if (!(date === d)) {
            if (reportDateFlag) {
                d = verified.reportDate();
            }
            else {
                if (verified.isDate(date, false)) {
                    d = verified.datepickerStrToDate(date);
                }
                else {
                    d = getOnlyDate()
                }
            }
        }

        $("#" + datepickerid).datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: 'yy/mm/dd',
            showOn: "both",
            buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
            onSelect: function (value) {
                if (verified.isDate(value, reportDateFlag))
                    $(this).parent().children().each(function () {
                        if ($(this).is('label') && $(this).hasClass('error'))
                            $(this).remove();
                        if ($(this).is('input') && $(this).hasClass('error'))
                            $(this).removeClass('error');
                    })
            }
        }).datepicker('setDate', d);
    }

    created.clearDatepickerRangeValue = function (
        datepickerStartid, datepickerEndid) {
        $("#" + datepickerStartid).val('');
        $("#" + datepickerStartid).datepicker("option", "maxDate", null);
        $("#" + datepickerEndid).val('');
        $('#' + datepickerEndid).datepicker("option", "minDate", null);
    }

    created.createDatepickerRange = function (datepickerStartid,
        datepickerEndid, reportDateFlag) {
        var format = 'yy/mm/dd';
        reportDateFlag = reportDateFlag || false;

        var from = $("#" + datepickerStartid)
                    .datepicker({
                        changeMonth: true,
                        changeYear: true,
                        dateFormat: format,
                        showOn: "both",
                        buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
                        onSelect: function (value) {
                            to.datepicker("option", "minDate", getDate(this));
                            if (verified.isDate(value, reportDateFlag)) {
                                $(this).parent().children().each(function () {
                                    if ($(this).is('label') && $(this).hasClass('error'))
                                        $(this).remove();
                                    if ($(this).is('input') && $(this).hasClass('error'))
                                        $(this).removeClass('error');
                                })
                            }
                        }
                    });
        var to = $("#" + datepickerEndid).datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: format,
            showOn: "both",
            buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
            onSelect: function (value) {
                from.datepicker("option", "maxDate", getDate(this));
                if (verified.isDate(value, reportDateFlag)) {
                    $(this).parent().children().each(function () {
                        if ($(this).is('label') && $(this).hasClass('error'))
                            $(this).remove();
                        if ($(this).is('input') && $(this).hasClass('error'))
                            $(this).removeClass('error');
                    })
                }
            }
        });

        function getDate(element) {
            var date;
            try {
                date = $.datepicker.parseDate(format, element.value);
            } catch (error) {
                date = null;
            }
            return date;
        }
    }

    verified.isDate = function (value, reportDate) {
        reportDate = reportDate || false;
        value = value || '';
        if ((typeof reportDate === 'boolean') && reportDate) {
            return dateFormat.test(value) ? verifiedReportDate(value) : false;
        }
        else
            return dateFormat.test(value);
    }

    verified.reportDate = function () {
        var d = getOnlyDate();
        var day = d.getDate();
        if (day <= 5) {
            d.setDate(1); //設定為當月份的第一天
            d.setDate(d.getDate() - 1); //將日期-1為上月的最後一天
            return d;
        }
        else {
            d.setDate(25);
            return d;
        }
    }

    //formate string(yyyy/MM/dd) to date 失敗回傳 false
    verified.datepickerStrToDate = function (value) {
        if (dateFormat.test(value)) {
            var d = value.split('/');
            return new Date(d[0] + '-' + d[1] + '-' + d[2]);
        }
        return false;
    }

    function verifiedReportDate(value) {       
        if (dateFormat.test(value)) {           
            var datepicker = verified.datepickerStrToDate(value);
            if (!datepicker) {
                return false;
            }
            if (datepicker.getDate() === 25)
                return true;
            var d = getOnlyDate();
            d.setFullYear(datepicker.getFullYear())
            d.setMonth(datepicker.getMonth());
            d.setDate(1); //第一天
            d.setMonth((d.getMonth() + 1)); //下一個月
            d.setDate(d.getDate() - 1); //這個月最後一天
            if (datepicker.getTime() === d.getTime())
                return true;
            return false;
        }
        return false;
    }

    function getOnlyDate() {
        var d = new Date();
        d = new Date(d.getFullYear() + '-' + padLeft((d.getMonth() + 1), 2) + '-' + (d.getDate()));
        return d;
    }

    function padLeft(str, lenght, padStr) {
        if (typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if (str.length >= lenght)
            return str;
        else
            return padLeft(padStr + str, lenght + padStr);
    }
    function padRight(str, lenght, padStr) {
        if (typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if (str.length >= lenght)
            return str;
        else
            return padRight(str + padStr, lenght, padStr);
    }
})(window);