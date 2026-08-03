Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Web.Http
Imports ClassLibrary_PGP_TO_SFTP
Imports CsvHelper
Imports DocumentFormat.OpenXml.Wordprocessing
Imports ICMTools.FileController
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class VTables
    Public Property TableName As String
    Public Property Cols As List(Of String)
    Public Property NTable As String
    Public Sub New(ByVal nombre As String, ByVal listaCols As List(Of String), ByVal postgreTable As String)
        Me.TableName = nombre
        Me.Cols = listaCols
        Me.NTable = postgreTable
    End Sub
End Class

Public Module vsVentasTablesConfig
    Public ReadOnly vsVentasCatalogos As New List(Of VTables) From {
        New VTables("CfgDates", New List(Of String) From {"IDDate", "Value"}, "cfgdates_vsventas"),
        New VTables("catTiendas", New List(Of String) From {"tiendaId", "plazaId"}, "cattiendas_vsventas"),
        New VTables("catPlazas", New List(Of String) From {"plazaId"}, "catplazas_vsventas"),
        New VTables("CfgStoreHierarchy", New List(Of String) From {"IDPlaza"}, "cfgstorehierarchy_vsventas"),
        New VTables("CatPromotions", New List(Of String) From {"IDPromotion"}, "catpromotions_vsventas"),
        New VTables("CfgStoreSociety", New List(Of String) From {"IDStore", "IDSociety"}, "cfgstoresociety_vsventas")
    }
End Module

Public Class ImportVentasController
    Inherits ApiController

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

