Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class ReporteArqueosController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _arqueosExcelReader As ArqueosExcelReader

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _arqueosExcelReader = New ArqueosExcelReader()
    End Sub

    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/reportearqueos/validarinfo")>
    Public Function ValidarInfo(<FromBody> request As ValidateFileRequestt) As IHttpActionResult
        Try
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing
            Dim tipo As Type = GetType(ArqueosExcelDto)
            Dim hojasDefinidas As List(Of Type) = _excelService.ObtenerTipos(tipo)
            Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

            For Each hoja In hojasDefinidas
                Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(hoja)
                Dim atributo = tipo.GetProperties().ToList().FirstOrDefault(Function(p) p.PropertyType.GetGenericArguments()(0) = hoja).GetCustomAttributes(GetType(ExcelSheetAttribute), False).Cast(Of ExcelSheetAttribute)().First()

                valoresErrores.AddRange(_arqueosExcelReader.ValidacionesArqueos(request.Path, atributo.HeaderRow, atributo.SheetName, mapeoColumnas))
            Next

            If valoresErrores.Count > 0 Then
                For Each errores In valoresErrores
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
            End If

            Dim rTable As String = sc.GetMessage("Arqueos", "CargaCompleta")
            Return Ok(New With {.d = True, .path = request.Path, .f = request.Path, .r = rTable})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/reportearqueos/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(500)

            Dim sqlConnSetting = ConfigurationManager.ConnectionStrings("SQLSERVER_CONNECTION")
            Dim connStr As String = If(sqlConnSetting IsNot Nothing, sqlConnSetting.ConnectionString, Nothing)

            If String.IsNullOrWhiteSpace(connStr) Then
                Return InternalServerError(New InvalidOperationException("No se encontró la cadena de conexión SQLSERVER_CONNECTION en Web.config."))
            End If

            Dim rutaArchivo As String = ObtenerRutaArchivoCarga(request)
            If String.IsNullOrWhiteSpace(rutaArchivo) OrElse Not File.Exists(rutaArchivo) Then
                Return Ok(New With {.d = False, .r = "No se encontró el archivo cargado en el servidor."})
            End If

            Dim tablaStg As DataTable = _arqueosExcelReader.ObtenerDataTableStgArqueos(rutaArchivo)

            Using conn As New SqlConnection(connStr)
                conn.Open()

                Using tran As SqlTransaction = conn.BeginTransaction()
                    Try
                        Using deleteCmd As New SqlCommand("DELETE FROM dbo.STG_ARQUEOS;", conn, tran)
                            deleteCmd.ExecuteNonQuery()
                        End Using

                        Using bulkCopy As New SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran)
                            bulkCopy.DestinationTableName = "dbo.STG_ARQUEOS"
                            bulkCopy.BatchSize = 1000
                            bulkCopy.BulkCopyTimeout = 0

                            For Each col As DataColumn In tablaStg.Columns
                                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName)
                            Next

                            bulkCopy.WriteToServer(tablaStg)
                        End Using

                        tran.Commit()
                    Catch
                        tran.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            Using db As New DataBase(connStr)
                db.ExecuteStoredProcedure("dbo.SP_VALIDATE_ARQUEOS", DataBase.EnumExecutionType.NonQuery)
            End Using

            Dim rTable As String = sc.GetMessage("Arqueos", "CargaCompleta")
            Return Ok(New With {.d = True, .r = rTable, .rows = tablaStg.Rows.Count})
        Catch ex As Exception
            Return Ok(New With {
                .d = False,
                .r = ex.Message
            })
        End Try
    End Function

    Private Function ObtenerRutaArchivoCarga(request As ValidateFileRequest) As String
        Dim rawFileType As String = "Arqueos"
        Dim rawExtension As String = ".xlsx"

        If request IsNot Nothing Then
            If Not String.IsNullOrWhiteSpace(request.FileType) Then
                rawFileType = request.FileType
            End If

            If Not String.IsNullOrWhiteSpace(request.Extension) Then
                rawExtension = request.Extension
            End If
        End If

        Dim userEmail As String = CType(HttpContext.Current.Session.Item("User"), User).Email

        Dim baseDir As String = Path.GetFullPath(HttpContext.Current.Server.MapPath("~/UploadedFiles/"))
        If Not baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()) Then
            baseDir &= Path.DirectorySeparatorChar
        End If

        Dim fileName As String = Path.GetFileName(userEmail & rawExtension)
        Dim fullPath As String = Path.GetFullPath(Path.Combine(baseDir, rawFileType, fileName))

        If Not fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase) Then
            Return Nothing
        End If

        Return fullPath
    End Function
End Class
