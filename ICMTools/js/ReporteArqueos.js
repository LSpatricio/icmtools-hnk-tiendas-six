function CheckExcelFileReporteArqueos() {
    CheckFileExists(configuraciones.carga, ValidarInformacionReporteArqueos)
}


function ValidarInformacionReporteArqueos(dataRequest) {

    console.log(dataRequest.FileClass);
    console.log(dataRequest.Path);

    $.ajax({
        type: "POST",
        url: "/api/reportearqueos/validarinfo",
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
                $("#formatErrors").html(response.r || response.d);
                if (typeof activateTable === "function") { activateTable(); }
                if (response.m) {
                    document.getElementById("MensajeError").textContent = response.m;
                }
            }
        },
        error: function (xhr) {
            console.log(xhr);
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicaciÃ³n (ValidarInformacionReporteArqueos).");
            $("#MensajeError").text("No se pudo cargar el documento por que no existe en la carpeta del servidor, Vuelva a intentar la carga ");
        }
    });
}


function GenerarCSV(path) {
    var fileName = '';
    var fileNameWithoutExt = '';

    try {
        fileName = $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].files[0].name.trim();
        $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02").prop("last_uploaded_file_name", fileName);
        fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');
    } catch (e) {
    }

    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: configuraciones.carga.fileType,
        LogBody: fileNameWithoutExt,
        Extension: configuraciones.carga.extension
    };

    $.ajax({
        type: "POST",
        url: "/api/reportearqueos/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}
