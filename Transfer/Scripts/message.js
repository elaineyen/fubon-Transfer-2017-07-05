(function (window, undefind) {
    var message = {};

    window.message = message;

    message.reportDate = "不符合 評估日/報導日 (yyyy/mm/dd)";
    message.date = "不符合日期格式 (yyyy/mm/dd)";
    message.version = "版本";
    message.bondsNumber = "債券編號";
    message.required = function (value) {
        return value + "為必填!";
    }
})(window);