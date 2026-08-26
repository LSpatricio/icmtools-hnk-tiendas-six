
// ===============================================
// Archivo: shared.js
// Descripción: Funciones compartidas para manejo de formularios, carga de archivos y llamadas a la API.
// Autor: Equipo de Desarrollo SOINF
// ==============================================

//Bandera para definir si hay una importacion en curso
let courseFlag = false;

function addToClientTable(name, text) {
    var table = $("#statusUploadTable");
    table.html("<tr><th>ti</th><th>Error</th></tr><tr><td>" + name + "</td><td>" + text + "</td></tr>");
}

function fillCell(row, cellNumber, text) {
    var cell = row.insertCell(cellNumber);
    cell.innerHTML = text;
}

async function GetSuccessMessage(cantidadTotal) {
    var params = new Object;
    params.pantalla = pageConfig.LogPage;
    params.cantidadTotal = cantidadTotal || 0;
    $.get('/api/shared/successmessage', params)
        .done(function (data) {
            InsertData_OnSuccess(data);
        });
}

function loadPersonnelDivisionsBySociety() {
    pageConfig.Society = $('#SelectSociety').val();
    $.ajax({
        type: "GET",
        url: "/api/shared/personneldivisions",
        data: { Society: pageConfig.Society },
        success:
            function (response) {
                $("#SelectPersonnelDivision").empty()
                $.each(response.l, function (i, item) {
                    $("#SelectPersonnelDivision").append($("<option>", { value: item.PersonnelDivisionValue, text: item.PersonnelDivisionName }, "</option>"));
                });
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                alert(XMLHttpRequest.responseText);
            }
    });
}

function loadPersonnelDivisionsBySocietyex() {
    pageConfig.Society = $('#SelectSociety').val();
    $.ajax({
        type: "GET",
        url: "/api/shared/personneldivisionsex",
        data: { Society: pageConfig.Society },
        success:
            function (response) {
                $("#SelectPersonnelDivision").empty()
                $.each(response.l, function (i, item) {
                    $("#SelectPersonnelDivision").append($("<option>", { value: item.PersonnelDivisionValue, text: item.PersonnelDivisionName }, "</option>"));
                });
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                alert(XMLHttpRequest.responseText);
            }
    });
}

function loadPersonnelDivisionsBySocietyexWithAllOption() {
    pageConfig.Society = $('#SelectSociety').val();
    $.ajax({
        type: "GET",
        url: "/api/shared/personneldivisionsex",
        data: { Society: pageConfig.Society },
        success:
            function (response) {
                $("#SelectPersonnelDivision").empty()
                if (pageConfig.Society != "-1") {
                    $("#SelectPersonnelDivision").append($("<option>", { value: "!", text: "Todas" }, "</option>"));
                }
                else {
                    $("#SelectPersonnelDivision").append($("<option>", { value: "-1", text: "Seleccione" }, "</option>"));
                }
                $.each(response.l, function (i, item) {
                    $("#SelectPersonnelDivision").append($("<option>", { value: item.PersonnelDivisionValue, text: item.PersonnelDivisionName }, "</option>"));
                });
            },
        error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                alert(XMLHttpRequest.responseText);
            }
    });
}

function loadSocieties() {
    const select = $("#SelectSociety");
    select.prop("disabled", true).html('<option value="">Cargando...</option>');

    $.ajax({
        type: "GET",
        url: "/api/shared/societies",
        contentType: "application/json",
        success: function (data) {
            select.empty().append('<option value="-1">Seleccione</option>');
            $.each(data, function (index, item) {
                select.append($("<option></option>").val(item.Value).html(item.Text));
            });
            select.prop("disabled", false);
        },
        error: function (xhr, textStatus, errorThrown) {
            console.error("Error cargando Sociedades:", errorThrown);
            select.html('<option value="">Error al cargar</option>');
        }
    });
}

