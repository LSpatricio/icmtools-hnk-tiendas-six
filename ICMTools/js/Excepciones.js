function listExceptionsConfiguration() {

    if ($("#SelectSociety").val() == "-1" || $("#SelectPersonnelDivision").val() == "-1") {
        $("#statusAlert").addClass("show")
        $("#statusAlert").html("<span>Por favor seleccione Sociedad y División de Personal.</span><i class='float-right fas fa-exclamation-triangle fa-fw'></i>");
        return false;
    } else {
        $("#statusAlert").removeClass("show")
    }
    setFormStatus("processing");
    const requestData = {
        Society: $("#SelectSociety").val(),
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    };
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/configuracion/exceptionconfig",
        data: JSON.stringify(requestData),
        success:
            function (response) {
                if (response.d == "False") {
                    $("#reportTitle").html("No existe configuración de Excepciones para Sociedad " + $("#SelectSociety").val() + " División de Personal " + $("#SelectPersonnelDivision").val());
                    setFormStatus("error");
                } else {
                    var Title = "Filtros: "
                    Title += "<button type='button' class='btn btn-info'>" +
                        "Sociedad <span class='badge badge-light'>" + $("#SelectSociety").val() + "</span>" +
                        "</button> ";
                    Title += "<button type='button' class='btn btn-info'>" +
                        "División de Personal <span class='badge badge-light'>" + $("#SelectPersonnelDivision").val() + "</span>" +
                        "</button>";
                    $("#reportTitle").html(Title);
                    $("#reportArea").show();
                    $("#reportTable").html(response.d);
                    activateTable();
                    setFormStatus("success");
                };
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
               // alert(XMLHttpRequest.responseText);
            }
    });
}

function getDetails(Lot) {
    const requestData = {
        Lot: Lot
    };
    $('#ModalTitle').html('Excepciones lote #' + Lot);
    $('#ModalBody').html("Sociedad:" + $("#SelectSociety") + " División de Personal:" + $("#SelectPersonnelDivision") + " Periodo:" + $("#SelectPeriod") + "<br/>");
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/SelectExceptionsHistoryDetails",
        data: JSON.stringify(requestData),
        success:
            function (response) {
                $('#ModalBody').html(response.d);
                $("#reportArea").show();
                customTable('#DetailsTable');
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                $(pageConfig.fileUploadSelector).find('input[type=file]').val("").css("background-color", "#fff");
            }
    });
};

