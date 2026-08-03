Option Strict On
Option Explicit On

Imports System.IO
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.UI.WebControls.Expressions
Imports DocumentFormat.OpenXml.Drawing.Charts
Imports Microsoft.VisualBasic.FileIO
Imports NCrontab
Imports Newtonsoft.Json.Linq
Imports Npgsql
Imports NpgsqlTypes

Public Class SyncTablesService

#Region "Variables Locales"

    ''' <summary>
    ''' Schedule
    ''' </summary>
    Private _schedule As CrontabSchedule

    ''' <summary>
    ''' WebServices
    ''' </summary>
    Private ReadOnly ws As New WebServiceICMGeneral()

    ''' <summary>
    ''' Log
    ''' </summary>
    Private ReadOnly mLog As Log

    ''' <summary>
    ''' Cadena de Conexión
    ''' </summary>
    Private NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ''' <summary>
    ''' Col Map
    ''' </summary>
    Private Class ColMap
        Public Property PgName As String
        Public Property CsvIndex As Integer
        Public Property PgType As NpgsqlDbType
    End Class

    Private Events As New List(Of String) From {
                "Insert",
                "Actualizar",
                "Update",
                "Suprimir",
                "Delete ",
                "Importación de datos concluida ",
                "Import Concluded"
            }

#End Region

#Region " Métodos Públicos "

    ''' <summary>
    ''' New
    ''' </summary>
    Public Sub New()
        mLog = New Log()
    End Sub

    ''' <summary>
    ''' CheckUpdates
    ''' </summary>
    ''' <param name="Model">Model</param>
    ''' <param name="Tables">Tables</param>
    ''' <param name="Prior">Prior</param>
    ''' <returns>List of tables.</returns>
    Public Function CheckUpdates(Model As String, Tables As List(Of TableMapModel), Prior As Integer) As List(Of TableMapModel)
        Dim currentDate = DateTime.Now
        Try

            If String.IsNullOrEmpty(Model) OrElse Not Regex.IsMatch(Model, "^[a-zA-Z0-9_]+$") Then
                Return New List(Of TableMapModel)()
            End If

            Dim safeTableNames As New List(Of String)
            For Each t In Tables

                Dim schedule As String = If(String.IsNullOrEmpty(t.Schedule), "* * * * *", t.Schedule)
                _schedule = CrontabSchedule.Parse(schedule)

                Dim nextRun = _schedule.GetNextOccurrence(currentDate)
                Dim diffMinutes = (currentDate - nextRun).TotalMinutes

                If diffMinutes >= -1 Or diffMinutes < 10 Then
                    Dim rawName As String = If(t.ICMTableName.StartsWith("Payee_") Or t.ICMTableName.StartsWith("Time_"),
                                 t.ICMTableName.Substring(0, t.ICMTableName.Length - 1),
                                 t.ICMTableName)
                    If Regex.IsMatch(rawName, "^[a-zA-Z0-9_]+$") Then
                        safeTableNames.Add($"'{rawName}'")
                    End If
                End If

            Next

            If safeTableNames.Count = 0 Then Return New List(Of TableMapModel)()

            Dim AuditTable As New List(Of TableMapModel)

            For Each table In Tables
                If table.ICMTableName.ToString() = "_Result374" Then
                    Continue For
                End If

                For Each ev As String In Events
                    Dim LastRow As JArray = ws.ConsultaAudit(table.ICMTableName.ToString(), ev, Model, table.LastUpdateDate.ToString("yyyy-MM-dd")).GetAwaiter().GetResult()
                    If LastRow IsNot Nothing AndAlso LastRow.Any() Then
                        Dim ScopedRow = AuditTable.FirstOrDefault(Function(f) f.ICMTableName = table.ICMTableName.ToString())
                        If ScopedRow Is Nothing Then
                            Dim Row As New TableMapModel With {
                                    .ICMTableName = table.ICMTableName.ToString(),
                                    .LastUpdateAuditID = LastRow(0).Value(Of Integer),
                                    .LastUpdateDate = LastRow(5).Value(Of Date),
                                    .Model = Model,
                                    .PostgreTableName = table.PostgreTableName.ToString(),
                                    .Schedule = table.Schedule.ToString(),
                                    .StagingTableName = table.StagingTableName.ToString()
                            }
                            AuditTable.Add(Row)
                        Else
                            If LastRow(0).Value(Of Integer) > ScopedRow.LastUpdateAuditID Then
                                ScopedRow.LastUpdateAuditID = LastRow(0).Value(Of Integer)
                                ScopedRow.LastUpdateDate = LastRow(5).Value(Of Date)
                            End If
                        End If
                    End If
                Next
            Next

            If Model IsNot Nothing AndAlso Model.StartsWith("femcoep") AndAlso Prior = 2 AndAlso AuditTable.Any() Then
                Dim Row As New TableMapModel With {
                    .ICMTableName = "_Result374",
                    .LastUpdateAuditID = AuditTable.Max(Function(f) f.LastUpdateAuditID),
                    .LastUpdateDate = AuditTable.Max(Function(f) f.LastUpdateDate),
                    .Model = Model,
                    .PostgreTableName = "Result374Varicent",
                    .Schedule = "* * * * *",
                    .StagingTableName = "Result374Staging"
                }
                AuditTable.Add(Row)
            End If

            Return AuditTable
        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesService", "CheckUpdates", "Error", ex.Message)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Download And Update
    ''' </summary>
    ''' <param name="Tables">Tables</param>
    ''' <param name="Prior">Prior</param>
    Public Sub DownloadAndUpdate(Tables As List(Of TableMapModel), Prior As Integer)
        Try
            If Tables Is Nothing OrElse Tables.Count = 0 Then Return
            Dim newGlobalAuditID As Long = Tables.Max(Function(x) x.LastUpdateAuditID)
            Dim allSuccess As Boolean = True
            Dim Rechazadas As New List(Of TableMapModel)

            For Each tableMap In Tables
                Try
                    mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", "Actualizando Lote", "Actualizando " & tableMap.ICMTableName & "...")
                    UpdateStarting(tableMap.ICMTableName)

                    Dim FilePath As String
                    If tableMap.ICMTableName = "_Result374" Then
                        FilePath = ws.GetFullCalcByPublish(tableMap.ICMTableName)
                        If FilePath Is Nothing Then
                            Rechazadas.Add(tableMap)
                            Continue For
                        End If
                    Else
                        FilePath = ws.GetFullTableByPublish(tableMap.ICMTableName, tableMap.Model)
                    End If

                    If Not String.IsNullOrEmpty(FilePath) AndAlso File.Exists(FilePath) Then
                        Dim success As Boolean = SmartBulkInsert(FilePath, tableMap)
                        If success Then
                            UpdateEnding(tableMap)
                        Else
                            allSuccess = False
                            mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", "Error Inserción", "Fallo al insertar " & tableMap.ICMTableName)
                        End If
                    Else
                        allSuccess = False
                        mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", "Descarga Fallida", $"No se pudo descargar el archivo para {tableMap.ICMTableName}")
                    End If

                Catch ex As Exception
                    allSuccess = False
                    mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", $"Error procesando {tableMap.ICMTableName}", ex.Message)
                End Try
            Next

            For Each rechazada In Rechazadas
                Tables.Remove(rechazada)
            Next

            If allSuccess Then
                UpdateBatchAuditID(Prior, newGlobalAuditID, Tables)
                mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", "Lote Exitoso", $"Prioridad {Prior} actualizada con el AuditID {newGlobalAuditID}")
            Else
                mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", "Lote Incompleto", $"No se actualizó el ID de Prioridad {Prior} debido a errores en algunas tablas.")
            End If
        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesService", "DownloadAndUpdate", "Error", ex.Message)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Envía notificación de error en una tabla
    ''' </summary>
    ''' <param name="tabla">Tabla</param>
    ''' <param name="mensajeError">Mensaje de Error.</param>
    Private Sub NotificacionErrorTabla(tabla As String, mensajeError As String)
        Try
            Dim parametros As New Dictionary(Of String, String) From {
                {"@Tabla", tabla},
                {"@MensajeError", mensajeError}
            }

            Dim correo As New PlantillaCorreo()
            correo.Parametros = parametros
            correo.Enviar(1)
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Get PG Schema
    ''' </summary>
    ''' <param name="tableName">Table Name</param>
    ''' <returns>Schema</returns>
    Private Function GetPGSchema(tableName As String) As Dictionary(Of String, NpgsqlDbType)
        Dim cols As New Dictionary(Of String, NpgsqlDbType)
        Using conn As New NpgsqlConnection(NpgsqlConn)
            conn.Open()
            Dim sql As String = "SELECT column_name, udt_name FROM information_schema.columns WHERE table_name = @tbl"
            Using cmd As New NpgsqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("tbl", tableName)
                Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim colName As String = reader("column_name").ToString()
                        Dim udt As String = reader("udt_name").ToString().ToLower()
                        Dim type As NpgsqlDbType
                        Select Case udt
                            Case "int4", "integer" : type = NpgsqlDbType.Integer
                            Case "int8", "bigint" : type = NpgsqlDbType.Bigint
                            Case "numeric", "decimal", "money" : type = NpgsqlDbType.Numeric
                            Case "float8", "double precision" : type = NpgsqlDbType.Double
                            Case "date" : type = NpgsqlDbType.Date
                            Case "timestamp" : type = NpgsqlDbType.Timestamp
                            Case "bool", "boolean" : type = NpgsqlDbType.Boolean
                            Case Else : type = NpgsqlDbType.Text
                        End Select
                        cols(colName) = type
                    End While
                End Using
            End Using
        End Using
        Return cols
    End Function

    ''' <summary>
    ''' Get Publish Filter
    ''' </summary>
    ''' <param name="PostgreTable">Postgre Table</param>
    ''' <param name="Field">Field</param>
    ''' <returns>Task</returns>
    Private Function GetPublishFilter(PostgreTable As String, Field As String) As String
        Dim resultValue As String = String.Empty

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Dim query As String = $"SELECT MAX(""{Field}"") FROM ""{PostgreTable}"""
                Using cmd As New NpgsqlCommand(query, conn)
                    Dim scalarResult As Object = cmd.ExecuteScalar()

                    If scalarResult IsNot Nothing AndAlso Not IsDBNull(scalarResult) Then
                        If TypeOf scalarResult Is DateTime Then

                            resultValue = CType(scalarResult, DateTime).ToString("yyyy-MM-dd")
                        Else
                            resultValue = scalarResult.ToString()
                        End If
                    End If
                End Using
            End Using

        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesService", "GetPublishFilter", "Error al obtener el filtro", $"Error al obtener el filtro para obtener la tabla {PostgreTable}")
            Return String.Empty
        End Try

        Return resultValue
    End Function

    '''<summary>
    '''Get Last Week
    '''</summary>
    '''<returns>String - La fecha de una semana atras en formato yyyy-MM-dd</returns>
    Private Function GetLastWeekString() As String
        Try
            Dim today As DateTime = DateTime.Today
            Dim lastWeek As DateTime = today.AddDays(-7)

            Return lastWeek.ToString("yyyy-MM-dd")
        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesService", "GetLastWeekString", "Error al obtener la fecha para filtrar", $"Error al obtener el Date para filtrar la publicacion: {ex}")
            Return String.Empty
        End Try
    End Function

    ''' <summary>
    ''' Get Sync Tables
    ''' </summary>
    ''' <param name="Priority">Prorities</param>
    ''' <returns>Tables</returns>
    Public Function GetSyncTables(Priority As Integer) As List(Of TableMapModel)
        Try
            Dim Lista As New List(Of TableMapModel)

            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""GetSyncTables""(@p_priority);", conn)
                    cmd.Parameters.AddWithValue("p_priority", Priority)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Lista.Add(New TableMapModel With {
                                .ICMTableName = reader("Table").ToString(),
                                .PostgreTableName = If(IsDBNull(reader("PGName")), reader("Table").ToString(), reader("PGName").ToString()),
                                .StagingTableName = If(IsDBNull(reader("PGStagingName")), reader("Table").ToString(), reader("PGStagingName").ToString()),
                                .LastUpdateAuditID = If(IsDBNull(reader("LastUpdateAuditID")), 0, Convert.ToInt64(reader("LastUpdateAuditID"))),
                                .Model = If(IsDBNull(reader("Model")), "", reader("Model").ToString()),
                                .Schedule = If(IsDBNull(reader("Schedule")), "", reader("Schedule").ToString()),
                                .LastUpdateDate = If(IsDBNull(reader("LastUpdateDate")), DateTime.UtcNow, Convert.ToDateTime(reader("LastUpdateDate")))
                            })
                        End While
                    End Using
                End Using
            End Using
            Return Lista
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Smart Bulk Insert
    ''' </summary>
    ''' <param name="csvFilePath">CSV File</param>
    ''' <param name="mapConfig">Map Config</param>
    ''' <returns>Regresa tareas</returns>
    Public Function SmartBulkInsert(csvFilePath As String, mapConfig As TableMapModel, Optional filter As String = Nothing) As Boolean
        Dim currentLineNumber As Long = 0
        Dim currentFields As String() = Nothing

        Try
            Dim pgSchema = GetPGSchema(mapConfig.StagingTableName)
            If pgSchema.Count = 0 Then
                Throw New Exception($"La tabla {mapConfig.StagingTableName} no existe en PostgreSQL o no tiene columnas")
            End If

            Dim activeMap As New List(Of ColMap)

            Using parser As New TextFieldParser(csvFilePath)
                parser.TextFieldType = FieldType.Delimited
                parser.SetDelimiters(",")
                parser.HasFieldsEnclosedInQuotes = True

                If parser.EndOfData Then Return True

                Dim csvHeaders As String() = parser.ReadFields()
                currentLineNumber = 1

                For Each pgCol In pgSchema
                    Dim index As Integer = Array.FindIndex(csvHeaders, Function(x) x.Equals(pgCol.Key, StringComparison.OrdinalIgnoreCase))

                    If index = -1 Then
                        index = Array.FindIndex(csvHeaders, Function(x) x.Replace("_", "").Equals(pgCol.Key.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                    End If

                    If index >= 0 Then
                        activeMap.Add(New ColMap With {
                        .PgName = pgCol.Key,
                        .PgType = pgCol.Value,
                        .CsvIndex = index
                    })
                    End If
                Next

                If activeMap.Count = 0 Then
                    Throw New Exception("No se encontraron coincidencias de columnas entre el CSV y la Tabla.")
                End If

                Using conn As New NpgsqlConnection(NpgsqlConn)
                    conn.Open()

                    If mapConfig.PostgreTableName <> "ReplicaICM_EP" Then
                        Using cmd As New NpgsqlCommand($"TRUNCATE TABLE ""{mapConfig.StagingTableName}"";", conn)
                            cmd.ExecuteNonQuery()
                        End Using
                    Else
                        If String.IsNullOrEmpty(filter) Then
                            Throw New Exception($"El parametro 'filter' es requerido para la ejecucion del BulkInsert para {mapConfig.PostgreTableName}")
                        End If
                        Using cmd As New NpgsqlCommand($"DELETE FROM ""{mapConfig.PostgreTableName}"" WHERE ""DATUM"" >= @filter;", conn)
                            cmd.Parameters.AddWithValue("@filter", NpgsqlTypes.NpgsqlDbType.Date, DateTime.ParseExact(filter, "yyyy-MM-dd", Nothing))
                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    Dim colList As String = String.Join(", ", activeMap.Select(Function(m) $"""{m.PgName}"""))
                    Dim copyCmd As String = $"COPY ""{mapConfig.StagingTableName}"" ({colList}) FROM STDIN (FORMAT BINARY)"

                    Using importer As NpgsqlBinaryImporter = conn.BeginBinaryImport(copyCmd)
                        While Not parser.EndOfData
                            currentLineNumber += 1
                            Dim fields As String() = parser.ReadFields()
                            currentFields = fields

                            If fields Is Nothing OrElse fields.Length = 0 OrElse (fields.Length = 1 AndAlso String.IsNullOrWhiteSpace(fields(0))) Then
                                Continue While
                            End If

                            Dim hasData As Boolean = False
                            For i As Integer = 0 To fields.Length - 1
                                If Not String.IsNullOrWhiteSpace(fields(i)) Then
                                    hasData = True
                                    Exit For
                                End If
                            Next
                            If Not hasData Then Continue While

                            importer.StartRow()

                            For Each map In activeMap
                                Dim val As Object = DBNull.Value

                                If map.CsvIndex < fields.Length Then
                                    Dim rawVal As String = fields(map.CsvIndex)

                                    If Not String.IsNullOrWhiteSpace(rawVal) Then
                                        Try
                                            Select Case map.PgType
                                                Case NpgsqlDbType.Date, NpgsqlDbType.Timestamp, NpgsqlDbType.TimestampTZ
                                                    Dim parsedDate As DateTime
                                                    Dim formats As String() = {"M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss"}
                                                    If DateTime.TryParseExact(rawVal, formats, System.Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, parsedDate) Then
                                                        val = parsedDate
                                                    Else
                                                        val = DateTime.Parse(rawVal, System.Globalization.CultureInfo.InvariantCulture)
                                                    End If

                                                Case NpgsqlDbType.Integer, NpgsqlDbType.Bigint, NpgsqlDbType.Smallint
                                                    Dim dotIndex As Integer = rawVal.IndexOf("."c)
                                                    If dotIndex >= 0 Then
                                                        val = Convert.ToInt64(rawVal.Substring(0, dotIndex))
                                                    Else
                                                        val = Convert.ToInt64(rawVal)
                                                    End If

                                                Case NpgsqlDbType.Numeric, NpgsqlDbType.Double, NpgsqlDbType.Money
                                                    val = Convert.ToDecimal(rawVal, System.Globalization.CultureInfo.InvariantCulture)

                                                Case NpgsqlDbType.Boolean, NpgsqlDbType.Bit
                                                    Dim s As String = rawVal.ToLower().Trim()
                                                    If s = "1" OrElse s = "t" OrElse s = "true" Then
                                                        val = True
                                                    ElseIf s = "0" OrElse s = "f" OrElse s = "false" Then
                                                        val = False
                                                    Else
                                                        val = Boolean.Parse(rawVal)
                                                    End If

                                                Case Else
                                                    val = rawVal
                                            End Select
                                        Catch
                                            val = DBNull.Value
                                        End Try
                                    End If
                                End If

                                Try
                                    importer.Write(val, map.PgType)
                                Catch ex As Exception
                                    Throw New Exception($"Error columna {map.PgName}: {ex.Message}")
                                End Try
                            Next
                        End While
                        importer.Complete()
                    End Using
                End Using
            End Using

            Return True

        Catch ex As Exception
            Dim lineData As String = If(currentFields IsNot Nothing, String.Join("|", currentFields), "N/A")
            Dim errorDetail As String = $"Línea CSV: {currentLineNumber} | Datos: {lineData} | Error: {ex.Message}"

            mLog.InsertApplicationLog("SyncTablesService", "SmartBulkInsert", $"Error en {mapConfig.StagingTableName}", errorDetail)
            Throw
        Finally
            If File.Exists(csvFilePath) Then
                Try : File.Delete(csvFilePath) : Catch : End Try
            End If
        End Try
    End Function

    ''' <summary>
    ''' Special Download And Update
    ''' </summary>
    ''' <param name="Tables">Tables</param>
    ''' <param name="Prior">Prior</param>
    Public Sub SpecialDownloadAndUpdate(Tables As List(Of TableMapModel), Prior As Integer)
        If Tables Is Nothing OrElse Tables.Count = 0 Then Return
        Dim newGlobalAuditID As Long = Tables.Max(Function(x) x.LastUpdateAuditID)
        Dim allSuccess As Boolean = True

        For Each tableMap In Tables
            Dim ICMTableName As String = tableMap.ICMTableName
            Dim tableSuccess As Boolean = True
            Dim mensajeError As String = String.Empty
            Try
                mLog.InsertApplicationLog("SyncTablesService", "SpecialDownloadAndUpdate", "Actualizando Lote", "Actualizando " & ICMTableName & "...")
                UpdateStarting(ICMTableName)

                Dim Filter As String = GetLastWeekString()
                If String.IsNullOrEmpty(Filter) Then Filter = "1900-01-01"
                Dim FilePath As String = ws.GetFullTableByPublishWFilter(ICMTableName, "DATUM", Filter)
                If Not String.IsNullOrEmpty(FilePath) AndAlso File.Exists(FilePath) Then
                    Dim success As Boolean = SmartBulkInsert(FilePath, tableMap, Filter)
                    If success Then
                        UpdateEnding(tableMap)
                    Else
                        mensajeError = "Fallo al insertar"
                        tableSuccess = False
                        mLog.InsertApplicationLog("SyncTablesService", "SpecialDownloadAndUpdate", "Error Inserción", "Fallo al insertar " & ICMTableName)
                    End If
                Else
                    tableSuccess = False
                    mensajeError = "Descarga Faillida"
                    mLog.InsertApplicationLog("SyncTablesService", "SpecialDownloadAndUpdate", "Descarga Fallida", $"No se pudo descargar el archivo para {ICMTableName}")
                End If
            Catch ex As Exception
                tableSuccess = False
                mensajeError = ex.Message
                mLog.InsertApplicationLog("SyncTablesService", "SpecialDownloadAndUpdate", $"Error procesando {ICMTableName}", ex.Message)
            Finally
                If (Not tableSuccess) Then
                    allSuccess = False
                    NotificacionErrorTabla(ICMTableName, mensajeError)
                End If
            End Try
        Next

        If allSuccess Then
            UpdateBatchAuditID(Prior, newGlobalAuditID)
            mLog.InsertApplicationLog("SyncTablesService", "SpecialDownloadAndUpdate", "Lote Exitoso", $"Prioridad {Prior} actualizada con el AuditID {newGlobalAuditID}")
        Else
            mLog.InsertApplicationLog("SyncTablesService", "SpecialDownloadAndUpdate", "Lote Incompleto", $"No se actualizó el ID de Prioridad {Prior} debido a errores en algunas tablas.")
        End If
    End Sub

    ''' <summary>
    ''' Update Batch Audit ID
    ''' </summary>
    ''' <param name="Priority">Priority</param>
    ''' <param name="newAuditID">New Audit ID</param>
    Private Sub UpdateBatchAuditID(Priority As Integer, newAuditID As Long)
        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("UPDATE ""SyncTables"" SET ""LastUpdateAuditID"" = @id, ""LastUpdateDate"" = NOW() WHERE ""Priority"" = @prior", conn)
                    cmd.Parameters.AddWithValue("id", newAuditID)
                    cmd.Parameters.AddWithValue("prior", Priority)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesService", "AuditUpdate", "Error al actualizar el AuditID", $"Error al actualizar AuditID de Prioridad {Priority}")
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Update Batch Audit ID
    ''' </summary>
    ''' <param name="Priority">Priority</param>
    ''' <param name="newAuditID">New Audit ID</param>
    ''' <param name="Tables">Tables</param>
    Private Sub UpdateBatchAuditID(Priority As Integer, newAuditID As Long, Tables As List(Of TableMapModel))
        Try
            Dim TablasF As String = "(" & String.Join(",", Tables.Select(Function(x) "'" & x.ICMTableName & "'")) & ")"

            newAuditID = If(TablasF = "('_Result374')", newAuditID - 1, newAuditID)

            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand($"UPDATE ""SyncTables"" SET ""LastUpdateAuditID"" = @id, ""LastUpdateDate"" = NOW() WHERE ""Priority"" = @prior AND ""Table"" IN {TablasF}", conn)
                    cmd.Parameters.AddWithValue("id", newAuditID)
                    cmd.Parameters.AddWithValue("prior", Priority)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesService", "AuditUpdate", "Error al actualizar el AuditID", $"Error al actualizar AuditID de Prioridad {Priority}")
            Throw
        End Try
    End Sub

#End Region

#Region " Métodos Privados "

    ''' <summary>
    ''' Valida si hay un evento global en ejecución consultando /globalactionstatus.
    ''' Devuelve True si el servicio indica que hay evento en progreso.
    ''' </summary>
    ''' <param name="model">Nombre del modelo a revisar.</param>
    ''' <returns>True si hay un evento global en ejecución; False en caso contrario.</returns>
    Private Function IsGlobalActionStatusRunning(model As String) As Boolean
        Try
            Dim httpClient As HttpClient = New HttpClient()
            Dim service As New ICMService(httpClient)
            Dim value As Boolean = service.GlobalActionStatus(model).GetAwaiter().GetResult()
            Return value
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Metodo que finaliza la actualización de una tabla
    ''' </summary>
    ''' <param name="table">Tabla</param>
    Private Sub UpdateEnding(table As TableMapModel)
        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Dim sql As String = "CALL ""SyncTables_Ending""(@p_table, @p_staging);"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("p_table", table.PostgreTableName)
                    cmd.Parameters.AddWithValue("p_staging", table.StagingTableName)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Metodo que inicia la actualización de una tabla
    ''' </summary>
    ''' <param name="table">Tabla</param>
    ''' <returns>Regresa true si puede realizar la actualización, false en caso de que exista otro proceso actualizando la tabla</returns>
    Private Function UpdateStarting(table As String) As Boolean
        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Dim sql As String = "SELECT * FROM ""SyncTables_Starting""(@p_table);"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("p_table", table)
                    Dim updatingObject As Object = cmd.ExecuteScalar()
                    Dim updatingBoolean As Boolean = If(Boolean.TryParse(updatingObject.ToString(), updatingBoolean), updatingBoolean, False)
                    Return updatingBoolean
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

#End Region

End Class