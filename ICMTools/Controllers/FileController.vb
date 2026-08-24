Imports System.Collections.ObjectModel
Imports System.ComponentModel.Design
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Security
Imports System.Threading
Imports System.Web.Http
Imports ClosedXML.Excel
Imports DocumentFormat.OpenXml.Spreadsheet
Imports Microsoft.SqlServer.Server
Imports SixLabors.Fonts.Tables.General
Imports Serilog
Imports Serilog.Context

Public Class FileController
    Inherits ApiController
#Region "Variables Locales"

    Private mUser As User
    'Private mLog As Log
    Private sc As New SharedController
    'Private sanitize As New FileClass
    Private ReadOnly _excelService As New ExcelService
    Private ReadOnly _excelReader As New ExcelReader
    Private ReadOnly _fileService As New FileServices

    '   Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

#End Region

#Region "Clases"
    Public Sub New()
        _excelService = New ExcelService()
        _excelReader = New ExcelReader()
        _fileService = New FileServices()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)

    End Sub

    Public Class DeleteFunction
        Property FilePath As String
    End Class
#End Region

#Region "Metodos POST"

    '''<summary>
    '''Lee una ruta relativa y valida si el archivo existe en el servidor
    '''</summary>
    '''<returns>Una respuesta JSON con un parametro Booleano.</returns>
    '''<remarks>
    '''Esta funcion Solo valida la exstencia de un archivo, no su contenido.
    '''</remarks>
    <HttpPost>
    <Route("api/files/checkexists")>
    Public Function CheckFileExists(<FromBody> request As CheckFileRequest) As IHttpActionResult

        If HttpContext.Current.Session.Item("User") Is Nothing Then Return Unauthorized()

        Dim logger = Log _
                .ForContext("Pantalla", request.Screen) _
                .ForContext("Usuario", mUser.Email) _
                .ForContext("Periodo", request.Period) _
                .ForContext("Proceso", LoggerConfig.Proceso.CheckExists.ToString())
        Try

            Dim rawFileType As String = If(request?.FileType, String.Empty)
            Dim rawExtension As String = If(request?.Extension, String.Empty)

            Dim baseDir As String = Path.GetFullPath(HttpContext.Current.Server.MapPath("~/UploadedFiles/"))
            If Not baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()) Then baseDir &= Path.DirectorySeparatorChar

            Dim userEmail As String = CType(HttpContext.Current.Session.Item("User"), User).Email
            Dim fileName As String = Path.GetFileName(userEmail & rawExtension)
            Dim fullPath As String = Path.GetFullPath(Path.Combine(baseDir, rawFileType, fileName))

            If Not fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase) Then
                Return BadRequest("Intento de Path Traversal bloqueado.")
            End If

            If File.Exists(fullPath) Then

               logger.Information("Archivo encontrado. Ruta: {FileType}. Extension: {Extension}",
                        rawFileType,
                        rawExtension)

                Return Ok(New With {.d = True, .path = fullPath})
            Else
                logger.Warning("Archivo no encontrado. Ruta: {FileType}. Extension: {Extension}",
                        rawFileType,
                        rawExtension)

                Return Ok(New With {.d = False, .m = "No se pudo cargar el documento porque no existe en la carpeta del servidor, vuelva a intentar la carga."})
            End If
        Catch ex As Exception
            logger.Error(
            ex,
            "Error al verificar la existencia del archivo."
        )
            Return InternalServerError(ex)
        End Try
    End Function


    '''<summary>
    '''Lee un array bidimensional 1-based y valida sus columnas y filas para asegurar que cumple con el formato esperado.
    '''</summary>
    '''<returns>Un Response con un Booleano.</returns>
    <HttpPost>
    <Route("api/files/validate")>
    Public Function ValidateExcelFile(<FromBody> request As ValidateFileRequest) As IHttpActionResult

        Dim logger = Log _
                .ForContext("Pantalla", request.Screen) _
                .ForContext("Usuario", mUser.Email) _
                .ForContext("Periodo", request.Period) _
                .ForContext("Proceso", LoggerConfig.Proceso.ValidarArchivo.ToString())

        Try

            Thread.Sleep(1000)

            Dim errorsList As String = Nothing
            Dim erroresValidacion As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

            Dim validarEncabezados As Boolean = True

            If request.FileClass Is Nothing Then
                Return Ok(New With {
                    .d = "No se encontró la definición del archivo."
                })
            End If

            erroresValidacion = _fileService.ValidarExcel(request)

            If erroresValidacion.Count > 0 Then
                For Each errores In erroresValidacion
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                logger.Warning("Archivo no paso validaciones. Ruta: {Path}.",
                        request.Path)

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

            End If

            logger.Information("Archivo validado correctamente. Ruta: {Path}.",
                        request.Path)

            Return Ok(New With {.d = True})

        Catch ex As Exception

            logger.Error(
                        ex,
                        "Error al validar el archivo. Ruta: {Path}",
                        request.Path
                    )
            Return InternalServerError(ex)
        End Try
    End Function

#End Region

#Region "Metodos GET"
    ''' <summary>
    ''' Descarga un archivo almacenado en el servidor.
    ''' </summary>
    ''' <returns>La ruta donde se encuentra el xlsx.</returns>
    <HttpGet>
    <Route("api/files/download")>
    Public Function DownloadFile(ByVal filename As String) As HttpResponseMessage
        ' mLog = New Log
        Try
            Dim fileClass As New FileClass
            Dim safeFileName = fileClass.GetSafeFileName(filename)
            Dim filePath As String = HttpContext.Current.Server.MapPath("~/UploadedFiles/" & safeFileName)

            If Not File.Exists(filePath) Then
                Return Request.CreateResponse(HttpStatusCode.NotFound, "El archivo no fue encontrado en el servidor.")
            End If

            Dim fileBytes As Byte() = File.ReadAllBytes(filePath)
            Dim response As New HttpResponseMessage(HttpStatusCode.OK)
            response.Content = New ByteArrayContent(fileBytes)
            response.Content.Headers.ContentType = New Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            response.Content.Headers.ContentDisposition = New Http.Headers.ContentDispositionHeaderValue("attachment") With {
                  .FileName = safeFileName
            }

            Return response

        Catch ex As Exception
            'mLog.insertLog("FileController", "DownloadFile", $"Ocurrió un error: " + ex.Message)
            Return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex)
        End Try
    End Function
#End Region

#Region "Metodos DELETE"
    ''' <summary>
    ''' Elimina un archivo almacenado en el servidor.
    ''' </summary>
    ''' <returns>Bool.</returns>
    <HttpDelete>
    <Route("api/files/delete")>
    Public Function DeleteFileFunction(<FromBody> request As DeleteFunction) As IHttpActionResult
        'mLog = New Log
        Try
            Dim fileClass As New FileClass

            Dim filePath As String = fileClass.GetNormalizePath(request.FilePath)
            Dim fileName As String = Path.GetFileName(filePath)
            Dim safeName = fileClass.GetSafeFileName(fileName)

            Dim rutaDirectorio As String = HttpContext.Current.Server.MapPath("~\UploadedFiles\")
            Dim archivos() As String = Directory.GetFiles(rutaDirectorio, safeName, SearchOption.AllDirectories)

            For Each archivo In archivos
                File.Delete(archivo)
            Next
            Return Ok(New With {.d = True})

            Return Nothing
        Catch ex As Exception
            'mLog.insertLog("FileController", "DeleteFileFunction", $"Ocurrió un error: " + ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Elimina todos los archivos almacenados en una carpeta del servidor.
    ''' </summary>
    ''' <returns>Bool.</returns>
    <HttpDelete>
    <Route("api/files/deleteall")>
    Public Function DeleteAllFunction(<FromBody> request As DeleteFunction) As IHttpActionResult
        'mLog = New Log
        Try
            Dim fileClass As New FileClass
            Dim basePath As String = HttpContext.Current.Server.MapPath("~\UploadedFiles")
            Dim folder = fileClass.GetNormalizePath(request.FilePath)
            If (Not String.IsNullOrEmpty(folder)) Then
                Dim safeFolder As New StringBuilder("")
                Dim segments() As String = folder.Split("\"c)
                For Each segment As String In segments
                    Dim safeNameFolder As String = fileClass.GetSafeFileName(segment)
                    safeFolder.Append(String.Concat(segment, "\"))
                Next

                safeFolder.Remove(safeFolder.Length - 1, 1)
                Dim foundPath As String = Directory.GetDirectories(basePath, safeFolder.ToString(), SearchOption.TopDirectoryOnly).FirstOrDefault()
                If foundPath IsNot Nothing Then
                    Dim files As String() = Directory.GetFiles(foundPath)
                    For Each file In files
                        IO.File.Delete(file)
                    Next
                End If
            End If

            Return Ok(New With {.d = True})
        Catch ex As Exception
            'mLog.insertLog("FileController", "DeleteAllFunction", $"Ocurrió un error: " + ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

#End Region

#Region "Funciones"
    Public Function QuitarAcentos(textos As String()) As String()
        If textos Is Nothing Then Return Nothing

        Dim resultado(textos.Length - 1) As String

        For i As Integer = 0 To textos.Length - 1
            Dim texto As String = textos(i)
            If String.IsNullOrEmpty(texto) Then
                resultado(i) = texto
            Else
                texto = Regex.Replace(texto, "[^\u0020-\u007E\u00A0-\u00FF]", "", RegexOptions.None)

                Dim normalizado As String = texto.Normalize(NormalizationForm.FormD)
                Dim sb As New StringBuilder()

                For Each c As Char In normalizado
                    Dim categoria As UnicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c)
                    If categoria <> UnicodeCategory.NonSpacingMark Then
                        sb.Append(c)
                    End If
                Next

                resultado(i) = sb.ToString().Normalize(NormalizationForm.FormC)
            End If
        Next

        Return resultado
    End Function

    'Public Function GetExcelArray(Modulo As String, Extension As String, Optional AllowDuplicateEntries As Boolean = False) As Object
    '    Dim finalArray As Object = Nothing

    '    Dim safeModulo As String = Modulo.Replace("..", "").Replace("\\", "").Replace("//", "")
    '    Dim safeExtension As String = Path.GetExtension(Extension)

    '    Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
    '    Dim filePath As String = HttpContext.Current.Server.MapPath("~\UploadedFiles\" + safeModulo + "\" + mUser.Email + safeExtension)

    '    If safeExtension = ".csv" Then
    '        finalArray = GetCSVArray(filePath)

    '        Dim result As Object(,) = finalArray
    '        If (Not AllowDuplicateEntries) Then
    '            result = DeleteDoppelganger(finalArray)
    '        End If

    '        Return result
    '    End If

    '    Using workbook As New XLWorkbook(filePath)
    '        Dim worksheet = workbook.Worksheet(1)
    '        If worksheet Is Nothing OrElse worksheet.LastCellUsed() Is Nothing Then Return Nothing

    '        Dim range = worksheet.RangeUsed()
    '        Dim numCols As Integer = range.ColumnCount()

    '        If numCols < 3 Then numCols = 3
    '        Dim dataRows As New List(Of Object())

    '        For Each row In range.Rows()
    '            If row.IsEmpty() Then Continue For

    '            Dim rowArray(numCols - 1) As Object
    '            Dim isRowEmpty As Boolean = True

    '            For j As Integer = 0 To numCols - 1
    '                Dim cell = row.Cell(j + 1)
    '                Dim cellValue As Object = Nothing

    '                If Not cell.IsEmpty() Then
    '                    If cell.DataType = XLDataType.DateTime Then
    '                        cellValue = cell.GetDateTime().ToString("dd/MM/yyyy")
    '                    Else
    '                        cellValue = cell.GetFormattedString()
    '                    End If
    '                End If

    '                rowArray(j) = cellValue
    '                If j < 3 AndAlso cellValue IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cellValue.ToString()) Then isRowEmpty = False
    '            Next

    '            If Not isRowEmpty Then dataRows.Add(rowArray)

    '        Next

    '        If dataRows.Count = 0 Then Return Nothing

    '        Dim finalNumRows As Integer = dataRows.Count
    '        Dim finalNumCols As Integer = dataRows(0).Length
    '        finalArray = Array.CreateInstance(GetType(Object), {finalNumRows, finalNumCols}, {1, 1})

    '        For i As Integer = 0 To finalNumRows - 1
    '            For j As Integer = 0 To finalNumCols - 1
    '                finalArray(i + 1, j + 1) = dataRows(i)(j)
    '            Next
    '        Next

    '        Dim result As Object(,) = finalArray
    '        If (Not AllowDuplicateEntries) Then
    '            result = DeleteDoppelganger(finalArray)
    '        End If

    '        Return result
    '    End Using
    'End Function


    'Public Function GetCSVArray(filePath As String) As Object
    '    Try
    '        Dim safeFilePath As String = sanitize.GetSafeFilePath(filePath)
    '        If Not File.Exists(safeFilePath) Then
    '            Return Nothing
    '        End If

    '        If Not File.Exists(safeFilePath) Then
    '            Return Nothing
    '        End If

    '        Dim content As String
    '        content = File.ReadAllText(safeFilePath, System.Text.Encoding.UTF8)

    '        If content.Contains("") Then
    '            content = File.ReadAllText(safeFilePath, System.Text.Encoding.GetEncoding(1252)) ' 1252 es la página de códigos para Windows - 1252
    '        End If

    '        Dim lines As List(Of String()) = New List(Of String())
    '        Using reader As New StringReader(content)
    '            While reader.Peek() >= 0
    '                Dim line As String = reader.ReadLine()
    '                If String.IsNullOrWhiteSpace(line) Then
    '                    Continue While
    '                End If
    '                'If Not String.IsNullOrWhiteSpace(line) Then
    '                '    Dim fields As String() = line.Split(","c)
    '                '    If Not fields.All(Function(f) String.IsNullOrWhiteSpace(f.Trim())) Then
    '                '        lines.Add(fields)
    '                '    End If
    '                'End If
    '                Dim fields As String() = line.Split(","c)
    '                If Not fields.All(Function(f) String.IsNullOrWhiteSpace(f.Trim())) Then
    '                    lines.Add(fields)
    '                End If
    '            End While
    '        End Using

    '        If lines.Count = 0 Then
    '            Return Nothing
    '        End If

    '        Dim numRows As Integer = lines.Count
    '        Dim numCols As Integer = lines(0).Length
    '        Dim finalArray As Array = Array.CreateInstance(GetType(Object), {numRows, numCols}, {1, 1})

    '        For i As Integer = 0 To numRows - 1
    '            For j As Integer = 0 To numCols - 1
    '                If j < lines(i).Length Then
    '                    finalArray.SetValue(lines(i)(j), i + 1, j + 1)
    '                Else
    '                    finalArray.SetValue(Nothing, i + 1, j + 1)
    '                End If
    '            Next
    '        Next

    '        Return finalArray
    '    Catch ex As Exception
    '        Throw
    '    End Try
    'End Function


    'Public Function GetChunkArray(filePath As String) As Object
    '    Try
    '        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

    '        Dim safeFilePath As String = sanitize.GetSafeFilePath(filePath)
    '        If Not File.Exists(safeFilePath) Then
    '            Return Nothing
    '        End If

    '        Dim lines As List(Of String()) = New List(Of String())
    '        Using reader As New StreamReader(safeFilePath)
    '            While Not reader.EndOfStream
    '                Dim line As String = reader.ReadLine()
    '                If Not String.IsNullOrWhiteSpace(line) Then
    '                    Dim fields As String() = line.Split(","c)
    '                    If Not fields.All(Function(f) String.IsNullOrWhiteSpace(f)) Then
    '                        lines.Add(fields)
    '                    End If
    '                End If
    '            End While
    '        End Using

    '        If lines.Count = 0 Then
    '            Return Nothing
    '        End If

    '        Dim numRows As Integer = lines.Count
    '        Dim numCols As Integer = lines(0).Length
    '        Dim finalArray As Array = Array.CreateInstance(GetType(Object), {numRows, numCols}, {1, 1})

    '        For i As Integer = 0 To numRows - 1
    '            For j As Integer = 0 To numCols - 1
    '                If j < lines(i).Length Then
    '                    finalArray.SetValue(lines(i)(j), i + 1, j + 1)
    '                Else
    '                    finalArray.SetValue(Nothing, i + 1, j + 1)
    '                End If
    '            Next
    '        Next

    '        Return finalArray
    '    Catch ex As Exception
    '        Throw
    '    End Try
    'End Function


    'Public Function BuildXlsx(dt As DataTable, ScreenName As String) As String
    '    Dim timestamp As String = DateTime.Now.ToString("ddMMyyyy_HH_mm_ss")
    '    Dim fileName As String = $"Resultados_{ScreenName}_{timestamp}.xlsx"
    '    Dim safeFileName As String = sanitize.GetSafeFileName(fileName)
    '    Dim filePath As String = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + safeFileName)

    '    Using workbook As New XLWorkbook()
    '        workbook.Worksheets.Add(dt, "Resultados")
    '        workbook.Worksheet(1).Columns().AdjustToContents()
    '        workbook.SaveAs(filePath)
    '    End Using

    '    Return filePath
    'End Function

    'Public Function BuildXlsx(dt As DataTable) As String
    '    Dim now As DateTime = DateTime.Now
    '    Dim hour12 As Integer = If(now.Hour Mod 12 = 0, 12, now.Hour Mod 12)
    '    Dim hour As String = hour12.ToString() & If(now.Hour < 12, "AM", "PM")
    '    Dim FileName As String = $"Log-CargaExcepciones{now.Month}-{now.Day}-{now.Year}_{hour}.xlsx"

    '    Dim safeFileName As String = sanitize.GetSafeFileName(FileName)

    '    If String.IsNullOrWhiteSpace(safeFileName) Then
    '        Throw New ArgumentException("El nombre del archivo no es válido.")
    '    End If

    '    Dim basePath As String = Path.GetTempPath()
    '    Dim filePath As String = Path.Combine(basePath, safeFileName)

    '    If Not filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) Then
    '        Throw New UnauthorizedAccessException("Ruta de archivo no permitida.")
    '    End If

    '    Using workbook As New XLWorkbook()
    '        workbook.Worksheets.Add(dt, "Resultados")
    '        workbook.Worksheet(1).Columns().AdjustToContents()
    '        workbook.SaveAs(filePath)
    '    End Using

    '    Return filePath
    'End Function

    '''<summary>
    '''Concatena todos los campos de un registro, Detecta duplicidades y posteriormente las elimina.
    '''</summary>
    '''<param name="BiArray">El array Bi-Dimensional a procesar</param>
    '''<returns>El array Bi-Dimensional procesado sin duplicados</returns>
    Public Function DeleteDoppelganger(ByVal BiArray As Object(,)) As Object(,)

        Dim usedRows As Integer = BiArray.GetUpperBound(0)
        Dim usedColumns As Integer = BiArray.GetUpperBound(1)

        Dim uniqueRows As New Dictionary(Of String, Object())()

        For row As Integer = 2 To usedRows
            Dim concatenatedRow As String = ""
            Dim rowData(usedColumns - 1) As Object

            For col As Integer = 1 To usedColumns
                concatenatedRow += CStr(BiArray(row, col))
                rowData(col - 1) = BiArray(row, col)
            Next

            If Not uniqueRows.ContainsKey(concatenatedRow) Then
                uniqueRows.Add(concatenatedRow, rowData)
            End If
        Next

        Dim newUsedRows = uniqueRows.Count + 1
        Dim newExcelArray As Array = Array.CreateInstance(GetType(Object), {newUsedRows, usedColumns}, {1, 1})

        For col As Integer = 1 To usedColumns
            newExcelArray.SetValue(BiArray(1, col), 1, col)
        Next

        Dim rowIndex As Integer = 2
        For Each rowValue In uniqueRows.Values
            For j As Integer = 0 To usedColumns - 1
                newExcelArray.SetValue(rowValue(j), rowIndex, j + 1)
            Next
            rowIndex += 1
        Next

        Return newExcelArray

    End Function

    Public Function DeleteSendedPgP() As Boolean
        Return True
    End Function
#End Region
End Class