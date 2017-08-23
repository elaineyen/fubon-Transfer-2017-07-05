(function (window, undefind) {
    var jqgridCustom = {};

    window.jqgridCustom = jqgridCustom;

    jqgridCustom.setHeight = '290';

    //#region jqgridCustom.createDialog 範例
    //var obj = [
    //   { 'name': 'testtxt', 'type': 'string', 'title': '測試', 'max': '3', 'req': 'true' },
    //   { 'name': 'testdate', 'type': 'date', 'title': '測試日期', 'req': 'true' }
    //]
    //jqgridCustom.createDialog(dialogid, obj);
    //#endregion

    jqgridCustom.createDialog =
    function (dialogid, data) {
        var str = '';
        str += '<input type="hidden" id="actionType" value="" />';
        str += '<form id="' + dialogid + 'From">';
        str += '<table style="width:100%">';
        var reqobjs = [];
        var datepickers = [];
        var reqobj = function (type, name) {
            this.type = type;
            this.name = name;
        };

        $.each(data, function (dkey, dvalue) {
            let tr = '<tr>';
            let tdtitle = '<td style="white-space:nowrap; text-align:right">';
            let tdinput = '<td style="white-space:nowrap"><input type="text" ';
            let name = '';
            let type = '';

            $.each(dvalue, function (key, value) {
                if (key === 'name')
                    name = (dialogid + value);
                if (key === 'title')
                    tdtitle += (value + ' : ');
                if (key === 'type') {
                    switch (value) {
                        case 'date':
                            type = 'date';
                            tdinput += ('id="' + name + 'datepicker" name="' + name + 'datepicker" ');
                            datepickers.push(name + 'datepicker');
                            break;
                        case 'string':
                        default:
                            type = 'string';
                            tdinput += ('id="' + name + '" name="' + name + '"  maxlength="3" ');
                            break;
                    }
                }
                if (key === 'max' && type !== 'date')
                    tdinput += (' maxlength = ' + value + ' ');
                if (key === 'req' && value === 'true')
                    reqobjs.push(new reqobj(type, name));
            })
            tdtitle += '</td>';
            tdinput += '></td>';
            tr += tdtitle;
            tr += tdinput;
            tr += '</tr>';
            str += tr;
        })
        str += '<tr>';
        str += '<td colspan="2" style="white-space:nowrap; text-align:center">'
        str += '<input type="button" class=" btn btn-primary" style="margin-right:30px;margin-top:10px;" id="' + dialogid + 'btnSave" value="儲存" />';
        str += '<input type="button" class=" btn btn-primary" style="margin-right:30px;margin-top:10px;" id="' + dialogid + 'btnDelete" value="刪除" />';
        str += '<input type="button" class=" btn btn-primary" style="margin-top:10px;" id="' + dialogid + 'btnCancel" value="取消" /></td>';
        str += '</tr>';
        str += '</table>';
        str += '</form>';
        $('#' + dialogid).append(str);

        $.each(datepickers, function (key, value) {
            $("#" + value).datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: 'yy/mm/dd',
                showOn: "both",
                buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
                onSelect: function (value) {
                    if (verified.isDate(value))
                        $(this).parent().children().each(function () {
                            if ($(this).is('label') && $(this).hasClass('error'))
                                $(this).remove();
                            if ($(this).is('input') && $(this).hasClass('error'))
                                $(this).removeClass('error');
                        })
                }
            });
        })

        $.each(reqobjs, function (key, value) {
            if (value.type === 'string')
                verified.required(dialogid + 'From', value.name, message.required(message.version));
            if (value.type === 'date')
                verified.datepicker(dialogid + 'From', value.name + 'datepicker', false, $('#' + value.name + 'datepicker').val());
        })

        $("#" + dialogid).dialog({
            autoOpen: false,
            resizable: true,
            height: 'auto',
            width: 'auto',
            position: { my: "center", at: "center", of: window },
            closeText: "取消",
            resizable: true,
        });
    }

    //#region jqgridCustom.randerAction 範例
    //colName unshift Actions
    //colModel unshift { name: "act", index: "act", width: 100, sortable: false }
    //jqgridCustom.createDialog(dialogid, obj);
    //jqgridCustom.randerAction(jqGridId, 'A41');
    //#endregion

    jqgridCustom.randerAction =
    function (jqGridId, viewId) {
        var ids = $("#" + jqGridId).jqGrid('getDataIDs');

        for (var i = 0; i < ids.length; i++) {
            var divStart = '<div class="btn-group">';
            var edit = '<a class="btn" style="padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;"' +
                       ' href="#" onclick=\"javascript:' + viewId + jqGridId + 'Edit(' + (i + 1) + ');\" return:false;><i class="fa fa-pencil-square-o fa-lg"></i></a>';
            var view = '<a class="btn" style="padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;"' +
                       ' href="#" onclick=\"javascript:' + viewId + jqGridId + 'View(' + (i + 1) + ')\" return:false;><i class="fa fa-eye fa-lg"></i></a>';
            var dele = '<a class="btn" style="padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;"' +
                       ' href="#" onclick=\"javascript:' + viewId + jqGridId + 'Dele(' + (i + 1) + ')\" return:false;><i class="fa fa-trash fa-lg"></i></a>';
            var divEnd = '</div>';
            $("#" + jqGridId).jqGrid('setRowData', i + 1, { act: divStart + edit + view + dele + divEnd });
        }
    }

    //#region jqgridCustom.updatePagerIcons 範例
    //loadComplete
    //var table = $(this);
    //jqgridCustom.updatePagerIcons(table);
    //#endregion
    jqgridCustom.updatePagerIcons =
    function updatePagerIcons(table)  //table => loadComplete this.table
    {
        var replacement =
        {
            'ui-icon-seek-first': 'ace-icon fa fa-angle-double-left bigger-140',
            'ui-icon-seek-prev': 'ace-icon fa fa-angle-left bigger-140',
            'ui-icon-seek-next': 'ace-icon fa fa-angle-right bigger-140',
            'ui-icon-seek-end': 'ace-icon fa fa-angle-double-right bigger-140'
        };
        $('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function () {
            var icon = $(this);
            var $class = $.trim(icon.attr('class').replace('ui-icon', ''));

            if ($class in replacement) icon.attr('class', 'ui-icon ' + replacement[$class]);
        })
    }

    jqgridCustom.hideFrozenTitle =
    function () {
        $('.ui-jqgrid-view > .frozen-div').find('.ui-jqgrid-resize').hide()
    }
})(window);