#Region " Variables Privadas "

    ''' <summary>
    ''' Headers DataTable
    ''' </summary>
    Private mHeaderDataTable As DataTable

#End Region

    Public Class CheckFileRequestIV
        Public Property FileType As String
        Public Property Extension As String
        Public Property Chunks As List(Of String)
    End Class
    Public Class InsertDataClass
        Public Property FileType As String
        Public Property Extension As String
    End Class
    Public Class SalesRecord
        Public Property IDSTORE As String
        Public Property PLAZA_CVE As String
        Public Property PLAZA_DES As String
        Public Property TIENDA_CVE As String
        Public Property TIENDA_DES As String
        Public Property ID_USUARIO As String
        Public Property ID_EMPLEADO As String
        Public Property VT_FOL_PRO As String
        Public Property PROMOCION_DESC As String
        Public Property TIPO_PROMOCION As String
        Public Property UDSNETAS_REALES As String
        Public Property CREATIONDATE As String
    End Class

    <HttpPost>
    <Route("api/importventas/insertdata")>
    Public Function InsertData(<FromBody> request As InsertDataClass) As IHttpActionResult
        Try
            mLog = New Log
            mLog.insertLog("Proceso Masivo", "Inicio de Proceso", "Se inició el proceso de importacion de ventas")
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim tBody As String = Nothing
            Dim PayeeCols As New List(Of String) From {"PayeeID_", "Termination_Date_"}
            Dim pgp As New DataTable()
            Dim rTable As String = Nothing
            Dim pgpRowsCount As Long = 0

            Dim ws As New WebServiceICMGeneral()
            Dim success As Boolean = False
            Dim ParcialC As Boolean = False

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If

            success = EjecutarFuncionPG()

            If success Then
                Return Ok(New With {.d = True})
            Else
                rTable = sc.GetMessage("Importación Ventas", "Error",
                    New List(Of String) From {"IDSTORE", "PLAZA_CVE", "PLAZA_DES", "TIENDA_CVE", "TIENDA_DES", "ID_USUARIO", "ID_EMPLEADO", "VT_FOL_PRO", "PROMOCION_DESC", "TIPO_PROMOCION", "UDSNETAS_REALES", "CREATIONDATE"},
                    New List(Of String) From {"10DCU50P9Y", "10DCU", "10DCU Colima", "50P9Y", "Camelinas CUL", "SACARO9307030", "5624447", "51169951", "238810_1 2 x VSG - Gansito", "GEN", "4", "22/10/2025"})
                Return Ok(New With {.d = False, .r = rTable})
            End If

        Catch ex As Exception
            mLog.insertLog("Importación de Ventas", "InsertData", "Error: " + ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/importventas/loadcatalogs")>
    Public Function LoadCatalogs() As IHttpActionResult
        Try
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
            Dim PayeeCols As New List(Of String) From {"PayeeID_", "Termination_Date_"}

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand($"CALL public.""femcovs_dropcats_vsVentas""()", conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            Using ws As New WebServiceICMGeneral()
                Dim Model As String = If(mUser.Model.Equals("DEBUG"), "femcovsdev", mUser.Model)

                For Each catalogo In vsVentasTablesConfig.vsVentasCatalogos
                    ws.InsertaCatalogos(catalogo.Cols, catalogo.TableName, Model, catalogo.NTable)
                Next

                ws.InsertaICMAPIQueryLotes(PayeeCols, "Payee_", Model, "payee_ventas")
            End Using

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog = New Log
            mLog.insertLog("Importación de Ventas", "LoadCatalogs", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/importventas/loadfile")>
    Public Function LoadFile() As IHttpActionResult
        mLog = New Log
        Try
            Dim chunkErrors As String = Nothing
            Dim contador As Integer = 0
            Const batchSize As Integer = 5000

            Dim chunkFiles = CType(HttpContext.Current.Session("VentasChunkFiles"), List(Of String))

                For Each chunkPath As String In chunkFiles
                    Dim salesBatch As New List(Of SalesRecord)()

                    Try
                        Using reader As New StreamReader(chunkPath)
                            Using csv As New CsvReader(reader, CultureInfo.InvariantCulture)

                                While csv.Read()
                                    Dim record = csv.GetRecord(Of SalesRecord)()
                                    salesBatch.Add(record)

                                    If salesBatch.Count >= batchSize Then
                                    BulkInsertSalesRecords(salesBatch)
                                    salesBatch.Clear()
                                    End If
                                End While
                            End Using
                        End Using

                        If salesBatch.Count > 0 Then
                        BulkInsertSalesRecords(salesBatch)
                        salesBatch.Clear()
                        End If

                        mLog.insertLog("Proceso Masivo", "LOG DE PROCESO", "Insercion de excel finalizada")

                    Catch ex As Exception
                        chunkErrors += "<tr><td>Error procesando el chunk #" & contador & "</td><td>" & ex.Message & "</td></tr>"
                    End Try

                    contador += 1
                Next

                If chunkErrors IsNot Nothing Then
                    Dim rTableErr = sc.TableBuilder(chunkErrors, 1)
                    Return Ok(New With {.d = False, .r = rTableErr})
                End If

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog.insertLog("Importación de Ventas", "LoadFile", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/importventas/dresponse")>
    Public Function DownloadResponse() As IHttpActionResult
        Dim filePath As String = Nothing
        mLog = New Log
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Dim errorQuery As String = "SELECT * FROM suggestedsalesales WHERE idstatus = false"
                Using errorCmd As New NpgsqlCommand(errorQuery, conn)
                    Using reader As NpgsqlDataReader = errorCmd.ExecuteReader()
                        If reader.HasRows Then
                            mHeaderDataTable = New DataTable()
                            For i As Integer = 0 To reader.FieldCount - 1
                                mHeaderDataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i))
                            Next
                            filePath = BuildCSV()
                            Using writer As New StreamWriter(filePath, True, Encoding.UTF8)
                                Using csvWriter As New CsvWriter(writer, CultureInfo.InvariantCulture)
                                    While reader.Read()
                                        For i As Integer = 0 To reader.FieldCount - 1
                                            csvWriter.WriteField(reader(i))
                                        Next
                                        csvWriter.NextRecord()
                                    End While
                                End Using
                            End Using
                        End If
                    End Using
                End Using
            End Using

            Return Ok(New With {
                       .f = filePath
                       })
        Catch ex As Exception
            mLog.insertLog("ImportVentasController", "DownloadResponse", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Crea un archivo .csv a partir de un DataTable y lo guarda en el servidor.
    ''' </summary>
    ''' <returns>La ruta absoluta del archivo .xlsx generado en el servidor.</returns>
    Private Function BuildCSV() As String
        Dim timestamp As String = DateTime.Now.ToString("ddMMyyyy_HH_mm_ss")
        Dim fileName As String = $"Resultados_ImportacionVentasErrores_{timestamp}.csv"

        Dim fileClass As New FileClass
        Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

        Dim filePath As String = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + safeFileName)
        Dim sb As New StringBuilder()

        Dim columnNamesList As New List(Of String)()
        For Each col As DataColumn In mHeaderDataTable.Columns
            Dim columnName As String = col.ColumnName
            Dim sanitizedName As String = String.Empty

            If columnName.Contains(",") OrElse columnName.Contains("""") Then
                sanitizedName = """" & columnName.Replace("""", """""") & """"
            Else
                sanitizedName = columnName
            End If

            columnNamesList.Add(sanitizedName)
        Next

        sb.AppendLine(String.Join(",", columnNamesList))

        For Each row As DataRow In mHeaderDataTable.Rows
            Dim fieldsList As New List(Of String)()

            For Each item As Object In row.ItemArray
                Dim fieldValue As String = If(item IsNot Nothing, item.ToString(), String.Empty)
                Dim sanitizedName As String = String.Empty

                If fieldValue.Contains(",") OrElse fieldValue.Contains("""") Then
                    sanitizedName = """" & fieldValue.Replace("""", """""") & """"
                Else
                    sanitizedName = fieldValue
                End If

                fieldsList.Add(sanitizedName)
            Next

            sb.AppendLine(String.Join(",", fieldsList))
        Next

        File.WriteAllText(filePath, sb.ToString())

        Return filePath
    End Function

    <HttpPost>
    <Route("api/importventas/ending")>
    Public Function Ending(<FromBody> request As InsertDataClass) As IHttpActionResult
        Try
            mLog = New Log
            mLog.insertLog("Proceso Masivo", "Inicio de Proceso", "Se inició el proceso de importacion de ventas")
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim tBody As String = Nothing
            Dim PayeeCols As New List(Of String) From {"PayeeID_", "Termination_Date_"}
            Dim pgp As New DataTable()
            Dim rTable As String = Nothing
            Dim pgpRowsCount As Long = 0

            Dim ws As New WebServiceICMGeneral()
            Dim success As Boolean = True
            Dim ParcialC As Boolean = False

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                Dim successQuery As String = "SELECT * FROM suggestedsalesales WHERE idstatus = true"
                Using successCmd As New NpgsqlCommand(successQuery, conn)
                    Using adapter As New NpgsqlDataAdapter(successCmd)
                        adapter.Fill(pgp)
                    End Using
                End Using

                Dim failedQuery As String = "SELECT EXISTS (SELECT 1 FROM suggestedsalesales WHERE idstatus = false)"
                Using failedCmd As New NpgsqlCommand(failedQuery, conn)
                    Dim result As Object = failedCmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                        ParcialC = Convert.ToBoolean(result)
                    End If
                End Using
            End Using

            mLog.insertLog("Proceso Masivo", "LOG DE PROCESO", "Finalizacion de proceso de retorno")

            pgpRowsCount = pgp.Rows.Count
            If pgpRowsCount > 0 AndAlso mUser IsNot Nothing AndAlso Not String.IsNullOrEmpty(Model) Then
                SendSFTP()
            End If

            If pgp IsNot Nothing Then
                pgp.Dispose()
                pgp = Nothing
            End If

            If success = True And ParcialC = False AndAlso pgpRowsCount > 0 Then
                rTable = sc.GetMessage("Importación Ventas", "CargaCompleta")
                Return Ok(New With {
                         .d = 1,
                         .r = rTable
                         })

            ElseIf success = True And ParcialC = True AndAlso pgpRowsCount > 0 Then
                rTable = sc.GetMessage("Importación Ventas", "CargaParcial")
                Return Ok(New With {
                       .d = 2,
                       .r = rTable
                       })
            ElseIf success = True And pgpRowsCount = 0 Then
                tBody = $"<tr>
                            <td>Error al ejecutar el proceso de importación del archivo de Ventas</td>
                            <td>No se encontró información válida para importar<br>Por favor verifique la información del archivo</td>
                        </tr>"
                rTable = sc.TableBuilder(tBody, 1)
                Return Ok(New With {
                        .d = 4,
                        .r = rTable})
            Else
                rTable = sc.GetMessage("Importación Ventas", "Error",
                New List(Of String) From {"IDSTORE", "PLAZA_CVE", "PLAZA_DES", "TIENDA_CVE", "TIENDA_DES", "ID_USUARIO", "ID_EMPLEADO", "VT_FOL_PRO", "PROMOCION_DESC", "TIPO_PROMOCION", "UDSNETAS_REALES", "CREATIONDATE"},
                New List(Of String) From {"10DCU50P9Y", "10DCU", "10DCU Colima", "50P9Y", "Camelinas CUL", "SACARO9307030", "5624447", "51169951", "238810_1 2 x VSG - Gansito", "GEN", "4", "22/10/2025"})

                Return Ok(New With {
                        .d = False,
                        .r = rTable
                        })
            End If

        Catch ex As Exception
            mLog.insertLog("Proceso Masivo", "LOG DE PROCESO", "Error: " + ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

#Region "Validaciones"
    <HttpPost>
    <Route("api/importventas/processchunks")>
    Public Function CheckFileExistsIV(<FromBody> request As CheckFileRequestIV) As IHttpActionResult
        Try
            If HttpContext.Current.Session.Item("User") Is Nothing Then Return Unauthorized()
            mUser = CType(HttpContext.Current.Session.Item("User"), User)

            Dim chunkFiles = CType(HttpContext.Current.Session("VentasChunkFiles"), List(Of String))
            For Each chunkPath As String In chunkFiles
                If Not File.Exists(chunkPath) Then
                    Return Ok(New With {.d = False, .m = $"No se pudo encontrar el archivo a procesar. Intente la carga nuevamente"})
                End If
            Next
            Return Ok(New With {.d = True})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/importventas/validate")>
    Public Function ValidateExcelFile(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            mLog = New Log

            Dim errorsList As String = Nothing
            Dim chunkFiles = CType(HttpContext.Current.Session("VentasChunkFiles"), List(Of String))
            For Each chunkPath As String In chunkFiles
                Dim ExcelArray(,) As Object = fc.GetChunkArray(chunkPath)
                If ExcelArray IsNot Nothing Then
                    Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
                    Dim usedColumns As Integer = ExcelArray.GetUpperBound(1)

                    If (usedColumns) <> request.columns.Length() Then
                        errorsList = "<tr><td>Cantidad incorrecta de columnas</td><td>El archivo contiene " & usedColumns.ToString & " columnas, por favor corrija a " & request.columns.Length() & " columnas: <b>" & String.Join(", ", request.columns) & "</b>, y vuelva a intentar la carga.</td></tr>"
                        Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
                    End If

                    If errorsList IsNot Nothing Then
                        Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
                    End If

                    Dim realColumns As String() = New String(0) {}
                    Dim StringColumns As String = Nothing

                    For col As Integer = 0 To usedColumns - 1
                        Array.Resize(realColumns, realColumns.Length + 1)
                        realColumns(realColumns.Length - 1) = ExcelArray(1, col + 1)
                        StringColumns += request.columns(col)
                        If col < usedColumns - 1 Then StringColumns += ", "
                    Next

                    realColumns = realColumns.Skip(1).ToArray()

                    For col As Integer = 0 To realColumns.Length - 1
                        If CStr(realColumns(col)).Trim().ToLower() <> request.columns(col).Trim.ToLower() Then
                            errorsList = "<tr><td>Nombres de columnas incorrectos</td><td>El archivo no contiene los nombres de columnas correctos, por favor corrija a los titulos esperados: <b>" & StringColumns & "</b>, y vuelva a intentar la carga.</td></tr>"
                            Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
                        End If
                    Next


                    'VALIDACION 3 - Reviso formato correcto de valores en filas
                    For row As Integer = 2 To usedRows
                        For col As Integer = 1 To request.columns.Length
                            Dim cellValue As Object = ExcelArray(row, col)
                            Dim expectedType As String = request.types(col - 1)
                            Dim columnName As String = request.columns(col - 1)

                            If cellValue Is Nothing OrElse String.IsNullOrWhiteSpace(CStr(cellValue)) Then Continue For
                            Select Case expectedType
                                Case "String"
                                    If CStr(cellValue).Length > 100 Then
                                        errorsList += "<tr><td>Formato incorrecto en Fila #" & (row + 1).ToString & "</td><td>El IDPlaza no debe superar los 100 caracteres.</td></tr>"
                                    End If
                                Case "Date", "Datetime"
                                    Dim tempDate As Date
                                    If Not Date.TryParseExact(CStr(cellValue), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, tempDate) Then
                                        errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> no es una fecha válida.</td></tr>"
                                    End If
                                Case "Integer"
                                    Dim tempInt As Integer
                                    If Not Integer.TryParse(CStr(cellValue), tempInt) Then
                                        errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> no es un número entero válido.</td></tr>"
                                    End If
                                Case "Decimal"
                                    Dim tempDecimal As Decimal
                                    If Not Decimal.TryParse(CStr(cellValue), tempDecimal) Then
                                        errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> no es un número decimal válido.</td></tr>"
                                    End If
                            End Select
                        Next
                    Next

                    If errorsList IsNot Nothing Then Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

                    'VALIDACION 4 - Celdas vacias
                    Dim ContainsInfo As Boolean = False
                    Dim allowSomeNulls As Boolean = request.nulleable_columns IsNot Nothing

                    For row As Integer = 2 To usedRows
                        Dim doesRowHaveAnyData As Boolean = False
                        Dim isRowPartiallyEmpty As Boolean = False
                        Dim emptyColumns As String = Nothing

                        For col As Integer = 1 To request.columns.Length
                            Dim cellValue As Object = ExcelArray(row, col)
                            Dim columnName As String = request.columns(col - 1)

                            Dim is_nulleable As Boolean = If(allowSomeNulls, request.nulleable_columns(col - 1) = "NULL", False)

                            If (cellValue Is Nothing OrElse String.IsNullOrWhiteSpace(CStr(cellValue))) AndAlso Not is_nulleable Then
                                isRowPartiallyEmpty = True

                                emptyColumns += columnName & ", "
                            Else
                                doesRowHaveAnyData = True
                            End If
                        Next

                        ' Si la fila tiene datos pero al mismo tiempo tiene celdas vacías, es un error.
                        If doesRowHaveAnyData AndAlso isRowPartiallyEmpty Then
                            If allowSomeNulls Then
                                emptyColumns = If(emptyColumns IsNot Nothing, emptyColumns.Substring(0, emptyColumns.Length - 2), emptyColumns)

                                errorsList += "<tr><td>Datos incompletos en Fila #" & row & "</td><td>La fila contiene celda(s) vacía(s): " & emptyColumns & "</td></tr>"
                            Else
                                errorsList += "<tr><td>Datos incompletos en Fila #" & row & "</td><td>La fila debe tener todas sus celdas con información o estar completamente vacía.</td></tr>"
                            End If
                        End If

                        ' Actualizamos el indicador global si encontramos alguna fila con datos.
                        If doesRowHaveAnyData Then ContainsInfo = True
                    Next
                ElseIf chunkFiles.Count = 1 And ExcelArray.GetLowerBound(0) = 1 Then
                    errorsList = "<tr><td>Archivo vacio</td><td>El archivo esta vacio</td></tr>"
                    Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
                End If
            Next

            If errorsList IsNot Nothing Then
                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
            End If

            Return Ok(New With {.d = True})

        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

#End Region

#Region "Funciones"

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.Ventas
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

#End Region
#Region "Inserts"
    Private Sub BulkInsertSalesRecords(ByVal records As List(Of SalesRecord))
        Using conn As New NpgsqlConnection(NpgSQL)
            conn.Open()
            Using writer = conn.BeginBinaryImport("COPY ""VentaSugerida_Ventas"" (""IDSTORE"", ""PLAZA_CVE"", ""PLAZA_DES"", ""TIENDA_CVE"", ""TIENDA_DES"", ""ID_USUARIO"", ""ID_EMPLEADO"", ""VT_FOL_PRO"", ""PROMOCION_DESC"", ""TIPO_PROMOCION"", ""USDNETAS_REALES"", ""CREATIONDATE"") FROM STDIN (FORMAT BINARY)")
                For Each record In records
                    writer.StartRow()
                    writer.Write(record.IDSTORE, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.PLAZA_CVE, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.PLAZA_DES, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.TIENDA_CVE, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.TIENDA_DES, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.ID_USUARIO, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.ID_EMPLEADO, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.VT_FOL_PRO, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.PROMOCION_DESC, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.TIPO_PROMOCION, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.UDSNETAS_REALES, NpgsqlTypes.NpgsqlDbType.Varchar)
                    writer.Write(record.CREATIONDATE, NpgsqlTypes.NpgsqlDbType.Varchar)
                Next
                writer.Complete()
            End Using
        End Using
    End Sub
    Private Function EjecutarFuncionPG() As Boolean
        Dim success As Boolean
        Dim jump As Integer
        Dim steps As Integer = 3

        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                For jump = 1 To steps
                    Using cmd As New NpgsqlCommand("SELECT public.femcovs_validacion_archivo_ventasS" + Convert.ToString(jump) + "();", conn)
                        cmd.CommandTimeout = 300
                        success = cmd.ExecuteScalar()
                    End Using
                Next

                mLog.insertLog("Proceso Masivo", "LOG DE PROCESO", "Funcion finalizada")
            End Using
        Catch ex As Exception
            Return False
        End Try
        Return success
    End Function
#End Region
End Class
