/// <reference path="shared.js" />

function beforeUploadStartsIV(sender, args) {
    try {
        $("#statusUploadTable").html("");

        const filename = args.get_fileName();
        var ext = filename.substring(filename.lastIndexOf(".") + 1);
        if (ext != "csv") {

            setTimeout(function () {
                try {
                    var input = document.getElementById(sender._inputFile.id);
                    if (input) input.value = "";
                } catch (e) {
                    console.log(e);
                }
            }, 50);

            throw {
                name: "Invalid File Type",
                level: "Error",
                message: "Tipo de archivo inválido, esta pantalla solo admite .csv",
                htmlMessage: "Invalid File Type (Only .csv)."
            };
            return false;
        }
        else {
            var fileSize = $("#ctl00_" + sender.get_element().id + "_ctl02")[0].files[0].size
            var maxFileSize = pageConfig.maxFileSize;
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

/**
 * Inserta Información
* Paso 5 - Importación
*/
function InsertDataImportVentas() {
    var $ResponseCSV = $(".ResponseCSV");
    setLoadingBar("Insertando datos, esto tomará varios minutos.", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/importventas/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                Ending();
            } else {
                $ResponseCSV.show();
                setFormStatus("error");
                $("#formatErrors").html(response.r);
                if (typeof activateTable === "function") { activateTable(); }
                DeleteAll();
            }
        },
        error: function (_xr) {
            courseFlag = false;
            $ResponseCSV.hide();
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (insert).");
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        },
        complete: function () {
            DeleteAll();
        }
    });
}

/**
 * Finalizar Proceso
* Paso 6 - Importación
*/
function Ending() {
    setLoadingBar("Finalizando...", 90);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/importventas/ending",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            (async () => {
                try {
                    if (response.d == 1) {
                        setLoadingBar("¡Completado!", 100);
                        setFormStatus("success");
                        $("#formatSuccess").html(response.r);
                        if (typeof activateSuccess === "function") { activateSuccess(); }
                    } else if (response.d == 2) {
                        setLoadingBar("¡Carga parcial completada!", 100);
                        setFormStatus("partial");
                        $("#formatWarning").html(response.r);
                        if (typeof activateSuccess === "function") { activateSuccess(); }
                    } else if (response.d == 3) {
                        setLoadingBar("No se cargaron datos.", 100);
                        setFormStatus("error");
                        $("#formatErrors").html(response.r);
                        if (typeof activateSuccess === "function") { activateTable(); }
                    } else {
                        setLoadingBar("No se cargaron datos.", 100);
                        setFormStatus("error");
                        $("#formatErrors").html(response.r);
                        if (typeof activateTable === "function") { activateTable(); }
                        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
                    }
                } catch (error) {
                    console.log("Error dentro del manejador asincrono: ", error)
                }
            })();
        },
        error: InsertData_OnError,
        complete: function () {
            DeleteAll();
        }
    });
}

/**
Carga los catálogos 
Paso 3 - Importación
*/
function LoadCatalogs() {
    setLoadingBar("Cargando catalogos...", 50);
    $.ajax({
        type: "POST",
        url: "/api/importventas/loadcatalogs",
        contentType: "application/json",
        success: function (response) {
            if (response.d === true) {
                LoadFile();
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                DeleteAll();
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (loadcatalogs).");
            DeleteAll();
        }
    });
}

/**
Carga el archivo 
Paso 4 - Importación
*/
function LoadFile() {
    setLoadingBar("Cargando archivo...", 60);
    $.ajax({
        type: "POST",
        url: "/api/importventas/loadfile",
        contentType: "application/json",
        success: function (response) {
            if (response.d === true) {
                InsertDataImportVentas();
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                DeleteAll();
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (loadcatalogs).");
            DeleteAll();
        }
    });
}

/**
* Paso 1 - Importación
*/
function CheckExcelFileIV() {
    courseFlag = true;
    setLoadingBar("Verificando archivo por segmentos...", 10);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    };

    $.ajax({
        type: "POST",
        url: "/api/importventas/processchunks",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                setFormStatus("processing");
                setLoadingBar("Iniciando importación", 20);
                ValidateExcelFileCustomIV();
                $("#MensajeError").text("");
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                if (response.m) {
                    document.getElementById("MensajeError").textContent = response.m;
                }
                DeleteAll();
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (checkexists).");
            $("#MensajeError").text("No se pudo cargar el documento por que uno o varios segmentos del csv no existen en la carpeta del servidor, Vuelva a intentar la carga ");
            DeleteAll();
        }
    });
}

/**
* Paso 2 - Importación
*/
function ValidateExcelFileCustomIV() {
    setLoadingBar("Validando formato de los segmentos del archivo...", 25);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension,
        columns: pageConfig.columns,
        types: pageConfig.types,
        LogPage: pageConfig.LogPage,
        LogType: pageConfig.LogType,
        LogBody: pageConfig.LogBody
    }
    $.ajax({
        type: "POST",
        url: "/api/importventas/validate",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                LoadCatalogs();
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                DeleteAll();
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (validate).");
            DeleteAll();
        }
    });
}

function DownloadResponse() {
    var $ResponseCSV = $(".ResponseCSV");
    var ResponseCsvHtml = $ResponseCSV.html();
    setButtonProcess($ResponseCSV)

    $.ajax({
        type: "POST",
        url: "/api/importventas/dresponse",
        contentType: "application/json",
        success: function (response) {
            (async () => {
                try {
                    await downloadAndDeleteFile(response.f);
                } catch (error) {
                    console.log("Error dentro del manejador asincrono: ", error)
                }
                finally {
                    $ResponseCSV.html(ResponseCsvHtml);
                }
            })();
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (validate).");
        },
        complete: function () {
            $ResponseCSV.html(ResponseCsvHtml);
        }
    });
}
