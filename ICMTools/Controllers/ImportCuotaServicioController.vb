Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json

Public Class ImportCuotaServicioController
    Inherits ApiController

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/cuotaservicio/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim tBody As String = Nothing
            Dim filePath = Nothing

            For row As Integer = 2 To usedRows
                Dim Plaza As String = ExcelArray(row, 1).ToString()
                Dim Tienda As String = ExcelArray(row, 2).ToString()
                Dim Value As Decimal = Convert.ToDecimal(ExcelArray(row, 3).ToString())
                Dim DateStar As String = ExcelArray(row, 4).ToString()
                Dim DateEnd As String = ExcelArray(row, 5).ToString()

                jTable.Add(New With {
                        .Plaza = Plaza,
                        .Tienda = Tienda,
                        .Value = Value,
                        .DateStar = DateStar,
                        .DateEnd = DateEnd
                    })
            Next

            If jTable.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

            Dim ws As New WebServiceICMGeneral()
            Dim success As Boolean = False
            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If

            Dim columnascatPlazas As New List(Of String) From {"plazaId", "Description"}
            Dim catPlazasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model)
            Dim jsonTableCatPlazas As String = JsonConvert.SerializeObject(catPlazasFEMCOVSDEV)

            Dim columnascatDistritosFEMCOVSDEV As New List(Of String) From {"plazaId", "Description", "distritoId"}
            Dim catDistritosFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritosFEMCOVSDEV, "catDistritos", Model)
            Dim jsonTablecatDistritos As String = JsonConvert.SerializeObject(catDistritosFEMCOVSDEV)

            Dim columnascatTiendas As New List(Of String) From {"tiendaId", "ID"}
            Dim catTiendasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model)
            Dim jsonTableCatTiendas As String = JsonConvert.SerializeObject(catTiendasFEMCOVSDEV)

            Dim columnasCfgServiceFee As New List(Of String) From {"IDPlaza", "IDDistrict", "IDStore", "DateStar", "IDStatus", "Value", "DateEnd"}
            Dim CfgServiceFeeFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnasCfgServiceFee, "CfgServiceFee", Model)
            Dim jsonTableCfgServiceFee As String = JsonConvert.SerializeObject(CfgServiceFeeFEMCOVSDEV)

            Dim xlsx As New DataTable()
            Dim rTable As String = Nothing
            Dim current_ccn As String = request.LogBody

            success = True

            If success = True Then
                tBody = "<tr><td>Ejecucion Completada Exitosamente</td><td>Se Ejecuto correctamente el proceso externo<ul><strong>Carga de Cuota Servicios favor de revisar el archivo anexo de errores para revisar </strong></td></tr>"

                rTable = sc.TableBuilder(tBody, 3)

                Return Ok(New With {
                          .d = True,
                          .r = rTable,
                          .f = filePath
                          })
            Else
                tBody = $"<tr><td>Error al ejecutar el proceso de validacion de archivo de Cuota Servicios</td>
                         <td>
                            Se han presentado inconvenientes al ejecutar el Proceso que carga en la intermedia el archivo de Cuota Servicios Peso.
				            <br><br>Por favor verifique la integridad del archivo
				            <ul>
					            <li>El archivo debe contener solo 3 columnas :</li>
				                <p>Ejemplo de archivo:</p>
                                <li>Las columnas deben estar en el orden mencionado</li>
				                <table style=""width:400px;"">
				                    <tr>
					                    <th>IDPlaza </th>
					                    <th>IDTienda</th>
					                    <th>Value</th>
					                    <th>DateStar</th>
					                    <th>DateEnd</th>
					                </tr>
				                    <tr>
				                        <td>Acapulco (Antiguo)</td>
				                        <td>GENERICO</td>
				                        <td>0.1</td>
				                        <td>01/01/2018</td>
				                        <td>3/31/2021</td>
				                        </tr>
				                </table>
                        </td>
                        </tr>"

                rTable = sc.TableBuilder(tBody, 1)

                Return Ok(New With {
                          .d = False,
                          .r = rTable,
                          .f = filePath
                          })
            End If
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function
End Class