function ValidateFileInitialName($input) {
    var isValid = true;

    var fileName = $input.get(0).files[0].name.trim()
    var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');

    if (fileNameWithoutExt.toLowerCase().indexOf("inicial_") != 0) {
        var mensaje = "El archivo inicial <b>" + fileName + "</b> no tiene un nombre válido." +
        "<br/>Recuerda que el nombre debe iniciar con <b>Inicial_</b>." +
        "<br/>Ejemplo: <b>Inicial_NombreArchivo.xlsx<b/>";

        setFormStatus("error");
        $("#formatErrors").html(mensaje);
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        isValid = false;
    }

    return isValid;
}

function ValidateFileFinalName($input) {
    var isValid = true;

    var fileName = $input.get(0).files[0].name.trim()
    var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');

    if (fileNameWithoutExt.toLowerCase().indexOf("final_") != 0) {
        var mensaje = "El archivo final <b>" + fileName + "</b> no tiene un nombre válido." +
            "<br/>Recuerda que el nombre debe iniciar con <b>Final_</b>." +
            "<br/>Ejemplo: <b>Final_NombreArchivo.xlsx<b/>";

        setFormStatus("error");
        $("#formatErrors").html(mensaje);
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        isValid = false;
    }

    return isValid;
}

// --- LÓGICA DE CARGA DE ARCHIVOS (Flujo unificado) ---

function beforeUploadStarts(sender, args) {
    try {
        $("#statusUploadTable").html("");

        const filename = args.get_fileName();

        var ext = filename.substring(filename.lastIndexOf(".") + 1);
        if (ext != "xlsx") {

            setTimeout(function () {
                try {
                    var input = document.getElementById(sender._inputFile.id);
                    if (input) input.value = "";
                } catch (e) {s
                    console.log(e);
                }
            }, 50);

            throw {
                name: "Invalid File Type",
                level: "Error",
                message: "Tipo de archivo inválido (Solo .xlsx).",
                htmlMessage: "Invalid File Type (Only .xlsx)."
            };
            return false;
        }
        else {
            var fileSize = sender.get_element().querySelector('input[type="file"]').files[0].size
            var maxFileSize = configuraciones.servidor.maxFileSize;
            if (fileSize > maxFileSize) {

                setTimeout(function () {
                    try {
                        var input = document.getElementById(sender._inputFile.id);
                        if (input) input.value = "";
                    } catch (e) {
                        console.log(e);
                    }
                }, 50);

                throw {
                    name: "Invalid File Size",
                    level: "Error",
                    message: "Archivo muy pesado (máximo " + (maxFileSize / 1048576).toFixed(2) + " Mb).",
                    htmlMessage: "Invalid File Size (maximum " + (maxFileSize / 1048576).toFixed(2) + " Mb)."
                };
                return false;
            }

        }

        return true;
    } catch (e) {
        console.error("Error en la validación de archivo: ", e.message);
        throw e;
    }
}
function setLoadingBar(Status, loadingValue) {
    $("#progressBar").attr("aria-valuenow", loadingValue).css("width", loadingValue + "%").html(Status + " " + loadingValue + "%");
}

function uploadError(sender, args) {
    setFormStatus("error");
    $("#formatErrors").html("Error al cargar el archivo: " + args.get_errorMessage());
}

function uploadComplete(sender, args) {
    var fileZone2 = document.getElementById("drop-zone2");

    if ((fileZone2 && $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].files[0] && $("#ctl00_ContentPlaceHolder1_AsyncFileUpload2_ctl02")[0].files[0]) || (!fileZone2)) {
        setFormStatus('newfile');
    }

}

function CheckFileExists(callBack) {
    courseFlag = true;
    setLoadingBar("Verificando archivo...", 10);
    const requestData = {
        FileType: configuraciones.carga.fileType,
        Extension: configuraciones.carga.extension,
        Screen: configuraciones.page,
        Period: $(configuraciones.carga.periodSelector).val()
    };

    $.ajax({
        type: "POST",
        url: "/api/files/checkexists",
        contentType: "application/json",
        data: JSON.stringify(requestData),

        success: function (response) {
            if (response.d === true) {
                setFormStatus("processing");
                setLoadingBar("Iniciando importación", 20);
                ValidateExcelFile(callBack, response.path);


                $("#MensajeError").text("");
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                if (response.m) {
                    document.getElementById("MensajeError").textContent = response.m;
                }
            }
        },
        error: function (xhr) {
            console.log(xhr);
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (checkexists).");
            $("#MensajeError").text("No se pudo cargar el documento por que no existe en la carpeta del servidor, Vuelva a intentar la carga ");
        }
    });
}

