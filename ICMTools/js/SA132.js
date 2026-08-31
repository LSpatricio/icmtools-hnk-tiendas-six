function CheckExcelFileSA132() {
    CheckFileExists(CargarInformacion);
}

function CargarInformacion(dataRequest) {
    setFormStatus("processing");
    setLoadingBar("Validando datos", 50);

    $.ajax({
        type: "POST",
        url: "/api/sa132/cargarinfo",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),
        success: function (response) {
            if (response.d === true) {
                const rutaSalida = dataRequest.Path.replace(/[^\\]+$/, "Salida");
                const requestData = {
                    PathSalida: rutaSalida,
                    IdGui: response.id,
                    Screen: dataRequest.Screen,
                    Period: dataRequest.Period
                };

                EnviarInformacion(requestData);
                $("#MensajeError").text("");
                return;
            }

            setFormStatus("error");
            $("#formatErrors").html(response.d);
            if (typeof activateTable === "function") activateTable();
        },
        error: function (xhr) {
            console.log(xhr);
            setFormStatus("error");
            $("#formatErrors").html("Error durante la validacion y carga de SA132.");
            $("#MensajeError").text("No se pudo procesar el documento. Revise el archivo y vuelva a intentar.");
        }
    });
}

function EnviarInformacion(dataRequest) {
    setFormStatus("processing");
    setLoadingBar("Enviando informacion", 85);

    $.ajax({
        type: "POST",
        url: "/api/sa132/enviarinformacion",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),
        success: function (response) {
            if (response.d === true) {
                setLoadingBar("Carga completa", 100);
                setFormStatus("success");
                $("#formatSuccess").html("Archivo de SA132 enviado.");
            }
        },
        error: function (xhr) {
            console.log(xhr);
            setFormStatus("error");
            $("#formatErrors").html("La informacion se cargo, pero no pudo enviarse al SFTP.");
            $("#MensajeError").text("No se pudo completar el envio de SA132.");
        }
    });
}
