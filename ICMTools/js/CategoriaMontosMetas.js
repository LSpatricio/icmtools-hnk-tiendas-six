function InsertDataCategoriaMontosMetas() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/categoriamontosmetas/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            courseFlag = false
            if (response.d === true) {
                setLoadingBar("¡Completado!", 100);
                setFormStatus("success");
                $("#formatSuccess").html(response.r);
                if (typeof activateSuccess === "function") { activateSuccess(); }
                if (response.f) {
                    const filename = response.f.split('\\').pop().split('/').pop();
                    const downloadUrl = "/api/files/download?filename=" + encodeURIComponent(filename);
                    window.location.href = downloadUrl;
                };
            }
            else {
                setFormStatus("error");
                $("#formatErrors").html(response.r);
                if (typeof activateTable === "function") { activateTable(); }
                $('#btnStartImport').prop('disabled', true).html('<i class="fas fa-play fa-fw"></i> Reintentar');
            }
        },
        error: function (xhr) {
            courseFlag = false
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (insert).");
            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        }
    });
}

function CheckExcelFileTG() {
    CheckFileExists(InsertDataCategoriaMontosMetas);
}