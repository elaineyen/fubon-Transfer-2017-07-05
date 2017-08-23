///* off-canvas sidebar toggle */
//$('[data-toggle=offcanvas]').click(function () {
//    $('.row-offcanvas').toggleClass('active');
//    $('.collapse').toggleClass('in').toggleClass('hidden-xs').toggleClass('visible-xs');
//});
(function (window, undefind) {
    if ($('.menu-list') != null) {
        $('.menu-list ul.sub-menu li,.HomeMenu').click(function () {
            $(this).children('a:first')[0].click();
        })
        var manu = $('#vbManu').val();
        var subManu = $('#vbSubmanu').val();
        if (manu !== undefined && manu.length > 0) {
            //$('#menu-content li#' + manu).click();

            $('#menu-content li').removeClass('active');
            $('#' + manu).addClass('active');
            if (manu !== 'HomeMain')
                $('#' + manu).click();
            if (subManu !== undefined && subManu.length > 0) {
                $('#' + subManu).addClass('active');
            }
        }
    }
})(window);