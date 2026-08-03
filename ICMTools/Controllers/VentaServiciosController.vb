Imports System.Threading
Imports System.Web
Imports System.Web.Http
Imports ClassLibrary_PGP_TO_SFTP
Imports ClosedXML.Excel.XLPredefinedFormat
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Public Class VentaServiciosController
    Inherits ApiController

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    ReadOnly fc As New FileController
    Private ReadOnly modelo As String = ConfigurationManager.AppSettings("ModelFemcoVSDev")
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/ventaservicios/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)


            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim IdStore As String = Nothing
            Dim tBody As String = Nothing


            For row As Integer = 2 To usedRows
                Dim StoreIDCL As String = ExcelArray(row, 1).ToString()
                Dim PlazaCRCL As String = ExcelArray(row, 2).ToString()
                Dim PlazaCL As String = ExcelArray(row, 3).ToString()
                Dim StoreCRCL As String = ExcelArray(row, 4).ToString()
                Dim StoreCL As String = ExcelArray(row, 5).ToString()
                Dim CashierIDCL As String = ExcelArray(row, 6).ToString()
                Dim PayeeIDCL As String = ExcelArray(row, 7).ToString()
                Dim SubcategoryCL As String = ExcelArray(row, 8).ToString()
                Dim UnitsSoldCL As String = ExcelArray(row, 9).ToString()
                Dim CreationDateCL As String = ExcelArray(row, 10).ToString()

                If String.IsNullOrWhiteSpace(StoreIDCL) AndAlso String.IsNullOrWhiteSpace(PlazaCL) Then Continue For

                jTable.Add(New With {
                        .StoreID = StoreIDCL,
                        .PlazaCR = PlazaCRCL,
                        .Plaza = PlazaCL,
                        .StoreCR = StoreCRCL,
                        .Store = StoreCL,
                        .CashierID = CashierIDCL,
                        .PayeeID = PayeeIDCL,
                        .Subcategory = SubcategoryCL,
                        .UnitsSold = UnitsSoldCL,
                        .CreationDate = CreationDateCL
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
            Dim columnascatPlazas As New List(Of String) From {"ID", "plazaId", "Description"}
            Dim catPlazasFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model)

            Dim columnascatDistritos As New List(Of String) From {"ID", "plazaId", "Description", "distritoId"}
            Dim catDistritosFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritos, "catDistritos", Model)

            Dim columnasCfgStoreSocietys As New List(Of String) From {"IDSociety", "IDStore"}
            Dim CfgStoreSocietyFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnasCfgStoreSocietys, "CfgStoreSociety", Model)

            Dim columnascatTiendas As New List(Of String) From {"tiendaId"}
            Dim catTiendasFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model)

            Dim columnasPayee_ As New List(Of String) From {"PayeeID_", "SocietyId", "Termination_Date_"}
            Dim PayeeFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnasPayee_, "Payee_", Model)

            Dim columnasTime_ As New List(Of String) From {"Name_", "TimeID_"}
            Dim TimeFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnasTime_, "Time_", Model)

            Dim catPlazasJson As String = JsonConvert.SerializeObject(catPlazasFEMCOVS)
            Dim catDistritosJson As String = JsonConvert.SerializeObject(catDistritosFEMCOVS)
            Dim CfgStoreSocietyJson As String = JsonConvert.SerializeObject(CfgStoreSocietyFEMCOVS)
            Dim catTiendasJson As String = JsonConvert.SerializeObject(catTiendasFEMCOVS)
            Dim PayeeJson As String = JsonConvert.SerializeObject(PayeeFEMCOVS)
            Dim TimeJson As String = JsonConvert.SerializeObject(TimeFEMCOVS)


            Dim xlsx As New DataTable()
            Dim rTable As String = Nothing

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.spFemcoVsImportVentaServicios(@jtable, @catplazastable ,@catdistritostable, @cfgstoresocietytable, @cattiendastable, @payeetable, @timetable)", conn)
                    cmd.Parameters.AddWithValue("jtable", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("catplazastable", NpgsqlDbType.Json, catPlazasJson)
                    cmd.Parameters.AddWithValue("catdistritostable", NpgsqlDbType.Json, catDistritosJson)
                    cmd.Parameters.AddWithValue("cfgstoresocietytable", NpgsqlDbType.Json, CfgStoreSocietyJson)
                    cmd.Parameters.AddWithValue("cattiendastable", NpgsqlDbType.Json, catTiendasJson)
                    cmd.Parameters.AddWithValue("payeetable", NpgsqlDbType.Json, PayeeJson)
                    cmd.Parameters.AddWithValue("timetable", NpgsqlDbType.Json, TimeJson)

                    success = cmd.ExecuteScalar()
                End Using
                Dim query As String = $"SELECT TIPO_DATO,VALOR,DETALLE FROM TMP_DETAILSTATUS"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        adapter.Fill(xlsx)
                    End Using
                End Using
            End Using

            Dim filePath As String = fc.BuildXlsx(xlsx, "VentaServicios")
            If success = True Then
                tBody = "<tr><td>Ejecucion Completada Exitosamente</td><td>Se Ejecuto correctamente el proceso <ul><strong>Import Empleados Activos</strong></td></tr>"

                rTable = sc.TableBuilder(tBody, 3)

                Return Ok(New With {
                          .d = True,
                          .r = rTable,
                          .f = filePath
                          })
            Else
                If mUser IsNot Nothing AndAlso Not String.IsNullOrEmpty(mUser.Model) Then
                    ClassLibrary_PGP_TO_SFTP.Main_PGPtoSFTP.Proceso("VentasServicios", xlsx, mUser.Model)
                End If

                tBody = $"<tr><td>Error al ejecutar el proceso de validacion de archivo de tiendas Ganadoras</td>
                         <td>
                            Se han presentado inconvenientes al ejecutar el Proceso que carga en la intermedia el archivo de Tiendas ganadoras.
                <br><br>Por favor verifique la integridad del archivo
                <ul>
                 <li>El archivo debe contener solo 3 columnas :</li>
                    <p>Ejemplo de archivo:</p>
                                <li>Las columnas deben estar en el orden mencionado</li>
                    <table style=""width:400px;"">
                        <tr>
                         <th>Plaza</th>
                         <th>Tienda</th>
                         <th>IDConcurso</th>
                     </tr>
                        <tr>    
                            <td>10CAN DESCRIPCION</td>
                            <td>50GLC</td>
                            <td>4</td>
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