function ValidateExcelFile(onSuccessCallback, filePath) {
    setLoadingBar("Validando formato de Excel", 25);

    const requestData = {
        FileClass: configuraciones.carga.fileClass,
        Path: filePath,
        HeaderRow: configuraciones.carga.headerRow,
        Screen: configuraciones.page,
        Period: $(configuraciones.carga.periodSelector).val()
    };

    if (configuraciones.carga.regionSelector) {
        requestData.Region = $(configuraciones.carga.regionSelector).val();
    }

    $.ajax({
        type: "POST",
        url: "/api/files/validate",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                setLoadingBar("Iniciando importación", 50);
                if (onSuccessCallback && typeof onSuccessCallback === 'function') {
                    onSuccessCallback(requestData);
                }
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (validate).");
        }
    });
}


// --- MANEJADORES DE ESTADO Y EVENTOS ---

/**
 * Evento OnError del proceso InsertData (ID).
 * @param {object} _xr Respuesta de error.
 * @param {number} selectors Número de objetos.
 */
function ID_OnError(_xr, selectors) {
    courseFlag = false;
    setFormStatus("error", selectors);
    $("#formatErrors").html("Error de comunicación (insert).");
    $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
}

/**
 * Evento OnSuccess del proceso InsertData (ID).
 * @param {Object} response - Respuesta de la api.
 * @param {number} selectors Número de objetos.
 */
function ID_OnSuccess(response, selectors) {
    (async () => {
        try {
            courseFlag = false;
            if (response.d == 6) {
                /*Exitoso al 100 con descarga de archivo*/
                setLoadingBar("¡Completado!", 100);
                setFormStatus("success", selectors);
                $("#formatSuccess").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
                await downloadAndDeleteFile(response.f);
            } else if (response.d == 1) {
                /*Exitoso al 100 sin descarga de archivo */
                setLoadingBar("¡Completado!", 100);
                setFormStatus("success", selectors);
                $("#formatSuccess").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
            } else if (response.d == 2) {
                setLoadingBar("¡Carga parcial completada!", 100);
                setFormStatus("partial", selectors);
                $("#formatWarning").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
                await downloadAndDeleteFile(response.f);
            } else if (response.d == 3) {
                setLoadingBar("¡Completado con Errores!", 100);
                setFormStatus("error", selectors);
                $("#formatErrors").html(response.r);
                if (typeof activateSuccess === "function") { activateTable(); };
                if (response.f) {
                    await downloadAndDeleteFile(response.f);
                };
            } else if (response.d == 4) {
                setLoadingBar("No se cargaron datos.", 100);
                setFormStatus("error", selectors);
                $("#formatErrors").html(response.r);
                if (typeof activateSuccess === "function") { activateTable(); }
                await downloadAndDeleteFile(response.f);
            } else if (response.d == 5) {
                setLoadingBar("¡Proceso Incompleto!", 90);
                setFormStatus("error", selectors);
                $("#formatErrors").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
                await downloadAndDeleteFile(response.f);
                Swal.fire({
                    icon: 'warning',
                    iconColor: '#FFA500',
                    title: 'Registros inválidos encontrados',
                    html: '<p>No fue posible concluir la carga de <strong>' + pageConfig.LogPage + '</strong>.<br>' +
                        '¿Deseas cargar únicamente la información válida o corregir el archivo para una nueva importación?<br><br>' +
                        '<small>Recuerda que cada importación reemplaza la información existente; asegúrate de incluir todos los registros.<small></p> ',
                    showCancelButton: true,
                    confirmButtonText: 'Continuar con la carga',
                    cancelButtonText: 'Corregir archivo y reintentar',
                    confirmButtonColor: '#28a745', // verde
                    cancelButtonColor: '#dc3545',  // rojo
                    reverseButtons: true,
                    width: '600px',
                    background: '#fefefe',
                }).then((result) => {
                    if (result.isConfirmed) {
                        setFormStatus("processing", selectors);
                        setLoadingBar("Realizando carga parcial...", 90);
                        $.ajax({
                            type: "POST",
                            url: pageConfig.apiUploadData,
                            contentType: "application/json",
                            success: InsertData_OnSuccess,
                            error: InsertData_OnError
                        });
                    }
                });
            } else {
                setFormStatus("error", selectors);
                $("#formatErrors").html(response.r);
                if (typeof activateTable === "function") { activateTable(); }
                $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
                if (response.f) {
                    await downloadAndDeleteFile(response.f);
                };
            }
        } catch (error) {
            console.log("Error dentro del manejador asincrono: ", error)
        }
    })();
}

