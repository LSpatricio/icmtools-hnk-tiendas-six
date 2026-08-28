function CheckExcelFileMDC() {
    CheckFileExists(configuraciones.carga, CargarInformacion)
}

function CargarInformacion(dataRequest) {

    setFormStatus("processing");
    setLoadingBar("Validando datos", 50);

    $.ajax({
        type: "POST",
        url: "/api/registrosadc/cargarinfo",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),

        success: function (response) {
            if (response.d === true) {

                const rutaSalida = dataRequest.Path.replace(/[^\\]+$/, "Salida");

                const requestData = {
                    PathSalida: rutaSalida,
                    IdGui: response.id
                };



                EnviarInformacion(requestData);


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
            $("#formatErrors").html("Error en la validación y carga.");
            $("#MensajeError").text("No se pudo validar y cargar el documento. Revise el documento y vuelva a intentar.");
        }
    });
}


//function GenerarCSV(path) {


//    console.log("JIJIIA")
//    var fileName = '';
//    var fileNameWithoutExt = '';

//    try {

//        fileName = $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].files[0].name.trim()
//        $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02").prop("last_uploaded_file_name", fileName)
//        fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');
//        //var extension = "." + fileName.split('.').pop();
//        //if (fileNameWithoutExt.includes('_')) {
//        //    fileCCNomina = fileNameWithoutExt.substring(fileNameWithoutExt.lastIndexOf('_') + 1);
//        //}
//    } catch (e) {
//    }

//    //if (fileCCNomina === '') {
//    //    setFormStatus("error");
//    //    $("#formatErrors").html("El nombre del archivo no es válido (" + fileName + "). Debe finalizar con el CCNomina, precedido por un guion bajo. Ejemplo: <b>NombreArchivo_118A.xlsx</b>");
//    //    $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');

//    //    return;
//    //}

//    setLoadingBar("Insertando datos...", 75);
//    const requestData = {
//        FileType: configuraciones.carga.fileType,
//        LogBody: fileNameWithoutExt,
//        Extension: configuraciones.carga.extension
//    }
//    $.ajax({
//        type: "POST",
//        url: "/api/registrosadc/insertdata",
//        contentType: "application/json",
//        data: JSON.stringify(requestData),
//        success: InsertData_OnSuccess,
//        error: InsertData_OnError,
//        complete: function () {
//            InsertData_OnComplete(requestData);
//        }
//    });
//}

function EnviarInformacion(dataRequest) {
    setFormStatus("processing");
    setLoadingBar("Cargando información", 85);

    $.ajax({
        type: "POST",
        url: "/api/registrosadc/enviarinformacion",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),

        success: function (response) {
            if (response.d === true) {

                setLoadingBar("Carga Completa!", 100);
                setFormStatus("success");

                $("#formatSuccess").html("Archivo enviado.");

                $("#MensajeError").text("");
            }
        },
        error: function (xhr) {
            console.log(xhr);
            setFormStatus("error");
            $("#formatErrors").html("Error en la validación y carga.");
            $("#MensajeError").text("No se pudo enviar el documento a ICM. Error de comunicación");
        }
    });
}
