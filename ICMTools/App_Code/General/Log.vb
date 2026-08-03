Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Results
Imports System.Web.WebPages
Imports ClosedXML.Excel
Imports DocumentFormat.OpenXml.Features
Imports DocumentFormat.OpenXml.Office2010.Excel
Imports ICMTools
Imports Microsoft.SqlServer
Imports Npgsql
Imports NpgsqlTypes

Public Class Log

    Private mUser As New User

#Region "Variables Locales"
    Private Npgsql As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
#End Region
    Public Sub insertLog(ModuleName As String, EventType As String, Description As String)
        If HttpContext.Current Is Nothing OrElse HttpContext.Current.Session Is Nothing OrElse HttpContext.Current.Session.Item("User") Is Nothing Then
            NoSessionInsertLog(ModuleName, EventType, Description)
        End If
        mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Dim HostAddress As String = HttpContext.Current.Request.UserHostAddress
        Dim sqlCommand As String = "CALL public.spicmtoolsactivityloginsert(:paramuser, :parammodel, :parammodulename, :parameventtype, :paramdescription, :paramhost)"
        Using conn As New NpgsqlConnection(Npgsql)
            conn.Open()
            Try
                Using cmd As New NpgsqlCommand(sqlCommand, conn)
                    cmd.Parameters.AddWithValue("paramuser", mUser.Email)
                    cmd.Parameters.AddWithValue("parammodel", mUser.Model)
                    cmd.Parameters.AddWithValue("parammodulename", ModuleName)
                    cmd.Parameters.AddWithValue("parameventtype", EventType)
                    cmd.Parameters.AddWithValue("paramdescription", Description)
                    cmd.Parameters.AddWithValue("paramhost", HostAddress)

                    cmd.ExecuteNonQuery()
                End Using
            Catch ex As NpgsqlException
                Console.WriteLine("Error de PostgreSQL: " & ex.Message)
                Throw
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' Insert Application Log
    ''' </summary>
    ''' <param name="ModuleName">Module</param>
    ''' <param name="EventType">Event</param>
    ''' <param name="LogType">Type</param>
    ''' <param name="Description">Description</param>
    Public Sub InsertApplicationLog(ModuleName As String, EventType As String, LogType As String, Description As String)
        Using conn As New NpgsqlConnection(Npgsql)
            conn.Open()
            Try
                Dim sqlCommand As String = "CALL ""spICMToolsApplicationLogInsert""(@p_module, @p_event, @p_type, @p_description);"
                Using cmd As New NpgsqlCommand(sqlCommand, conn)
                    cmd.Parameters.AddWithValue("p_module", ModuleName)
                    cmd.Parameters.AddWithValue("p_event", EventType)
                    cmd.Parameters.AddWithValue("p_type", LogType)
                    cmd.Parameters.AddWithValue("p_description", Description)
                    cmd.ExecuteNonQuery()
                End Using
            Catch ex As Exception
                Throw
            End Try
        End Using
    End Sub

    Public Sub NoSessionInsertLog(ModuleName As String, EventType As String, Description As String)
        'Dim HostAddress As String = HttpContext.Current.Request.UserHostAddress
        Dim sqlCommand As String = "CALL public.spicmtoolsactivityloginsert(:paramuser, :parammodel, :parammodulename, :parameventtype, :paramdescription, :paramhost)"
        Dim Email As String = "ICMTools"
        Dim Model As String = "FEMCOEPPRD"

        Using conn As New NpgsqlConnection(Npgsql)
            conn.Open()
            Try
                Using cmd As New NpgsqlCommand(sqlCommand, conn)
                    cmd.Parameters.AddWithValue("paramuser", Email)
                    cmd.Parameters.AddWithValue("parammodel", Model)
                    cmd.Parameters.AddWithValue("parammodulename", ModuleName)
                    cmd.Parameters.AddWithValue("parameventtype", EventType)
                    cmd.Parameters.AddWithValue("paramdescription", Description)
                    'cmd.Parameters.AddWithValue("paramhost", HostAddress)
                    cmd.Parameters.AddWithValue("paramhost", "0.0.0.0")

                    cmd.ExecuteNonQuery()
                End Using
            Catch ex As NpgsqlException
                Console.WriteLine("Error de PostgreSQL: " & ex.Message)
                Throw
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' Método que envía una notificación de error.
    ''' </summary>
    ''' <param name="appException">Excepción</param>
    Public Sub NotificacionError(appException As Exception, Optional pantalla As String = "- Sin Especificar -")
        Dim archivoCsv As String = String.Empty
        Try
            Dim innerException As String = " - Sin Información - "
            Dim exceptionSource As String = "- Sin Información -"
            Dim exceptionCode As String = "- Sin Información -"
            Dim exceptionMessage As String = "- Sin Información -"
            Dim exceptionStackTrace As String = "- Sin Información -"

            If (appException IsNot Nothing) Then
                exceptionSource = appException.Source
                exceptionCode = appException.HResult
                exceptionMessage = appException.Message
                exceptionStackTrace = Truncar(appException.StackTrace, 8000)
                If (appException.InnerException IsNot Nothing) Then
                    innerException = Truncar(appException.InnerException.ToString(), 3000)
                End If

                exceptionStackTrace = HttpUtility.HtmlEncode(exceptionStackTrace)
                innerException = HttpUtility.HtmlEncode(innerException)
                exceptionMessage = HttpUtility.HtmlEncode(exceptionMessage)
            End If

            Dim IP As String = "0.0.0.0"
            Dim SessionValues As String = ""

            If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Session IsNot Nothing Then
                IP = HttpContext.Current.Request.UserHostAddress

                For Each key As String In HttpContext.Current.Session.Keys
                    Dim valor = HttpContext.Current.Session(key)
                    If valor IsNot Nothing And valor.ToString().Equals("ICMTools.User") Then
                        Dim userValues As ICMTools.User = valor
                        SessionValues &= String.Format("<tr><td style='padding: 8px 12px;'>{0}</td><td style='padding: 8px 12px;'>{1}</td></tr>", "Email", userValues.Email)
                        SessionValues &= String.Format("<tr><td style='padding: 8px 12px;'>{0}</td><td style='padding: 8px 12px;'>{1}</td></tr>", "Model", userValues.Model)
                    Else
                        SessionValues &= String.Format("<tr><td style='padding: 8px 12px;'>{0}</td><td style='padding: 8px 12px;'>{1}</td></tr>", key, If(valor IsNot Nothing, valor.ToString(), "- NOTHING -"))
                    End If
                Next
            End If

            If (SessionValues.Length.Equals(0)) Then
                SessionValues = "<tr><td style='padding: 8px 12px;'>- Ninguna -</td><td style='padding: 8px 12px;'>- Sin sesión activa -</td></tr>"
            End If

            archivoCsv = GenerarArchivoCsv(pantalla)
            If (archivoCsv.Equals("TAMAÑO_EXCESIVO")) Then
                exceptionMessage += " <br/><b>NOTA</b>: El archivo CSV no fue procesado, ya que excede el límite de tamaño permitido."
            End If

            Dim parametros As New Dictionary(Of String, String) From {
                        {"@Pantalla", pantalla},
                        {"@ExceptionDate", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString()},
                        {"@IP", IP},
                        {"@SessionValues", SessionValues},
                        {"@ExceptionSource", exceptionSource},
                        {"@ExceptionCode", exceptionCode},
                        {"@ExceptionMessage", exceptionMessage},
                        {"@StackTrace", exceptionStackTrace},
                        {"@InnerException", innerException}
                    }

            Dim correo As New PlantillaCorreo()
            correo.ArchivoAdjunto = archivoCsv
            correo.Parametros = parametros
            correo.Enviar(9)
        Catch ex As Exception
            InsertApplicationLog("Log", "NotificacionError", "Error", ex.Message)
        Finally
            Try
                If (Not String.IsNullOrWhiteSpace(archivoCsv) And System.IO.File.Exists(archivoCsv)) Then
                    System.IO.File.Delete(archivoCsv)
                End If
            Catch ex2 As Exception
                InsertApplicationLog("Log", "NotificacionError", "Error", ex2.Message)
            End Try
        End Try
    End Sub

    Private Function GenerarArchivoCsv(pantalla As String) As String
        Dim archivo As String = String.Empty
        Dim oDataTable As New DataTable()
        Try
            Dim oUser As New User
            If (HttpContext.Current.Session.Item("User") IsNot Nothing) Then
                oUser = CType(HttpContext.Current.Session.Item("User"), User)
            End If

            Using conn As New NpgsqlConnection(Npgsql)
                conn.Open()

                If (pantalla.ToLower().Equals("excepciones")) Then
                    Dim sociedad As String = HttpContext.Current.Session("Sociedad")
                    Dim divisionPersonal As String = HttpContext.Current.Session("DivisionPersonal")

                    If (Not String.IsNullOrEmpty(sociedad) AndAlso Not String.IsNullOrEmpty(divisionPersonal)) Then
                        Dim sql As String = "SELECT * FROM ""Excepciones_NotificacionError""(@p_email, @p_sociedad, @p_divisionpersonal);"
                        Using cmd As New NpgsqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@p_email", oUser.Email)
                            cmd.Parameters.AddWithValue("@p_sociedad", HttpContext.Current.Session("Sociedad"))
                            cmd.Parameters.AddWithValue("@p_divisionpersonal", HttpContext.Current.Session("DivisionPersonal"))
                            Using da As New NpgsqlDataAdapter(cmd)
                                da.Fill(oDataTable)
                            End Using
                        End Using
                    End If
                End If

            End Using

            If (oDataTable IsNot Nothing AndAlso oDataTable.Rows.Count > 0) Then
                Dim nombreArchivo As String = String.Format("{0}_{1}.csv", pantalla, DateTime.Now.Ticks.ToString())
                archivo = DataTableToCsv(oDataTable, nombreArchivo)

                If (Not ValidarTamañoCsv(archivo)) Then
                    archivo = "TAMAÑO_EXCESIVO"
                End If
            End If

            Return archivo
        Catch ex As Exception
            InsertApplicationLog("Log", "GenerarArchivoCsv", "Error", ex.Message)
            Return String.Empty
        End Try
    End Function

    Private Function DataTableToCsv(dt As DataTable, nombreArchivo As String) As String
        Try
            Dim directorio As String = Path.GetFullPath(HttpContext.Current.Server.MapPath("~/UploadedFiles/"))
            Dim archivo As String = System.IO.Path.Combine(directorio, nombreArchivo)
            Dim separador As String = ","

            Using writer As New StreamWriter(archivo, False, Encoding.UTF8)

                ' Encabezados
                Dim headers As New List(Of String)
                For Each col As DataColumn In dt.Columns
                    headers.Add(EnvolverCsv(col.ColumnName))
                Next
                writer.WriteLine(String.Join(separador, headers))

                ' Filas
                For Each row As DataRow In dt.Rows
                    Dim campos As New List(Of String)

                    For Each col As DataColumn In dt.Columns
                        Dim valor As String = If(row(col) IsNot DBNull.Value, row(col).ToString(), "")
                        campos.Add(EnvolverCsv(valor))
                    Next

                    writer.WriteLine(String.Join(separador, campos))
                Next
            End Using

            Return archivo
        Catch ex As Exception
            InsertApplicationLog("Log", "DataTableToCsv", "Error", ex.Message)
            Return String.Empty
        End Try
    End Function

    Private Function EnvolverCsv(valor As String) As String
        valor = valor.Replace("""", """""") ' Escapar comillas
        Return """" + valor + """"
    End Function

    Private Function ValidarTamañoCsv(archivoCsv As String) As Boolean
        Dim esValido As Boolean = True
        Try

            If IO.File.Exists(archivoCsv) Then
                Dim info As New IO.FileInfo(archivoCsv)
                Dim tamañoBytes As Long = info.Length
                Dim maxSize As Long = 15L * 1024 * 1024 ' 15 MB
                If tamañoBytes > maxSize Then
                    IO.File.Delete(archivoCsv)
                    esValido = False
                End If
            End If

            Return esValido
        Catch ex As Exception
            InsertApplicationLog("Log", "GenerarArchivoCsv", "Error", ex.Message)
            Return False
        End Try
    End Function

    Private Function Truncar(texto As String, maxLength As Integer) As String
        If String.IsNullOrEmpty(texto) Then
            Return "- Sin Información -"
        End If

        If texto.Length <= maxLength Then
            Return texto
        End If

        Return texto.Substring(0, maxLength) & vbCrLf & "[TRUNCADO – " + (texto.Length - maxLength).ToString() + " caracteres omitidos]"
    End Function

End Class