/**
 * Evento OnComplete del proceso InsertData.
 * @param {object} requestData Request Data.
 */
function InsertData_OnComplete(requestData) {
    DeleteFile(serverPath + "UploadedFiles\\" + requestData.FileType + "\\" + userEmail + requestData.Extension);
}

/**
 * Evento OnError del proceso InsertData.
 * @param {object} xr Respuesta de error.
 */
function InsertData_OnError(xr) {
    ID_OnError(xr, 0);
}

/**
 * Evento OnSuccess del proceso InsertData.
 * @param {Object} response Respuesta de la api.
 */
function InsertData_OnSuccess(response) {
    ID_OnSuccess(response, 0);
}

/**
 * Evento OnError del proceso InsertDataTwoFiles.
 * @param {object} xr Respuesta de error.
 */
function InsertDataTwoFiles_OnError(xr) {
    ID_OnError(xr, 2);
}

/**
 * Evento OnSuccess del proceso InsertDataTwoFiles.
 * @param {Object} response Respuesta de la api.
 */
function InsertDataTwoFiles_OnSuccess(response) {
    ID_OnSuccess(response, 2);
}

/**
 * Pone el estatus de procesando a un botón.
 * @param {Object} $button Botón
 */
function setButtonProcess($button) {
    $button.html("<i class='fas fa-sync-alt fa-spin fa-fw'></i>Procesando...");
}

