
function CheckFileExistsC() {
    CheckFileExists(InsertClasificaciones)
}

function InsertClasificaciones() {
    setLoadingBar("Insertando datos...", 75);
    const requestData = {
        FileType: pageConfig.FileType,
        Extension: pageConfig.Extension,
        Society: $("#SelectSociety").val(),
        PersonnelDivision: $("#SelectPersonnelDivision").val()
    }

    var token = $('input[name="__RequestVerificationToken"]').val();
    $.ajax({
        type: "POST",
        url: apiUrl, 
        contentType: 'application/json; charset=utf-8',
        headers: {
            'X-XSRF-Token': token
        },
        data: JSON.stringify(requestData),
        success: InsertData_OnSuccess,
        error: InsertData_OnError,
        complete: function () {
            InsertData_OnComplete(requestData);
        }
    });
}