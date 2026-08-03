function InsertDataMetaCategoria() {
    var fileName = '';
    var fileCCNomina = '';

    try {
        fileName = $("#ctl00_ContentPlaceHolder1_AsyncFileUpload1_ctl02")[0].last_uploaded_file_name
        var fileNameWithoutExt = fileName.split('.').slice(0, -1).join('.');
        if (fileNameWithoutExt.includes('_')) {
            fileCCNomina = fileNameWithoutExt.substring(fileNameWithoutExt.lastIndexOf('_') + 1);
        }
    } catch (e) {
    }

    if (fileCCNomina === '') {
        setFormStatus("error");
        $("#formatErrors").html("El nombre del archivo no es válido (" + fileName + "). Debe finalizar con el CCNomina, precedido por un guion bajo. Ejemplo: <b>MetasCategoriaPesos_118A.xlsx</b>");
        $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');

        return;
    }

    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: 'Categoria\\MetaCategoria',
        LogBody: fileCCNomina,
        Extension: '.xlsx'
    }
    $.ajax({
        type: "POST",
        url: "/api/metacategoria/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
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
                $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (insert).");
            $('#btnStartImport').prop('disabled', false).html('<i class="fas fa-play fa-fw"></i> Reintentar');
        }
    });
}

function CheckExcelFileTG() {
    CheckFileExists(InsertDataMetaCategoria)
}