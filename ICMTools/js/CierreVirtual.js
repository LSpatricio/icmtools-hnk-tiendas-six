var _jsonCierreVirtual;
var _jsonCierrePorcentajes;
var _jsonCierreAsignaciones;
var _jsonDocsGenerados;
var _jsonCierreDistritos;
var _Avance;
var _horarioConsulta;
var _estatusImportacion;

$(document).ready(function () {
    //ImportacionICM();
});

function DescargarCierreVirtual() {
    var $DescargarButton = $("#DescargarButton");
    var DescargarButtonHtml = $DescargarButton.html();
    setButtonProcess($DescargarButton);

    var requestData = {
        jsonCierreVirtual: _jsonCierreVirtual,
        jsonCierreAsignaciones: _jsonCierreAsignaciones,
        jsonCierreDistritos: _jsonCierreDistritos,
        jsonCierrePorcentajes: _jsonCierrePorcentajes,
        jsonDocsGenerados: _jsonDocsGenerados,
        Avance: _Avance,
        HorarioConsulta: _horarioConsulta,
        EstatusImportacion: _estatusImportacion,
        Periodo: $("#PeriodoSelect").val()
    };

    $.ajax({
        type: "POST",
        data: JSON.stringify(requestData),
        url: "/api/cierrevirtual/descargar",
        contentType: "application/json",
        success: function (response) {
            (async () => {
                await downloadAndDeleteFile(response.f);
                $DescargarButton.html(DescargarButtonHtml);
            })();
        },
        error: function (xhr) {
            console.log(xhr);            
            $DescargarButton.html(DescargarButtonHtml);
        }
    });

    return false;
}

function ImportacionICM() {
    CleanUp();
    setLoadingBar("Ejecutando importación...", 10);
    setFormStatus("processing", 0);
    $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Procesando...');

    _horarioConsulta = formatoFecha();
    $('#hora_consulta').html("Horario de consulta: " + _horarioConsulta);

    $.ajax({
        type: "POST",
        url: "/api/cierrevirtual/ImportacionICM",
        contentType: "application/json",
        success: function (response) {
            _estatusImportacion = response.status_importacion;
            $("#status_importacion").html(_estatusImportacion);
            InsertDataCierreVirtual(false);
        },
        error: function (xhr) {
            setFormStatus("error");

            let error_detail = getRootExceptionMessage(xhr);
            let error_message_ = 'Error de comunicación (ImportacionICM).';

            if (error_detail !== '') {
                error_message_ += ' ' + error_detail;
            }

            $("#formatErrors").html(error_message_);
            $("#status_importacion").html(error_message_);

            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
            InsertDataCierreVirtual(true);
        }
    });
}

function InsertDataCierreVirtual(import_failed) {
    setLoadingBar("Obteniendo información...", 50);
    const requestData = {
        Periodo: $("#PeriodoSelect").val()
    }
    $.ajax({
        type: "POST",
        data: JSON.stringify(requestData),
        url: "/api/cierrevirtual/insertdata",
        contentType: "application/json",
        success: function (response) {

            _jsonCierreVirtual = response.json_cierre_virtual;
            _jsonCierreAsignaciones = response.json_cierre_asignaciones;
            _jsonCierreDistritos = response.json_cierre_distritos;
            _jsonCierrePorcentajes = response.json_cierre_porcentajes;
            _jsonDocsGenerados = response.json_docs_generados;
            _Avance = response.doc_gen_percentage;

            $("#tbody_cierre_virtual").html(response.cierre_virtual);
            $("#tbody_cierre_virtual_porcentaje").html(response.cierre_porcentajes);
            $("#tbody_cierre_por_plaza").html(response.cierre_asignaciones);
            $("#tbody_doc_generados").html(response.docs_generados);
            $("#tbody_cierre_distritos").html(response.cierre_distritos);
            $("#doc_gen_percentage").html("<b>" + response.doc_gen_percentage + "</b>");

            setFormStatus("success");
            $("#DescargarButton").show();

            if (import_failed) {
                $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
                setLoadingBar("Consulta sin importación", 100);
            }
            else {
                $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Iniciar Consulta');
                setLoadingBar("¡Completado!", 100);
            }
        },
        error: function (xhr) {
            setFormStatus("error");

            let error_detail = getRootExceptionMessage(xhr);
            let error_message_ = 'Error de comunicación (insertdata).';

            if (error_detail !== '') {
                error_message_ += ' ' + error_detail;
            }

            $("#formatErrors").html(error_message_);

            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        }
    });
}

function getRootExceptionMessage(xhr) {
    let inner_message = "";

    try {
        let current = xhr.responseJSON;
        while (current) {
            inner_message = current.ExceptionMessage;
            current = current.InnerException;
        }
    } catch (e) { }

    return inner_message;
}

function CleanUp() {
    $("#status_importacion").html('');
    $("#tbody_cierre_virtual").html('');
    $("#tbody_cierre_virtual_porcentaje").html('');
    $("#tbody_cierre_por_plaza").html('');
    $("#tbody_doc_generados").html('');
    $("#tbody_cierre_distritos").html('');
    $("#doc_gen_percentage").html('');
    $("#DescargarButton").hide();
}

function formatoFecha(fecha = new Date()) {
    let dd = String(fecha.getDate()).padStart(2, '0');
    let MM = String(fecha.getMonth() + 1).padStart(2, '0');
    let yyyy = fecha.getFullYear();

    let horas = fecha.getHours();
    let minutos = String(fecha.getMinutes()).padStart(2, '0');

    let tt = horas >= 12 ? "PM" : "AM";
    horas = horas % 12 || 12;
    horas = String(horas).padStart(2, '0');

    return `${dd}/${MM}/${yyyy} ${horas}:${minutos} ${tt}`;
}