function CheckExcelFileArqueos() {
    CheckFileExists(configuraciones.carga, ValidarInformacionArqueos)
}


function ValidarInformacionArqueos(dataRequest) {

    console.log(dataRequest.FileClass);
    console.log(dataRequest.Path);

    $.ajax({
        type: "POST",
        url: "/api/arqueos/validarinfo",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),

        success: function (response) {
            if (response.d === true) {
                setFormStatus("processing");
                setLoadingBar("Validando datos", 60);
                GenerarCSV(response.path);

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
            $("#formatErrors").html("Error de comunicación (ValidarInformacionArqueos).");
            $("#MensajeError").text("No se pudo cargar el documento por que no existe en la carpeta del servidor, Vuelva a intentar la carga ");
        }
    });
}


function GenerarCSV(path) {


    console.log("JIJIIA")
    var fileName = '';
    var fileCCNomina = '';

    try {

        fileName = $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].files[0].name.trim()
        $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02").prop("last_uploaded_file_name", fileName)
        var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');
        var extension = "." + fileName.split('.').pop();
        if (fileNameWithoutExt.includes('_')) {
            fileCCNomina = fileNameWithoutExt.substring(fileNameWithoutExt.lastIndexOf('_') + 1);
        }
    } catch (e) {
    }

    if (fileCCNomina === '') {
        setFormStatus("error");
        $("#formatErrors").html("El nombre del archivo no es válido (" + fileName + "). Debe finalizar con el CCNomina, precedido por un guion bajo. Ejemplo: <b>NombreArchivo_118A.xlsx</b>");
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');

        return;
    }

    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: configuraciones.carga.fileType,
        LogBody: fileCCNomina,
        Extension: configuraciones.carga.extension
    }
    $.ajax({
        type: "POST",
        url: "/api/arqueos/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}
