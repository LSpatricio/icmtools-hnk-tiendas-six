function StartImport() {
    Society = $("#SelectSociety").val();
    PersonnelDivision = $("#SelectPersonnelDivision").val();
    DisableButton();
}

function DisableButton() {
    $('#btnStartImport').prop('disabled', true);
    $('#btnStartImport').tooltip('hide');
}

function addToClientTable(name, text) {
    var table = $("#statusUploadTable");
    table.html("<tr><th>Archivo</th><th>Error</th></tr><tr><td>" + name + "</td><td>" + text + "</td></tr>");
}

function StartImportTiendas() {
    StartImport();
    CheckFileExistsTiendas();
}

function CheckFileExistsTiendas() {
    CheckFileExists(ValidateCustomTiendas);
}

function ValidateCustomTiendas() {
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension,
        columns: pageConfig.columns,
        types: pageConfig.types,
        Society: $("#SelectSociety").val(),
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    }
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/tiendas/validatecustomtiendas",
        data: JSON.stringify(requestData),
        success: function (response) {
            (async () => {
                try {
                    if (response.t) {
                        $("#formatErrors").html(response.t);
                        activateTable();
                        setFormStatus("error");
                        await downloadAndDeleteFile(response.f);
                        EliminarArchivo(requestData);
                    } else if (response.d === true) {
                        InsertTiendas()
                    } else {
                        $("#formatErrors").html(response.d);
                        if ($("#formatErrors").find("table").length > 0) { activateTable(); }
                        setFormStatus("error");
                        await downloadAndDeleteFile(response.f);
                        EliminarArchivo(requestData);
                    };
                }
                catch (e) {
                    console.error("Error en el manejador async de success: ", e);
                }
            })();
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (validate).");
            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        }
    })
}

function InsertTiendas() {
    var token = $('input[name="__RequestVerificationToken"]').val();
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/tiendas/InsertInfoBDTiendas",
        data: JSON.stringify(requestData),
        headers: {
            'X-XSRF-Token': token
        },
        success:
            function (response) {
                if (response.d == true) {
                    setFormStatus("processing");
                    setLoadingBar("Exportado", 75);
                    ConfirmExceptionsT();
                } else {
                    $("#formatErrors").html(response.d);              
                    activateTable();                    
                    setFormStatus("error");
                };
            },
        error: InsertData_OnError
    });
}

function ConfirmExceptionsT() {
    setLoadingBar("Finalizando proceso", 95);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/tiendas/ConfirmExceptionsT",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}

function StartImportExcepciones() {
    CheckFileExistsExcepciones();
}

function CheckFileExistsExcepciones() {
    CheckFileExists(ValidateCustomExcepciones);
}

function ValidateCustomExcepciones() {
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension,
        columns: pageConfig.columns,
        types: pageConfig.types,
        Society: $("#SelectSociety").val(),
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    }
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/tiendas/validatecustomexcepciones",
        data: JSON.stringify(requestData),
        success: function (response) {
            (async () => {
                try {
                    if (response.t) {
                        $("#formatErrors").html(response.t);
                        activateTable();
                        setFormStatus("error");
                        await downloadAndDeleteFile(response.f);
                        EliminarArchivo(requestData);
                    } else if (response.d === true) {
                        InsertExcepciones();
                    } else {
                        $("#formatErrors").html(response.d);
                        if ($("#formatErrors").find("table").length > 0) { activateTable(); }
                        setFormStatus("error");
                        await downloadAndDeleteFile(response.f);
                        EliminarArchivo(requestData);
                    };
                }
                catch (e) {
                    console.error("Error en el manejador async de success: ", e);
                }
            })();
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (validate).");
            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        }
    })
}

function InsertExcepciones() {
    var token = $('input[name="__RequestVerificationToken"]').val();
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/tiendas/InsertInfoBDExcepciones",
        data: JSON.stringify(requestData),
        headers: {
            'X-XSRF-Token': token
        },
        success:
            function (response) {
                if (response.d == true) {
                    setFormStatus("processing");
                    setLoadingBar("Exportado", 75);
                    ConfirmExceptionsE();
                } else {
                    setFormStatus("error");
                    $("#formatErrors").html(response.r);
                    if (typeof activateTable === "function") { activateTable(); }
                    $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
                };
            },
        error: InsertData_OnError
    });
}

function ConfirmExceptionsE() {
    setLoadingBar("Finalizando proceso", 95);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/tiendas/ConfirmExceptionsE",
        data: JSON.stringify(requestData),
        dataType: "json",
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}

function EliminarArchivo(requestData) {
    if (requestData) {
        var file = serverPath + "UploadedFiles\\" + requestData.FileType + "\\" + userEmail + requestData.Extension;
        DeleteFile(file);
    };
}