function setFormStatus(status, selectors = 0) {
    $("#errorPanel, #successPanel, #WarningPanel").fadeOut("fast")
    switch (status) {
        case 'newfile':
            $("#progressDiv").fadeOut("fast");
            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Iniciar Importación');
            break;
        case 'processing':
            $("#fileName").html("");
            $("#statusAlert").html("<i class='fas fa-exclamation-triangle fa-fw'></i><strong>Importante!</strong> Espere por favor, no actualice la página...");
            $("#statusAlert").addClass("show")

            $("#progressDiv").fadeIn("slow");
            $("#progressBar").fadeIn("slow");
            $("#progressBar").removeClass("bg-danger").addClass("bg-success").addClass("progress-bar-animated");

            $('#myForm').find('input, file, button, select').attr('disabled', true);

            $("#btnStartImport").attr('disabled', true);
            $("#btnStartImport").html("<i class='fas fa-sync-alt fa-spin fa-fw'></i>Procesando...");
            break;
        case 'error':
            $("#errorPanel").fadeIn("slow");
            $("#statusUploadTable").html("");
           
            $(configuraciones.carga.selector).find('input[type=file]').val("").css("background-color", "#fff");
            
            File = "-1";
            $("#statusAlert").removeClass("show");
            $("#progressBar").removeClass("progress-bar-animated").removeClass("bg-success").addClass("bg-danger");
            $('#myForm').find('input, file, button, select').attr('disabled', false);
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
            $(".bootstrap-filestyle").find("input[type=text]").val("").attr("disabled", true);
            break;
        case 'fail':
            $("#statusUploadTable").html("");            
            $(configuraciones.carga.selector).find('input[type=file]').val("").css("background-color", "#fff");
            
            File = "-1";
            $(".bootstrap-filestyle").find("input[type=text]").val("").attr("disabled", true);
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
            break;
        case 'success':
            $("#successPanel").fadeIn("slow");
            $("#statusUploadTable").html("");
            $(configuraciones.carga.selector).find('input[type=file]').val("").css("background-color", "#fff");
            
            File = "-1";

            $("#statusAlert").removeClass("show");
            $("#progressBar").removeClass("progress-bar-animated")
            $('#myForm').find('input, file, button, select').attr('disabled', false);
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Iniciar Importación');
            $(".bootstrap-filestyle").find("input[type=text]").val("").attr("disabled", true);
            break;
        case 'partial':
            $("#WarningPanel").fadeIn("slow");
            $("#statusUploadTable").html("");
            $(configuraciones.carga.selector).find('input[type=file]').val("").css("background-color", "#fff");
         
            File = "-1";

            $("#statusAlert").removeClass("show");
            $("#progressBar").removeClass("progress-bar-animated")
            $('#myForm').find('input, file, button, select').attr('disabled', false);
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Iniciar Importación');
            $(".bootstrap-filestyle").find("input[type=text]").val("").attr("disabled", true);
            break;
        default:
           // alert('default');
    }
}
function activateTable() {
    if ($.fn.dataTable.isDataTable('#Table')) {        
        $('#Table').DataTable().destroy();
    }
    $('#Table').DataTable({        
        "lengthMenu": [[10, 20, 30, -1], [10, 20, 30, "Todos"]],
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
                    filename: "Errores en carga de xlsx",
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
};

function activateSuccess() {
    if ($.fn.dataTable.isDataTable('#Table')) {        
        $('#Table').DataTable().destroy();
    }

    $('#Table').DataTable({        
        "lengthMenu": [[10, 20, 30, -1], [10, 20, 30, "Todos"]],
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
                    filename: "Respuesta ICMTools",
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
};

function DeleteFile(path) {
    const requestData = {
        FilePath: path
    };
    $.ajax({
        type: "DELETE",
        url: "/api/files/delete",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error al eliminar el Excel del servidor, favor de comunicarse con soporte.");
        }
    });
}

function DeleteAll() {
    const requestData = {
        FilePath: pageConfig.FileType
    };
    $.ajax({
        type: "DELETE",
        url: "/api/files/deleteall",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error al eliminar los archivos del servidor, favor de comunicarse con soporte.");
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    var asyncFileInput = document.querySelector('#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02');
    var asyncFileInput2 = document.querySelector('#ctl00_ContentPlaceHolder1_AsyncFileUpload2_ctl02');

    if (asyncFileInput) {
        asyncFileInput.addEventListener('change', function () {
            AsyncFileUpload_Change(this);
        });
    }

    if (asyncFileInput2) {
        asyncFileInput2.addEventListener('change', function () {
            AsyncFileUpload_Change(this);
        });
    }
});

function AsyncFileUpload_Change(input) {
    if (input.value) {
    } else {
        $("#btnStartImport").attr('disabled', true);
        $(input).css("background-color", "#fff");
    }
}

async function downloadAndDeleteFile(serverFilePath) {
    if (!serverFilePath) {
        console.error("Se requiere de una ruta de archivo valida para descargar y eliminar");
        return;
    }
    try {
        const filename = serverFilePath.split('\\').pop().split('/').pop();
        const downloadUrl = "/api/files/download?filename=" + encodeURIComponent(filename);
        const fileResponse = await fetch(downloadUrl)
        if (!fileResponse.ok) {
            throw new Error(`La descarga del archivo de resultados falló con estado: ${fileResponse.status}`);
        }
        const blob = await fileResponse.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click()
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a)

        DeleteFile(serverFilePath)
    } catch (error) {
        console.log("Ocurrio un error durante la descarga o limpieza del archivo.")
    }
}

function mostrarModalBloqueo(mensajePersonalizado = null) {
    if (mensajePersonalizado) {
        $("#mensajeBloqueoGlobal").html(mensajePersonalizado);
    } 
    $("#modalGlobalBloqueo").modal("show");
}