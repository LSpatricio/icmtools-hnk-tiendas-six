function InsertDataTiendasGanadoras() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/empleadoslideres/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            courseFlag = false
            if (response.d == 1) {
                setLoadingBar("¡Completado!", 100);
                setFormStatus("success");
                $("#formatSuccess").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
            } else if (response.d == 2) {
                setLoadingBar("¡Carga parcial completada!", 100);
                setFormStatus("partial");
                $("#formatWarning").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
                const filename = response.f.split('\\').pop().split('/').pop();
                const downloadUrl = "/api/files/download?filename=" + encodeURIComponent(filename);
                window.location.href = downloadUrl;
                setTimeout(function () {
                    DeleteFile(serverPath + "UploadedFiles\\" + filename);
                }, 1000 * 60);
            }
            else {
                setFormStatus("error");
                $("#formatErrors").html(response.r);
                if (typeof activateTable === "function") { activateTable(); }
                $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
                if (response.f) {
                    const filename = response.f.split('\\').pop().split('/').pop();
                    const downloadUrl = "/api/files/download?filename=" + encodeURIComponent(filename);
                    window.location.href = downloadUrl;
                    setTimeout(function () {
                        DeleteFile(serverPath + "UploadedFiles\\" + filename);
                    }, 1000 * 60);
                }
            }
        },
        error: function (xhr) {
            courseFlag = false
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (insert).");
            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        },
        complete: function () {
            DeleteFile(serverPath + "UploadedFiles\\" + requestData.FileType + "\\" + userEmail + requestData.Extension);
        }
    });
}

function CheckExcelFileTG() {
    CheckFileExists(InsertDataTiendasGanadoras)
}