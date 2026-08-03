Imports System.Collections.ObjectModel
Imports System.ComponentModel.Design
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Security
Imports System.Threading
Imports System.Web.Http
Imports ClosedXML.Excel
Imports DocumentFormat.OpenXml.Spreadsheet
Imports Microsoft.SqlServer.Server
Imports Microsoft.VisualBasic.Logging
Imports SixLabors.Fonts.Tables.General

Public Class FileController
    Inherits ApiController
#Region "Variables Locales"

    Private mUser As User
    Private mLog As Log
    Private sc As New SharedController
    Private sanitize As New FileClass

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

#End Region

#Region "Clases"
    Public Class ValidateFileRequest
        Property FileType As String
        Property Extension As String
        Property columns As String()
        Property types As String()
        Property nulleable_columns As String()
        Property LogPage As String
        Property LogType As String
        Property LogBody As String
        Property AllowDuplicateEntries As Boolean
    End Class
    Public Class CheckFileRequest
        Public Property FileType As String
        Public Property Extension As String
    End Class
    Public Class DeleteFunction
        Property FilePath As String
    End Class
#End Region

#Region "Metodos POST"
    '''<summary>
    '''Lee un array bidimensional 1-based y valida sus columnas y filas para asegurar que cumple con el formato esperado.
    '''</summary>
    '''<returns>Un Response con un Booleano.</returns>
    <HttpPost>
    <Route("api/files/validate")>
    Public Function ValidateExcelFile(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        mLog = New Log
        Try
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing
            Dim ExcelArray(,) As Object = GetExcelArray(request.FileType, request.Extension)

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
                    realColumns(realColumns.Length - 1) = Convert.ToString(ExcelArray(1, col + 1))
                    StringColumns += request.columns(col)
                    If col < usedColumns - 1 Then StringColumns += ", "
                Next

                realColumns = realColumns.Skip(1).ToArray()
                realColumns = QuitarAcentos(realColumns)
                request.columns = QuitarAcentos(request.columns)

                For col As Integer = 0 To realColumns.Length - 1
                    If CStr(realColumns(col)).Trim().ToLower() <> request.columns(col).Trim.ToLower() Then
                        errorsList = "<tr><td>Nombres de columnas incorrectos</td><td>El archivo no contiene los nombres de columnas correctos, por favor corrija a los titulos esperados: <b>" & StringColumns & "</b>, y vuelva a intentar la carga.</td></tr>"
                        Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
                    End If
                Next

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
                                Dim esFechaValida As Boolean = False
                                esFechaValida = esFechaValida Or Date.TryParseExact(CStr(cellValue), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, tempDate)
                                esFechaValida = esFechaValida Or Date.TryParseExact(CStr(cellValue), "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, tempDate)
                                esFechaValida = esFechaValida Or Date.TryParseExact(CStr(cellValue), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, tempDate)
                                esFechaValida = esFechaValida Or Date.TryParseExact(CStr(cellValue), "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, tempDate)
                                If Not esFechaValida Then
                                    errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> no es una fecha válida.</td></tr>"
                                End If
                            Case "Integer"
                                Dim tempInt As Integer
                                If Not Integer.TryParse(CStr(cellValue), tempInt) Then
                                    errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> no es un número entero válido.</td></tr>"
                                End If
                            Case "Decimal"
                                Dim tempDecimal As Decimal
                                If (CStr(cellValue).Contains("$")) Then
                                    cellValue = CStr(cellValue).Replace("$", "")
                                End If
                                If Not Decimal.TryParse(CStr(cellValue), tempDecimal) Then
                                    errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> no es un número decimal válido.</td></tr>"
                                End If
                            Case "Alphabetic"
                                Dim isAlphabetOnly As Boolean = Regex.IsMatch(cellValue, "^[A-Za-z+]+$")
                                If Not isAlphabetOnly Then
                                    errorsList += "<tr><td>Formato incorrecto en Fila #" & row & "</td><td>El valor en la columna <strong>" & columnName & "</strong> debe contener solo letras.</td></tr>"
                                End If
                        End Select
                    Next
                Next

                If errorsList IsNot Nothing Then Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

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

                    If doesRowHaveAnyData AndAlso isRowPartiallyEmpty Then
                        If allowSomeNulls Then
                            emptyColumns = If(emptyColumns IsNot Nothing, emptyColumns.Substring(0, emptyColumns.Length - 2), emptyColumns)

                            errorsList += "<tr><td>Datos incompletos en Fila #" & row & "</td><td>La fila contiene celda(s) vacía(s): " & emptyColumns & "</td></tr>"
                        Else
                            errorsList += "<tr><td>Datos incompletos en Fila #" & row & "</td><td>La fila debe tener todas sus celdas con información o estar completamente vacía.</td></tr>"
                        End If
                    End If

                    If doesRowHaveAnyData Then ContainsInfo = True
                Next

                If errorsList IsNot Nothing Then
                    Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
                End If

                Return Ok(New With {.d = True})

            End If

            errorsList = "<tr><td>Archivo vacio</td><td>El archivo esta vacio</td></tr>"
            Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
            Return Ok(New With {.d = False})

        Catch ex As Exception
            mLog.insertLog("FileController", "ValidateExcelFile", $"Ocurrió un error: " + ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

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
        Try
            Dim sanitizeClass As New Sanitizacion
            If HttpContext.Current.Session.Item("User") Is Nothing Then Return Unauthorized()
            mLog = New Log()

            Dim rawFileType As String = If(request?.FileType, String.Empty)
            Dim rawExtension As String = If(request?.Extension, String.Empty)

            Dim safeModulo As String = sanitizeClass.GetSafeModulo(rawFileType)
            Dim safeExtension As String = sanitizeClass.GetSafeExtension(rawExtension)

            If safeModulo = "INVALID" OrElse safeExtension = "INVALID" Then
                mLog.insertLog("FileController", "CheckFileExists", "Parámetros Módulo o Extensión no permitidos")
                Return BadRequest("Parámetros Módulo o Extensión no permitidos.")
            End If

            Dim baseDir As String = Path.GetFullPath(HttpContext.Current.Server.MapPath("~/UploadedFiles/"))
            If Not baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()) Then baseDir &= Path.DirectorySeparatorChar

            Dim userEmail As String = CType(HttpContext.Current.Session.Item("User"), User).Email
            Dim fileName As String = Path.GetFileName(userEmail & safeExtension)
            Dim fullPath As String = Path.GetFullPath(Path.Combine(baseDir, safeModulo, fileName))

            If Not fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase) Then
                Return BadRequest("Intento de Path Traversal bloqueado.")
            End If

            If File.Exists(fullPath) Then
                If safeExtension = ".csv" Then
                    Try
                        Dim firstLine As String = File.ReadLines(fullPath).FirstOrDefault()
                        If firstLine IsNot Nothing AndAlso Not firstLine.Contains(",") Then
                            Dim errorHtml As String = "<tr><td>Delimitador incorrecto</td><td>El archivo CSV debe estar delimitado por comas (,).</td></tr>"
                            mLog.InsertApplicationLog("FileController", "CheckFileExists", "Error", "El archivo importado no contiene el delimitador correcto")
                            Return Ok(New With {.d = sc.TableBuilder(errorHtml, 1)})
                        End If
                    Catch ex As Exception
                        Dim errorHtml As String = "<tr><td>Error de lectura</td><td>No se pudo leer el archivo CSV para validación.</td></tr>"
                        mLog.InsertApplicationLog("FileController", "CheckFileExists", "Error", $"Error al intentar leer el archivo {ex}")
                        Return Ok(New With {.d = sc.TableBuilder(errorHtml, 1)})
                    End Try
                End If

                Return Ok(New With {.d = True})
            Else
                Return Ok(New With {.d = False, .m = "No se pudo cargar el documento porque no existe en la carpeta del servidor, vuelva a intentar la carga."})
            End If
        Catch ex As Exception
            mLog.insertLog("FileController", "CheckFileExists", $"Ocurrió un error al revisar el archivo " + ex.Message)
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
        mLog = New Log
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
            mLog.insertLog("FileController", "DownloadFile", $"Ocurrió un error: " + ex.Message)
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
        mLog = New Log
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
            mLog.insertLog("FileController", "DeleteFileFunction", $"Ocurrió un error: " + ex.Message)
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
        mLog = New Log
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
            mLog.insertLog("FileController", "DeleteAllFunction", $"Ocurrió un error: " + ex.Message)
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

    '''<summary>
    '''Lee un Archivo Excel almacenado en UploadedFiles y convierte su contenido en un array bidimensional 1-based.
    '''</summary>
    '''<returns>Un array de objetos (Object(,)) 1-based que contiene los datos de la hoja de cálculo.</returns>
    '''<remarks>
    '''Esta funcion usa la libreriía ClosedXML para leer un archivo .xlsx.
    '''A diferencia de los arrays de .NET, que son 0-based, este método retorna un array 1-based para que coincida con la numeración de filas y columnas de Excel.
    '''</remarks>
    Public Function GetExcelArray(Modulo As String, Extension As String, Optional AllowDuplicateEntries As Boolean = False) As Object
        Dim finalArray As Object = Nothing

        Dim safeModulo As String = Modulo.Replace("..", "").Replace("\\", "").Replace("//", "")
        Dim safeExtension As String = Path.GetExtension(Extension)

        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Dim filePath As String = HttpContext.Current.Server.MapPath("~\UploadedFiles\" + safeModulo + "\" + mUser.Email + safeExtension)

        If safeExtension = ".csv" Then
            finalArray = GetCSVArray(filePath)

            Dim result As Object(,) = finalArray
            If (Not AllowDuplicateEntries) Then
                result = DeleteDoppelganger(finalArray)
            End If

            Return result
        End If

        Using workbook As New XLWorkbook(filePath)
            Dim worksheet = workbook.Worksheet(1)
            If worksheet Is Nothing OrElse worksheet.LastCellUsed() Is Nothing Then Return Nothing

            Dim range = worksheet.RangeUsed()
            Dim numCols As Integer = range.ColumnCount()

            If numCols < 3 Then numCols = 3
            Dim dataRows As New List(Of Object())

            For Each row In range.Rows()
                If row.IsEmpty() Then Continue For

                Dim rowArray(numCols - 1) As Object
                Dim isRowEmpty As Boolean = True

                For j As Integer = 0 To numCols - 1
                    Dim cell = row.Cell(j + 1)
                    Dim cellValue As Object = Nothing

                    If Not cell.IsEmpty() Then
                        If cell.DataType = XLDataType.DateTime Then
                            cellValue = cell.GetDateTime().ToString("dd/MM/yyyy")
                        Else
                            cellValue = cell.GetFormattedString()
                        End If
                    End If

                    rowArray(j) = cellValue
                    If j < 3 AndAlso cellValue IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cellValue.ToString()) Then isRowEmpty = False
                Next

                If Not isRowEmpty Then dataRows.Add(rowArray)

            Next

            If dataRows.Count = 0 Then Return Nothing

            Dim finalNumRows As Integer = dataRows.Count
            Dim finalNumCols As Integer = dataRows(0).Length
            finalArray = Array.CreateInstance(GetType(Object), {finalNumRows, finalNumCols}, {1, 1})

            For i As Integer = 0 To finalNumRows - 1
                For j As Integer = 0 To finalNumCols - 1
                    finalArray(i + 1, j + 1) = dataRows(i)(j)
                Next
            Next

            Dim result As Object(,) = finalArray
            If (Not AllowDuplicateEntries) Then
                result = DeleteDoppelganger(finalArray)
            End If

            Return result
        End Using
    End Function

    '''<summary>
    '''Lee un Archivo CSV almacenado en UploadedFiles y convierte su contenido en un array bidimensional 1-based.
    '''</summary>
    '''<returns>Un array de objetos (Object(,)) 1-based que contiene los datos del Archivo.</returns>
    '''<remarks>
    '''Esta funcion usa la libreriía ClosedXML para leer un archivo .xlsx.
    '''A diferencia de los arrays de .NET, que son 0-based, este método retorna un array 1-based para que coincida con la numeración de filas y columnas del CSV.
    '''</remarks>
    Public Function GetCSVArray(filePath As String) As Object
        Try
            Dim safeFilePath As String = sanitize.GetSafeFilePath(filePath)
            If Not File.Exists(safeFilePath) Then
                Return Nothing
            End If

            If Not File.Exists(safeFilePath) Then
                Return Nothing
            End If

            Dim content As String
            content = File.ReadAllText(safeFilePath, System.Text.Encoding.UTF8)

            If content.Contains("") Then
                content = File.ReadAllText(safeFilePath, System.Text.Encoding.GetEncoding(1252)) ' 1252 es la página de códigos para Windows - 1252
            End If

            Dim lines As List(Of String()) = New List(Of String())
            Using reader As New StringReader(content)
                While reader.Peek() >= 0
                    Dim line As String = reader.ReadLine()
                    If String.IsNullOrWhiteSpace(line) Then
                        Continue While
                    End If
                    'If Not String.IsNullOrWhiteSpace(line) Then
                    '    Dim fields As String() = line.Split(","c)
                    '    If Not fields.All(Function(f) String.IsNullOrWhiteSpace(f.Trim())) Then
                    '        lines.Add(fields)
                    '    End If
                    'End If
                    Dim fields As String() = line.Split(","c)
                    If Not fields.All(Function(f) String.IsNullOrWhiteSpace(f.Trim())) Then
                        lines.Add(fields)
                    End If
                End While
            End Using

            If lines.Count = 0 Then
                Return Nothing
            End If

            Dim numRows As Integer = lines.Count
            Dim numCols As Integer = lines(0).Length
            Dim finalArray As Array = Array.CreateInstance(GetType(Object), {numRows, numCols}, {1, 1})

            For i As Integer = 0 To numRows - 1
                For j As Integer = 0 To numCols - 1
                    If j < lines(i).Length Then
                        finalArray.SetValue(lines(i)(j), i + 1, j + 1)
                    Else
                        finalArray.SetValue(Nothing, i + 1, j + 1)
                    End If
                Next
            Next

            Return finalArray
        Catch ex As Exception
            Throw
        End Try
    End Function

    '''<summary>
    '''Lee un Archivo CSV almacenado en UploadedFiles y convierte su contenido en un array bidimensional 1-based.
    '''</summary>
    '''<returns>Un array de objetos (Object(,)) 1-based que contiene los datos del Archivo.</returns>
    '''<remarks>
    '''Esta funcion usa la libreriía ClosedXML para leer un archivo .xlsx.
    '''A diferencia de los arrays de .NET, que son 0-based, este método retorna un array 1-based para que coincida con la numeración de filas y columnas del CSV.
    '''</remarks>
    Public Function GetChunkArray(filePath As String) As Object
        Try
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim safeFilePath As String = sanitize.GetSafeFilePath(filePath)
            If Not File.Exists(safeFilePath) Then
                Return Nothing
            End If

            Dim lines As List(Of String()) = New List(Of String())
            Using reader As New StreamReader(safeFilePath)
                While Not reader.EndOfStream
                    Dim line As String = reader.ReadLine()
                    If Not String.IsNullOrWhiteSpace(line) Then
                        Dim fields As String() = line.Split(","c)
                        If Not fields.All(Function(f) String.IsNullOrWhiteSpace(f)) Then
                            lines.Add(fields)
                        End If
                    End If
                End While
            End Using

            If lines.Count = 0 Then
                Return Nothing
            End If

            Dim numRows As Integer = lines.Count
            Dim numCols As Integer = lines(0).Length
            Dim finalArray As Array = Array.CreateInstance(GetType(Object), {numRows, numCols}, {1, 1})

            For i As Integer = 0 To numRows - 1
                For j As Integer = 0 To numCols - 1
                    If j < lines(i).Length Then
                        finalArray.SetValue(lines(i)(j), i + 1, j + 1)
                    Else
                        finalArray.SetValue(Nothing, i + 1, j + 1)
                    End If
                Next
            Next

            Return finalArray
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Crea un archivo .xlsx a partir de un DataTable y lo guarda en el servidor.
    ''' </summary>
    ''' <param name="dt">El DataTable que contiene los datos para el archivo Excel.</param>
    ''' <returns>La ruta absoluta del archivo .xlsx generado en el servidor.</returns>
    Public Function BuildXlsx(dt As DataTable, ScreenName As String) As String
        Dim timestamp As String = DateTime.Now.ToString("ddMMyyyy_HH_mm_ss")
        Dim fileName As String = $"Resultados_{ScreenName}_{timestamp}.xlsx"
        Dim safeFileName As String = sanitize.GetSafeFileName(fileName)
        Dim filePath As String = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + safeFileName)

        Using workbook As New XLWorkbook()
            workbook.Worksheets.Add(dt, "Resultados")
            workbook.Worksheet(1).Columns().AdjustToContents()
            workbook.SaveAs(filePath)
        End Using

        Return filePath
    End Function

    Public Function BuildXlsx(dt As DataTable) As String
        Dim now As DateTime = DateTime.Now
        Dim hour12 As Integer = If(now.Hour Mod 12 = 0, 12, now.Hour Mod 12)
        Dim hour As String = hour12.ToString() & If(now.Hour < 12, "AM", "PM")
        Dim FileName As String = $"Log-CargaExcepciones{now.Month}-{now.Day}-{now.Year}_{hour}.xlsx"

        Dim safeFileName As String = sanitize.GetSafeFileName(FileName)

        If String.IsNullOrWhiteSpace(safeFileName) Then
            Throw New ArgumentException("El nombre del archivo no es válido.")
        End If

        Dim basePath As String = Path.GetTempPath()
        Dim filePath As String = Path.Combine(basePath, safeFileName)

        If Not filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) Then
            Throw New UnauthorizedAccessException("Ruta de archivo no permitida.")
        End If

        Using workbook As New XLWorkbook()
            workbook.Worksheets.Add(dt, "Resultados")
            workbook.Worksheet(1).Columns().AdjustToContents()
            workbook.SaveAs(filePath)
        End Using

        Return filePath
    End Function

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