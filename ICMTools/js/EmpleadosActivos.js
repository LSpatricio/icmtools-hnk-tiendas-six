function CheckExcelFileTG() {
    $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-sync-alt fa-spin fa-fw"></i> Procesando...');
    setLoadingBar("Verificando archivos...", 10);

    validarArchivos(0, InsertDataEmpleadosActivos)
}

function validarArchivos(index, onSuccessCallback) {    
    if (index >= pageConfigs.length) {
        setLoadingBar("Iniciando importación", 50);
        if (onSuccessCallback && typeof onSuccessCallback === 'function') {
            onSuccessCallback();
        };
        return;
    }

    var $input = $(pageConfigs[index].fileUploadSelector).find("input[type='file']");
    if (index == 0) {
        if (!ValidateFileInitialName($input)) {
            return;
        }
    }
    else {
        if (!ValidateFileFinalName($input)) {
            return;
        }
    }

    CheckFileExistsEmpleadosActivos(pageConfigs[index], index, function (exists) {
        if (exists === true) {            
            ValidateExcelFileEmpleadosActivos(pageConfigs[index], function () {
                validarArchivos(index + 1, onSuccessCallback)
            });
        }
        else {
            setFormStatus("error", 2);
        }
    });
}

function CheckFileExistsEmpleadosActivos(v, index, funcionAn) {
    courseFlag = true;

    const progreso = Math.round((index + 1) * (20 / pageConfigs.length))
    const requestData = {
        FileType: v.FileType,
        Extension: pageConfig.Extension
    };

    $.ajax({
        type: "POST",
        url: "/api/files/checkexists",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                setFormStatus("processing", 2);
                setLoadingBar("Iniciando importación", progreso);
            }
            else {
                $("#formatErrors").html(response.d);

                if (response.m) {
                    document.getElementById("MensajeError").textContent = response.m;
                }
            }
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Procesando...');

            funcionAn(response.d)
        },
        error: function (xhr) {
            setFormStatus("error", 2);
            $("#formatErrors").html("Error de comunicación (checkexists).");
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Procesando...');
        }
    });
}

function ValidateExcelFileEmpleadosActivos(v, funcionAnValidarArchivos) {
    const requestData = {
        FileType: v.FileType,
        Extension: pageConfig.Extension,
        columns: pageConfig.columns,
        types: pageConfig.types,
        LogPage: v.LogPage,
        LogType: v.LogType,
        LogBody: v.LogBody,
        nulleable_columns: pageConfig.nulleable_columns
    }
    $.ajax({
        type: "POST",
        url: "/api/files/validate",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {                
                funcionAnValidarArchivos()
            } else {
                setFormStatus("error", 2);
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
            }
        },
        error: function (xhr) {
            setFormStatus("error", 2);
            $("#formatErrors").html("Error de comunicación (validate).");
            $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        }
    });
}

function InsertDataEmpleadosActivos() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfigs[0].FileType,
        FileType2: pageConfigs[1].FileType,
        Extension: pageConfig.Extension
    };
    let downloadUrl = null;
    let filename = null;
    $.ajax({
        type: "POST",
        url: "/api/empleadosactivos/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertDataTwoFiles_OnSuccess,
        error: InsertDataTwoFiles_OnError,
        complete: function () {
            DeleteFile(serverPath + "UploadedFiles\\" + requestData.FileType + "\\" + userEmail + requestData.Extension);
            DeleteFile(serverPath + "UploadedFiles\\" + requestData.FileType2 + "\\" + userEmail + requestData.Extension);

            pageConfig.CargaPrevia = "start";
            pageConfig.CargaPrevia1 = "start";
            
            var input1 = document.getElementById("ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02");
            var hidden1 = document.getElementById("ContentPlaceHolder1_AsyncFileUpload1_ctl00");
            if (input1) input1.value = "";
            if (hidden1) hidden1.value = "";
 
            var input2 = document.getElementById("ctl00_ContentPlaceHolder1_AsyncFileUpload2_ctl02");
            var hidden2 = document.getElementById("ContentPlaceHolder1_AsyncFileUpload2_ctl00");
            if (input2) input2.value = "";
            if (hidden2) hidden2.value = "";

            if (filename) {
                setTimeout(function () {
                    DeleteFile(serverPath + "UploadedFiles\\" + filename);
                }, 1000 * 60);
            }
            if (downloadUrl) {
                window.location.href = downloadUrl;
            }           
        }
    });
}