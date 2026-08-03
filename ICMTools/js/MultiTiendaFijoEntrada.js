/// <reference path="shared.js" />
function InsertDataMultiTiendaFijoEntrada() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension
    }
    $.ajax({
        type: "POST",
        url: "/api/multitiendafijoentrada/insertdata",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}

function CheckExcelFileMTFE() {
    CheckFileExists(InsertDataMultiTiendaFijoEntrada)
}