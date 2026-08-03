function InsertDataVentaMonto() {
    var fileCCNomina = '';
    var fileName = $(pageConfig.fileUploadSelector).find("input[type='file']").get(0).files[0].name.trim()
    var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');

    if (fileNameWithoutExt.includes('_')) {
        fileCCNomina = fileNameWithoutExt.substring(fileNameWithoutExt.lastIndexOf('_') + 1);
    }

    if (fileCCNomina === '') {
        setFormStatus("error");
        $("#formatErrors").html("El nombre del archivo no es válido (" + fileName + "). <br/>Debe finalizar con el CCNomina, precedido por un guion bajo. <br/>Ejemplo: <b>NombreArchivo_118A.xlsx</b>");
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        return;
    }

    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension,
        LogBody: fileCCNomina
    }
    $.ajax({
        type: "POST",
        url: "/api/ventamonto/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}

function CheckExcelFileTG() {
    CheckFileExists(InsertDataVentaMonto)
}