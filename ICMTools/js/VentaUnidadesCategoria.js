function Step1_CheckFile() {
    CheckFileExists(Step2_LoadCatalogs)
}

/**
Carga los catálogos 
*/
function Step2_LoadCatalogs() {
    var fileName = '';
    var fileCCNomina = '';

    try {
        fileName = $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].files[0].name.trim()
        $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02").prop("last_uploaded_file_name", fileName)
        var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');
        if (fileNameWithoutExt.includes('ICMToolsPlantilla_')) {
            fileNameWithoutExt = fileNameWithoutExt.replace('ICMToolsPlantilla_', '');
        }
        if (fileNameWithoutExt.includes('_')) {
            fileCCNomina = fileNameWithoutExt.substring(fileNameWithoutExt.lastIndexOf('_') + 1);
        }
    } catch (e) {
        setFormStatus("error");
        $("#formatErrors").html("El nombre del archivo no es válido (" + fileName + "). Debe finalizar con el CCNomina, precedido por un guion bajo. Ejemplo: <b>NombreArchivo_118A.xlsx</b>");
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        return;
    }

    if (fileCCNomina === '') {
        setFormStatus("error");
        $("#formatErrors").html("El nombre del archivo no es válido (" + fileName + "). Debe finalizar con el CCNomina, precedido por un guion bajo. Ejemplo: <b>NombreArchivo_118A.xlsx</b>");
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        return;
    }

    const requestData = {
        FileType: pageConfig.FileType,
        LogBody: fileCCNomina,
        LogPage: pageConfig.LogPage,
        Extension: pageConfig.Extension,
        AllowDuplicateEntries: pageConfig.allowDuplicateEntries
    }

    setLoadingBar("Cargando catalogos...", 50);
    $.ajax({
        type: "POST",
        url: "/api/ventaunidadescategorias/loadcatalogs",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                Step3_InsertData();
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

function Step3_InsertData() {
    var fileName = '';
    var fileCCNomina = '';

    fileName = $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].files[0].name.trim()
    $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02").prop("last_uploaded_file_name", fileName)
    var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');
    if (fileNameWithoutExt.includes('ICMToolsPlantilla_')) {
        fileNameWithoutExt = fileNameWithoutExt.replace('ICMToolsPlantilla_', '');
    }
    if (fileNameWithoutExt.includes('_')) {
        fileCCNomina = fileNameWithoutExt.substring(fileNameWithoutExt.lastIndexOf('_') + 1);
    }

    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        LogBody: fileCCNomina,
        LogPage: pageConfig.LogPage,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/ventaunidadescategorias/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}