function customTable(id) {
    switch (id) {
        case '#Table':
            var table = $(id).DataTable({                
                "lengthMenu": [[10, 20, 30, -1], [10, 20, 30, "Todos"]],
                "order": [[0, "desc"]],
                "columnDefs": [{
                    "targets": -1,
                    "data": null,
                    "defaultContent": "<div class='btn-group btn-group-sm' role='group'>" +
                        "<button type='button' data-toggle='modal' data-target='#Modal' class='btn btn-sm btn-outline-primary' data-action='detalles'><i class='fas fa-eye'></i></button>" +
                        "</div>"
                }],
                responsive: true,
                language: {
                    "decimal": "",
                    "emptyTable": "No hay información disponible", 
                    "info": "Mostrando de _START_ a _END_ de _TOTAL_ registros", 
                    "infoEmpty": "Mostrando 0 a 0 de 0 registros", 
                    "infoFiltered": "(filto de _MAX_ registros)", 
                    "infoPostFix": "",
                    "thousands": ",",
                    "lengthMenu": "Mostrar _MENU_ registros", 
                    "loadingRecords": "Cargando...", 
                    "processing": "Procesando...", 
                    "search": "Buscar", 
                    "zeroRecords": "No se encontraron coincidencias", 
                    "paginate": {
                        "first": "Primera", 
                        "last": "Ultima", 
                        "next": "Siguiente", 
                        "previous": "Anterior", 
                    },
                    "aria": {
                        "sortAscending": ": activate to sort column ascending",
                        "sortDescending": ": activate to sort column descending"
                    }
                }
            });
            $('#Table tbody').on('click', 'button', function () {
                var data = table.row($(this).parents('tr')).data();                
                var action = $(this).data('action');

                switch (action) {
                    case 'detalles':
                        getDetails(data[0], $("#SelectSociety").val(), $("#SelectPersonnelDivision").val(), $("#SelectPeriod").val());
                        break;
                    default:
                        return false;
                        break;
                }
            });
            break;
        case '#DetailsTable':
            var table = $(id).DataTable({
                responsive: true,
                dom: 'Bfrtip',
                buttons: [                    
                    [
                        {
                            extend: 'copy',
                            text: '<i class="fas fa-copy fa-fw"></i>Copiar',
                            className: 'btm-sm btn-outline-info',
                            title: null,
                            messageTop: null,
                            exportOptions: {
                                modifier: {
                                    page: 'all'
                                }
                            }
                        }
                    ],
                    [
                        {
                            extend: 'excel',
                            text: '<i class="fas fa-file-excel fa-fw"></i>Exportar',
                            className: 'btm-sm btn-outline-success',
                            filename: "Configuración de Excepciones",
                            title: null,
                            messageTop: null,
                            exportOptions: {
                                modifier: {
                                    page: 'all'
                                }
                            }
                        }
                    ],
                ],
                language: {
                    "decimal": "",
                    "emptyTable": "No hay información disponible", 
                    "info": "Mostrando de _START_ a _END_ de _TOTAL_ registros", 
                    "infoEmpty": "Mostrando 0 a 0 de 0 registros", 
                    "infoFiltered": "(filto de _MAX_ registros)", 
                    "infoPostFix": "",
                    "thousands": ",",
                    "lengthMenu": "Mostrar _MENU_ registros", 
                    "loadingRecords": "Cargando...", 
                    "processing": "Procesando...", 
                    "search": "Buscar", 
                    "zeroRecords": "No se encontraron coincidencias", 
                    "paginate": {
                        "first": "Primera", 
                        "last": "Ultima", 
                        "next": "Siguiente", 
                        "previous": "Anterior",
                    },
                    "aria": {
                        "sortAscending": ": activate to sort column ascending",
                        "sortDescending": ": activate to sort column descending"
                    }
                }
            });
            break;
        default:
            return false;
            break;
    }
};
function getExceptionsHistoryReport() {
    $("#reportArea").hide();
    if ($("#SelectPeriod").val() == "-1" || $("#SelectSociety").val() == "-1" || $("#SelectPersonnelDivision").val() == "-1") {
        $("#statusAlert").addClass("show")
        $("#statusAlert").html("Por favor seleccione Periodo, Sociedad y División de Personal.<i class='fas fa-exclamation-triangle fa-fw float-right'></i>");
        return false;
    } else {
        $("#statusAlert").removeClass("show");
    }
    const requestData = {
        Society: $("#SelectSociety").val(),
        Period: $("#SelectPeriod").val(),
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    };

    $("#btnRefresh").attr('disabled', true);
    $("#btnRefresh").html("<i class='fas fa-sync-alt fa-spin fa-fw'></i>Procesando...");
    setFormStatus("processing");

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/SelectExceptionsHistory",
        data: JSON.stringify(requestData),
        success:
            function (response) {
                if (response.d == "False") {
                    $("#reportTitle").html("No existe historial de carga para Periodo " + $("#SelectPeriod").val() + " Sociedad " + $("#SelectSociety").val() + " División de Personal " + $("#SelectPersonnelDivision").val());
                    setFormStatus("error");
                } else {
                    var Title = "Filtros: "
                    Title += "<button type='button' class='btn btn-info'>" +
                        "Periodo <span class='badge badge-light'>" + $("#SelectPeriod").val() + "</span>" +
                        "</button> ";
                    Title += "<button type='button' class='btn btn-info'>" +
                        "Sociedad <span class='badge badge-light'>" + $("#SelectSociety").val() + "</span>" +
                        "</button> ";
                    Title += "<button type='button' class='btn btn-info'>" +
                        "División de Personal <span class='badge badge-light'>" + $("#SelectPersonnelDivision").val() + "</span>" +
                        "</button>";
                    $("#reportTitle").html(Title);
                    $("#reportTable").html(response.d);
                    customTable('#Table');
                    $("#reportArea").show();
                    setFormStatus("success");
                };

                $("#btnRefresh").attr('disabled', false);
                $("#btnRefresh").html("<i class='fas fa-sync-alt fa-fw'></i> Mostrar reporte");

                $(function () {
                    $('[data-toggle="tooltip"]').tooltip()
                })
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                $("#btnRefresh").attr('disabled', false);
                $("#btnRefresh").html("<i class='fas fa-sync-alt fa-fw'></i> Mostrar reporte");
                setFormStatus("error");
               // alert(XMLHttpRequest.responseText);
            }
    });
}

