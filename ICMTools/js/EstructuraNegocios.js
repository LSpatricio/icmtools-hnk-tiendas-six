function CheckExcelFileMDC() {
    CheckFileExists(configuraciones.carga, CargarInformacion)
}


function CargarInformacion(dataRequest) {

    setFormStatus("processing");
    setLoadingBar("Validando datos", 50);

    $.ajax({
        type: "POST",
        url: "/api/estructuranegocios/cargarinfo",
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


function EnviarInformacion(dataRequest) {
    setFormStatus("processing");
    setLoadingBar("Cargando información", 85);

        $.ajax({
            type: "POST",
            url: "/api/estructuranegocios/enviarinformacion",
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



