(function (window, undefind) {

    var verified = {};
    var dateFormat;
    dateFormat = /^((?!0000)[0-9]{4}[/]((0[1-9]|1[0-2])[/](0[1-9]|1[0-9]|2[0-8])|(0[13-9]|1[0-2])[/](29|30)|(0[13578]|1[02])[/]31)|([0-9]{2}(0[48]|[2468][048]|[13579][26])|(0[48]|[2468][048]|[13579][26])00)[/]02[/]29)$/;

    window.verified = verified;

    verified.required = function (formid, elementid, message)
    {       
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

    verified.datepicker = function (formid, datepickerid, reportDateFlag,date)
    {      
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
        $.validator.addMethod("dateFormate",
        function (value, element, arg) {
            return verified.isDate(value, reportDateFlag);
        }, reportDateFlag ? message.reportDate : message.date);
        //#endregion
        $('#' + datepickerid).rules('add', {
            dateFormate: true,
        })
    }

    verified.isDate = function (value, reportDate) {
        
        reportDate = reportDate || false;

        if ((typeof reportDate === 'boolean') && reportDate)
        {
          
            return dateFormat.test(value) ? verifiedReportDate(value) : false;
        }           
        else
            return dateFormat.test(value);
    }

    verified.reportDate = function ()
    {
        var d = getOnlyDate();
        var day = d.getDate();
        if (day <= 5)
        {
            d.setDate(1); //設定為當月份的第一天
            d.setDate(d.getDate() - 1); //將日期-1為上月的最後一天
            return d;
        }
        else
        {
            d.setDate(25);
            return d;
        }         
    }

    //formate string(yyyy/MM/dd) to date 失敗回傳 false
    verified.datepickerStrToDate = function (value)
    {
        if(dateFormat.test(value))
        {
            var d = value.split('/');
            return new Date(d[0]+'-'+d[1]+'-'+d[2]);
        }
        return false;
    }

    function verifiedReportDate(value)
    {
        if(dateFormat.test(value))
        {
            var datepicker = verified.datepickerStrToDate(value);
            if(!datepicker)
            {
                return false;
            }
            if(datepicker.getDate() === 25)
                return true;
            var d = getOnlyDate();
            d.setMonth(datepicker.getMonth() + 1);
            d.setDate(1);
            d.setDate(d.getDate() - 1);            
            if (datepicker.getTime() === d.getTime())
                return true;
            return false;
        }
        return false;
    }

    function getOnlyDate()
    {
        var d = new Date();
        d = new Date(d.getFullYear() + '-' + padLeft((d.getMonth() + 1), 2) + '-' + (d.getDate()));
        return d;
    }
 
    function padLeft(str,lenght,padStr){
        if(typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if(str.length >= lenght)
            return str;
        else
            return padLeft(padStr +str,lenght+ padStr);
    }
    function padRight(str,lenght,padStr){
        if(typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if(str.length >= lenght)
            return str;
        else
            return padRight(str+padStr,lenght,padStr);
    }

})(window);