function StartImport() {
    Period = $("#SelectPeriod").val();
    Society = $("#SelectSociety").val();
    PersonnelDivision = $("#SelectPersonnelDivision").val();

    if (Society != '-1' && PersonnelDivision == null) {
        $("#statusAlert").addClass("show")
        $("#statusAlert").html("No existen jerarquias para la Sociedad " + Society + "<i class='fas fa-exclamation fa-fw float-right'></i>");
        return false;
    }

    if (Period == "-1" || Society == "-1" || PersonnelDivision == "-1") {
        $("#statusAlert").addClass("show")
        $("#statusAlert").html("<span>Seleccione filtros y archivo para la carga.</span><i class='fas fa-exclamation-triangle fa-fw float-right'></i>");
    } else {
        CheckFileExistsE();
    }
}

function validaCatalogos() {
    setLoadingBar("Validando catalogos", 5);
    //const requestData = {}
    $.ajax({
        tpye: "GET",
        url: "/api/excepciones/validacatalogos",
        success:
            CheckFileExistsE(),
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                //alert(XMLHttpRequest.responseText);
            }
    });
};

function CheckFileExistsE() {
    CheckFileExists(InsertInfoBDExcepciones)
}

function InsertInfoBDExcepciones() {
    setLoadingBar("Preparando registros para iniciar validación", 50);

    var token = $('input[name="__RequestVerificationToken"]').val();

    const requestData = {
        Period: $("#SelectPeriod").val(),
        Society: $("#SelectSociety").val(),
        PersonnelDivision: $("#SelectPersonnelDivision").val(),
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    } 
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/InsertInfoBD",
        headers: { 'X-XSRF-Token': token },
        data: JSON.stringify(requestData),
        success:
            function (response) {
                if (response.d == true) {
                    DeleteFile(response.f)
                    ValidateInfoBD();
                } else {
                    var errorsTable = "<table id='Table' class='table table-sm table-hover'>" +
                        "<thead>" +
                        "<tr>" +
                        "<th>Problema</th>" +
                        "<th>Detalles</th>" +
                        "</tr>" +
                        "</thead> " +
                        "<tbody> " +
                        "<tr><td>Error</td><td>" + response.d + "</td></tr>" +
                        "</tbody> " +
                        "</table>"
                    $("#formatErrors").html(errorsTable);
                    activateTable();                    
                    setFormStatus("error");
                };
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                //alert(XMLHttpRequest.responseText);
                if (XMLHttpRequest.status === 400 && XMLHttpRequest.responseText.includes("LIMIT_EXCEED")) {
                    mostrarModalBloqueo("Se ha detectado un problema persistente en la carga.<br />Has alcanzado el límite de intentos permitido (3) para esta división. Para evitar inconsistencias,<strong> la carga ha sido bloqueada para esta Division de Personal</strong>.<br />Por favor, contacta a soporte técnico y <strong>continua con la carga de las siguientes Divisiones de Personal</strong>");
                }
            }
    });
};

