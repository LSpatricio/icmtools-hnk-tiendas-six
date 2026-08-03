function Step1_CheckFile() {
    CheckFileExists(Step2_LoadCatalogs)
}

/**
Carga los catálogos 
*/
function Step2_LoadCatalogs() {
    const requestData = {
        FileType: pageConfig.FileType,
        LogPage: pageConfig.LogPage,
        Extension: pageConfig.Extension
    }

    setLoadingBar("Cargando catalogos...", 50);
    $.ajax({
        type: "POST",
        url: "/api/importmetas/loadcatalogs",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.d === true) {
                Step3_InsertData();
            } else {
                setFormStatus("error");
                $("#formatErrors").html(response.d);
                if (typeof activateTable === "function") { activateTable(); }
                DeleteAll();
            }
        },
        error: function (xhr) {
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (loadcatalogs).");
            DeleteAll();
        }
    });
}

function Step3_InsertData() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/importmetas/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}