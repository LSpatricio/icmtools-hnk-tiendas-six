(function ($) {

    // Tooltop de ayuda descriptiva de las funciones de los botones.
    $.AyudaTooltip = function () {
        $('[data-toggle="tooltip"]').tooltip()
    }

    // Al hacer click en el boton de cargar del documento, deshabilita el botón y empieza la importanción
    $.btnStartImport = function () {
        $("#btnStartImport").click(function () {
                StartImport();
                $('#btnStartImport').tooltip('hide');
        });
    }

    
})(jQuery);