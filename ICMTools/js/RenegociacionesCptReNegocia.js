function CheckExcelFileRenegociaciones() {
    CheckFileExists(CargarRenegociaciones);
}

function CargarRenegociaciones(dataRequest) {
    setFormStatus("processing");
    setLoadingBar("Validando datos", 50);

    $.ajax({
        type: "POST",
        url: "/api/renegociaciones/cargarinfo",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),
        success: function (response) {
            if (response.d === true) {
                const rutaSalida = dataRequest.Path.replace(/[^\\]+$/, "Salida");
                EnviarRenegociaciones({ PathSalida: rutaSalida, IdGui: response.id, Screen: dataRequest.Screen, Period: dataRequest.Period });
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
            }
        },
        error: function () {
            setFormStatus("error");
            $("#formatErrors").html("Error en la validación y carga.");
            $("#MensajeError").text("No se pudo validar y cargar el documento. Revise el documento y vuelva a intentar.");
        }
    });
}

function EnviarRenegociaciones(dataRequest) {
    setFormStatus("processing");
    setLoadingBar("Cargando información", 85);

    $.ajax({
        type: "POST",
        url: "/api/renegociaciones/enviarinformacion",
        contentType: "application/json",
        data: JSON.stringify(dataRequest),
        success: function (response) {
            if (response.d === true) {
                setLoadingBar("Carga Completa!", 100);
                setFormStatus("success");
                $("#formatSuccess").html("Archivo enviado.");
            }
        },
        error: function () {
            setFormStatus("error");
            $("#formatErrors").html("Error al enviar la información.");
            $("#MensajeError").text("No se pudo enviar el documento a ICM.");
        }
    });
}