function ValidateInfoBD() {
    setLoadingBar("Validando Excepciones en ICM", 75);
    const requestData = {
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    } 
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/ValidateInfoICM",
        data: JSON.stringify(requestData),
        success:
            function (response) {
                if (response.d == true) {
                    InsertExceptions();
                } else {
                    $("#formatErrors").html(response.d);
                    $("#reportArea").show();
                    activateTable();                    
                    setFormStatus("error");
                };
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                //alert(XMLHttpRequest.responseText);
                if (XMLHttpRequest.status === 400 && XMLHttpRequest.responseText.includes("LIMIT_EXCEED")) {
                    mostrarModalBloqueo("Se ha detectado un problema persistente en la carga.<br />Has alcanzado el límite de intentos permitido (3) para esta división. Para evitar inconsistencias,<strong> la carga ha sido bloqueada para esta Division de Personal</strong>.<br />Por favor, contacta a soporte técnico y <strong>continua con la carga de las siguientes Divisiones de Personal</strong>");
                }
            }
    });
};

function InsertExceptions() {
    setLoadingBar("Insertando Excepciones", 90);
    const requestData = {
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    } 
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/InsertExceptions",
        data: JSON.stringify(requestData),
        success:
            function (response) {
                if (response.d == true) {
                    ConfirmExceptions();
                } else {
                    var errorsTable = "<table class='table table-sm table-hover'>" +
                        "<thead>" +
                        "<tr>" +
                        "<th>Problema</th>" +
                        "<th>Detalles</th>" +
                        "</tr>" +
                        "</thead> " +
                        "<tbody> " +
                        "<tr><td>Error</td><td>" + response.d + "</td></tr>" +
                        "</tbody> " +
                        "</table>"
                    $("#formatErrors").html(errorsTable);
                    $("#reportArea").show();
                    activateTable();                    
                    setFormStatus("error");
                };
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                // alert(XMLHttpRequest.responseText);
                if (XMLHttpRequest.status === 400 && XMLHttpRequest.responseText.includes("LIMIT_EXCEED")) {
                    mostrarModalBloqueo("Se ha detectado un problema persistente en la carga.<br />Has alcanzado el límite de intentos permitido (3) para esta división. Para evitar inconsistencias,<strong> la carga ha sido bloqueada para esta Division de Personal</strong>.<br />Por favor, contacta a soporte técnico y <strong>continua con la carga de las siguientes Divisiones de Personal</strong>");
                }
            }
    });
};
function ConfirmExceptions() {
    setLoadingBar("Finalizando confirmaciones", 95); //confirmacion por correo
    const requestData = {
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    } 
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/ConfirmExceptions",
        data: JSON.stringify(requestData),
        dataType: "json",
        success:
            function (response) {
                if (response.d) {
                    setLoadingBar("Importación Completa", 100);
                    setFormStatus("success");
                };
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                // alert(XMLHttpRequest.responseText);
                if (XMLHttpRequest.status === 400 && XMLHttpRequest.responseText.includes("LIMIT_EXCEED")) {
                    mostrarModalBloqueo("Se ha detectado un problema persistente en la carga.<br />Has alcanzado el límite de intentos permitido (3) para esta división. Para evitar inconsistencias,<strong> la carga ha sido bloqueada para esta Division de Personal</strong>.<br />Por favor, contacta a soporte técnico y <strong>continua con la carga de las siguientes Divisiones de Personal</strong>");
                } 
            }
    });
}

function getDetails(Lot, Society, PersonnelDivision, Period) {
    //$('#Modal').modal('show');
    $('#ModalTitle').html('Excepciones lote #' + Lot);
    $('#ModalBody').html("<strong>Sociedad:</strong> " + Society + "   <strong>División de Personal:</strong> " + PersonnelDivision + "   <strong>Periodo:</strong> " + Period + "<br/>");
    requestData = { LotData: Lot}
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/excepciones/SelectExceptionsHistoryDetails",
        data: JSON.stringify(requestData),
        success:
            function (data) {
                $('#ModalBody').html(data.d);
                activateTable('#DetailsTable');
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                setFormStatus("error");
                alert(XMLHttpRequest.responseText);
            }
    });
};