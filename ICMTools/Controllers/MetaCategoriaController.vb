Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Imports System.Web.Http
Imports System.Threading

Public Class MetaCategoriaController
    Inherits ApiController

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/metacategoria/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim tBody As String = Nothing

            For row As Integer = 2 To usedRows
                Dim PlazaCR As String = ExcelArray(row, 1).ToString()
                Dim StoreCR As String = ExcelArray(row, 2).ToString()
                Dim Goal As Decimal = Convert.ToDecimal(ExcelArray(row, 3).ToString())

                jTable.Add(New With {
                        .PlazaCR = PlazaCR,
                        .StoreCR = StoreCR,
                        .Goal = Goal
                    })
            Next

            If jTable.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

            Dim ws As New WebServiceICMGeneral()
            Dim success As Boolean = False

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcodev"
            End If

            Dim columnascatCCNominaFEMCOVSDEV As New List(Of String) From {"CCNomina"}
            Dim catCCNominaFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatCCNominaFEMCOVSDEV, "catCCNomina", Model)
            Dim jsonTableCatCCNomina As String = JsonConvert.SerializeObject(catCCNominaFEMCOVSDEV)

            Dim columnascatPlazas As New List(Of String) From {"ID", "plazaId"}
            Dim catPlazasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model)
            Dim jsonTableCatPlazas As String = JsonConvert.SerializeObject(catPlazasFEMCOVSDEV)

            Dim columnascatTiendas As New List(Of String) From {"tiendaId"}
            Dim catTiendasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model)
            Dim jsonTableCatTiendas As String = JsonConvert.SerializeObject(catTiendasFEMCOVSDEV)

            Dim xlsx As New DataTable()
            Dim rTable As String = Nothing
            Dim current_ccn As String = request.LogBody

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT public.femcovs_validacion_archivo_metacategoriapesos(@archivo_info_json, @catccnominajson, @catplazasjson, @cattiendasjson, @current_ccn)", conn)
                    cmd.Parameters.AddWithValue("archivo_info_json", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("catccnominajson", NpgsqlDbType.Json, jsonTableCatCCNomina)
                    cmd.Parameters.AddWithValue("catplazasjson", NpgsqlDbType.Json, jsonTableCatPlazas)
                    cmd.Parameters.AddWithValue("cattiendasjson", NpgsqlDbType.Json, jsonTableCatTiendas)
                    cmd.Parameters.AddWithValue("current_ccn", NpgsqlDbType.Varchar, current_ccn)
                    success = cmd.ExecuteScalar()
                End Using

                Dim query As String = $"SELECT TIPO_DATO, VALOR, DETALLE FROM MetaCategoriaStatus"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        adapter.Fill(xlsx)
                    End Using
                End Using
            End Using

            Dim filePath As String = Nothing
            If (xlsx.Rows.Count) > 0 Then filePath = fc.BuildXlsx(xlsx, "MetaCategoria")

            If success = True Then
                tBody = "<tr><td>Ejecucion Completada Exitosamente</td><td>Se Ejecuto correctamente el proceso externo<ul><strong>Carga de Meta Categoria Peso favor de revisar el archivo anexo de errores para revisar </strong></td></tr>"
                rTable = sc.TableBuilder(tBody, 3)

                Return Ok(New With {
                          .d = True,
                          .r = rTable,
                          .f = filePath
                          })
            Else
                tBody = $"<tr><td>Error al ejecutar el proceso de validacion de archivo de Meta Categoria Peso</td>
                         <td>
                            Se han presentado inconvenientes al ejecutar el Proceso que carga en la intermedia el archivo de Meta Categoria Peso.
				            <br><br>Por favor verifique la integridad del archivo
				            <ul>
					            <li>El archivo debe contener solo 3 columnas :</li>
				                <p>Ejemplo de archivo:</p>
                                <li>Las columnas deben estar en el orden mencionado</li>
				                <table style=""width:400px;"">
				                    <tr>
					                    <th>CR Plaza</th>
					                    <th>Cr Tienda</th>
					                    <th>METAS</th>
					                </tr>
				                    <tr>
				                        <td>10VCZ</td>
				                        <td>5000T</td>
				                        <td>60356.59</td>
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