Imports System.Configuration
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports AjaxControlToolkit.HtmlEditor
Imports ClosedXML.Excel
Imports DocumentFormat.OpenXml.Drawing.Diagrams
Imports Microsoft.Office
Imports NCrontab
Imports Npgsql

Public Class JobSchedulerService
    Private Shared ReadOnly _lastrunMap As New Dictionary(Of Integer, DateTime)()
    Private ReadOnly _connectionString As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    Private ReadOnly mLog As Log
    Private ReadOnly ws As New WebServiceICMGeneral()
    Private ReadOnly fc As New FileController

    Public Sub New()
        Me.mLog = New Log()
    End Sub

    ''' <summary>
    ''' Construye el archivo Xlsx
    ''' </summary>
    ''' <param name="ds">DataSet</param>
    ''' <returns>Regresa la ruta del archivo</returns>
    Public Function BuildXlsx(ds As DataSet) As String
        Dim now As DateTime = DateTime.Now
        Dim hour12 As Integer = If(now.Hour Mod 12 = 0, 12, now.Hour Mod 12)
        Dim hour As String = hour12.ToString() & If(now.Hour < 12, "AM", "PM")
        Dim FileName As String = $"Log-CargaExcepciones{now.Month}-{now.Day}-{now.Year}_{hour}.xlsx"

        Dim basePath As String = Path.GetTempPath()
        Dim filePath As String = Path.Combine(basePath, FileName)

        If Not filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) Then
            Throw New UnauthorizedAccessException("Ruta de archivo no permitida.")
        End If

        Using workbook As New XLWorkbook()
            For Each table As DataTable In ds.Tables
                workbook.Worksheets.Add(table, table.TableName)
            Next
            workbook.Worksheet(1).Columns().AdjustToContents()
            workbook.SaveAs(filePath)
        End Using

        Return filePath
    End Function

    Public Sub SendLogByMail()
        Dim FilePath As String = Nothing
        Dim dsXlsx As New DataSet
        Try
            Dim MailList As List(Of String) = GetExceptionLogMail()
            Dim CC As List(Of String) = GetExceptionLogCC()

            Using conn As New NpgsqlConnection(_connectionString)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""GetExceptionsLots""()", conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Using LogTable As New DataTable
                            adapter.Fill(LogTable)
                            LogTable.TableName = "Lotes"
                            dsXlsx.Tables.Add(LogTable.Copy())
                        End Using
                    End Using
                End Using

                Using cmd As New NpgsqlCommand("SELECT * FROM ""GetExceptionsUploadLog""()", conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Using LogTable As New DataTable
                            adapter.Fill(LogTable)
                            LogTable.TableName = "Resultados"
                            dsXlsx.Tables.Add(LogTable.Copy())
                        End Using
                    End Using
                End Using
            End Using

            If (dsXlsx.Tables(0).Rows.Count.Equals(0) And dsXlsx.Tables(1).Rows.Count.Equals(0)) Then
                mLog.InsertApplicationLog("SendLogByMail", "Correos Enviados", "NoRecords", "No se encontraron Excepciones por enviar")
                Return
            End If

            Dim Fecha As String = DateTime.Now.ToString("dddd d 'de' MMMM", New System.Globalization.CultureInfo("es-MX"))
            Dim partes As String() = Fecha.Split(" "c)
            Fecha = Char.ToUpper(partes(0)(0)) & partes(0).Substring(1) & " " &
            partes(1) & " " &
            partes(2) & " " &
            Char.ToUpper(partes(3)(0)) & partes(3).Substring(1)
            Dim cdmxNow As DateTime = DateTime.UtcNow.AddHours(-6)
            Dim hour12 As Integer = If(cdmxNow.Hour Mod 12 = 0, 12, cdmxNow.Hour Mod 12)
            Dim Hora As String = hour12.ToString() & If(cdmxNow.Hour < 12, "AM", "PM")

            FilePath = BuildXlsx(dsXlsx)

            Dim Body As String = If(GetBodyMail(22), String.Empty)

            ws.WebServiceSendSomeMails(MailList, CC, $"ICMTools - Carga de Excepciones {Fecha} a las {Hora} (Zona Horaria GMT-6).", Body, "femcoepprd", FilePath)

            mLog.InsertApplicationLog("SendLogByMail", "Correos Enviados", "Success", "Log de carga de Excepciones enviados correctamente")
        Catch ex As Exception
            Dim errorMsg As String = ex.ToString().Replace(Environment.NewLine, " ").Replace(vbCrLf, " ").Replace(vbLf, " ")
            mLog.InsertApplicationLog("SendLogByMail", "Error al enviar el Mail", "Error", "Ocurrio un error al ejecutar la funcion " & errorMsg)
            Throw
        Finally
            If FilePath IsNot Nothing AndAlso File.Exists(FilePath) Then
                Try
                    File.Delete(FilePath)
                Catch ex As IOException
                    mLog.InsertApplicationLog("SendLogByMail", "Error al eliminar archivo temporal", "Warning", ex.Message)
                End Try
            End If
        End Try
    End Sub

    Public Sub CheckAndExcecuteJobs()
        Try
            Dim jobs As List(Of JobConfig) = GetActiveJobs()

            For Each job In jobs
                For Each schedule In job.Schedules
                    Try
                        Dim now As DateTime = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"))
                        Dim cron = CrontabSchedule.Parse(schedule.CRON_EXPRESSION)
                        Dim nextRun = cron.GetNextOccurrence(now)

                        If nextRun <= now Or (nextRun - now).TotalMinutes < 5 Then
                            mLog.InsertApplicationLog("JobSchedulerService", "CheckAndExecuteJobs", "LOG DE EJECUCION DE JOB", "Ejecutando Job: " & job.JOB_NAME)
                            If job.FUNCTION_NAME = "spICMToolsExceptionsLotsStatusWM" Then
                                ExecuteJobFunction(job.FUNCTION_NAME)
                            Else
                                ExcecuteJobSP(job.FUNCTION_NAME)
                            End If


                            If _lastrunMap.ContainsKey(schedule.ID) Then
                                _lastrunMap(schedule.ID) = now
                            Else
                                _lastrunMap.Add(schedule.ID, now)
                            End If


                        End If
                    Catch ex As Exception
                        mLog.InsertApplicationLog("JobSchedulerService", "CheckAndExecuteJobs", "LOG DE EJECUCION DE JOB", "Error procesando schedule " & schedule.ID & " del job " & job.JOB_NAME & ": " + ex.Message)
                    End Try
                Next
            Next
        Catch ex As Exception
            mLog.InsertApplicationLog("JobSchedulerService", "CheckAndExecuteJobs", "LOG DE EJECUCION DE JOB", "Error general en JobScheduler" + ex.Message)
        End Try
    End Sub

    Private Sub ExcecuteJobSP(functionName As String)
        Using conn As New NpgsqlConnection(_connectionString)
            conn.Open()
            Dim cb As New NpgsqlCommandBuilder()
            Dim safeFunctionName As String = cb.QuoteIdentifier(functionName)
            Using cmd As New NpgsqlCommand("CALL " & safeFunctionName & "();", conn)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Sub ExecuteJobFunction(functionName As String)
        Dim responseLots As New List(Of LotsResponse)

        Using conn As New NpgsqlConnection(_connectionString)
            conn.Open()

            Dim cb As New NpgsqlCommandBuilder()

            Dim safeFunctionName As String = cb.QuoteIdentifier(functionName)

            Using cmd As New NpgsqlCommand("SELECT * FROM " & safeFunctionName & "();", conn)

                Using reader = cmd.ExecuteReader
                    While reader.Read()
                        Dim j As New LotsResponse()

                        j.O_Lot = Convert.ToInt32(reader("O_Lot"))
                        j.O_Status = reader("O_Status").ToString()
                        j.O_Date = Convert.ToDateTime(reader("O_Date"))
                        j.O_Subject = reader("O_Subject").ToString()
                        j.O_Body = reader("O_Body").ToString()
                        j.O_To = reader("O_To").ToString()
                        j.O_Cc = reader("O_Cc").ToString()

                        responseLots.Add(j)
                    End While
                End Using
            End Using
        End Using

        If responseLots.Count = 0 Then
            mLog.InsertApplicationLog("JobSchedulerService", "ExecuteJobFunction", "LOG DE EJECUCION DE FUNCION", "No se encontraron registros para notificar en Lotes de Excepciones")
            Return
        End If

        For Each i In responseLots
            Dim strTo As String = If(i.O_To, String.Empty)
            Dim strCc As String = If(i.O_Cc, String.Empty)

            Dim listTo As List(Of String) = strTo.Split(New Char() {";"c}, StringSplitOptions.RemoveEmptyEntries).ToList()
            Dim listCc As List(Of String) = strCc.Split(New Char() {";"c}, StringSplitOptions.RemoveEmptyEntries).ToList()

            Dim mailBody As String = i.O_Body
            Dim mailSubject As String = i.O_Subject

            ws.WebServiceSendMail(listTo, listCc, mailSubject, mailBody, "femcoepdev")

        Next
    End Sub


    Private Function GetActiveJobs() As List(Of JobConfig)
        Dim jobs As New List(Of JobConfig)
        Dim schedules As New List(Of JobSchedule)

        Using conn As New NpgsqlConnection(_connectionString)
            conn.Open()
            Using cmd As New NpgsqlCommand("SELECT ""ID"",""JOB_NAME"", ""FUNCTION_NAME"", ""IS_ACTIVE"" FROM ""JOB_CONFIG"" WHERE ""IS_ACTIVE"" = true;", conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim j As New JobConfig()
                        j.ID = Convert.ToInt32(reader("ID"))
                        j.JOB_NAME = reader("JOB_NAME").ToString()
                        j.FUNCTION_NAME = reader("FUNCTION_NAME").ToString()
                        j.IS_ACTIVE = reader("IS_ACTIVE").ToString()
                        jobs.Add(j)
                    End While
                End Using
            End Using

            Using cmd As New NpgsqlCommand("SELECT ""ID"", ""JOB_ID"", ""CRON_EXPRESSION"", ""IS_ACTIVE"" FROM ""JOB_SCHEDULE"" WHERE ""IS_ACTIVE"" = true;", conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim s As New JobSchedule()
                        s.ID = Convert.ToInt32(reader("ID"))
                        s.JOB_ID = Convert.ToInt32(reader("JOB_ID"))
                        s.CRON_EXPRESSION = reader("CRON_EXPRESSION").ToString()
                        s.IS_ACTIVE = Convert.ToBoolean(reader("IS_ACTIVE"))
                        schedules.Add(s)
                    End While
                End Using
            End Using
        End Using

        For Each job In jobs
            job.Schedules = schedules.Where(Function(s) s.JOB_ID = job.ID).ToList()
        Next

        Return jobs
    End Function

    Private Function GetExceptionLogMail() As List(Of String)
        Dim MailList As New List(Of String)
        Try
            Using conn As New NpgsqlConnection(_connectionString)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""GetMailsExceptionLog""()", conn)
                    Using reader = cmd.ExecuteReader
                        While reader.Read()
                            MailList.Add(reader("Mail").ToString())
                        End While
                    End Using
                End Using
            End Using
            Return MailList
        Catch ex As Exception
            Dim errorMsg As String = ex.ToString().Replace(Environment.NewLine, " ").Replace(vbCrLf, " ").Replace(vbLf, " ")
            mLog.InsertApplicationLog("GetExceptionLogMail", "Envio Logs Excepciones", "Error", "Ocurrio un error al obtener la lista de Mails " & errorMsg)
            Throw
        End Try
    End Function

    Private Function GetExceptionLogCC() As List(Of String)
        Dim MailList As New List(Of String)
        Try
            Using conn As New NpgsqlConnection(_connectionString)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""GetMailsExceptionLogCC""()", conn)
                    Using reader = cmd.ExecuteReader
                        While reader.Read()
                            MailList.Add(reader("Mail").ToString())
                        End While
                    End Using
                End Using
            End Using
            Return MailList
        Catch ex As Exception
            Dim errorMsg As String = ex.ToString().Replace(Environment.NewLine, " ").Replace(vbCrLf, " ").Replace(vbLf, " ")
            mLog.InsertApplicationLog("GetExceptionLogMail", "Envio Logs Excepciones", "Error", "Ocurrio un error al obtener la lista de correos CC " & errorMsg)
            Throw
        End Try
    End Function

    Private Function GetBodyMail(id As Integer) As String
        Dim Body As String
        Try
            Using conn As New NpgsqlConnection(_connectionString)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT ""Body"" FROM ""ICMToolsReports"" WHERE ""ReportId"" = @bodyId", conn)
                    cmd.Parameters.AddWithValue("bodyId", id)
                    Body = Convert.ToString(cmd.ExecuteScalar())
                End Using
            End Using
            Return Body
        Catch ex As Exception
            Dim errorMsg As String = ex.ToString().Replace(Environment.NewLine, " ").Replace(vbCrLf, " ").Replace(vbLf, " ")
            mLog.InsertApplicationLog("GetBodyMail", "Obtencion de Body Table", "Error", "Ocurrio un error al obtener el body " & errorMsg)
            Throw
        End Try
    End Function

End Class