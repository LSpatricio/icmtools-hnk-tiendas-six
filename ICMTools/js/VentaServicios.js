/// <reference path="shared.js" />
function InsertDataVentaServicios() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: 'Servicios\\VentaServicios',
        Extension: '.xlsx'
    }
    $.ajax({
        type: "POST",
        url: "/api/ventaservicios/insertdata",
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
            }
        },
        error: function (xhr) {
            courseFlag = false
            setFormStatus("error");
            $("#formatErrors").html("Error de comunicación (insert).");            
        }
    });
}

function CheckExcelFileTG() {
    CheckFileExists(InsertDataVentaServicios)
}