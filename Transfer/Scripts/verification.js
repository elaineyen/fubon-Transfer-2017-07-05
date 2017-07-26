﻿(function (window, undefind) {

    var verified = {};

    window.verified = verified;

    verified.isDate = function isDate(value) {
        var dateFormat;
        dateFormat = /^((?!0000)[0-9]{4}[/]((0[1-9]|1[0-2])[/](0[1-9]|1[0-9]|2[0-8])|(0[13-9]|1[0-2])[/](29|30)|(0[13578]|1[02])[/]31)|([0-9]{2}(0[48]|[2468][048]|[13579][26])|(0[48]|[2468][048]|[13579][26])00)[/]02[/]29)$/;
        return dateFormat.test(value);
    }

})(window);