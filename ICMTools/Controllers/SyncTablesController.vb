Imports System.Web.Http

Public Class SyncTablesController
    Inherits ApiController

#Region " Propiedades Privadas "

    ''' <summary>
    ''' Log
    ''' </summary>
    Private mLog As Log

    ''' <summary>
    ''' Scheduler
    ''' </summary>
    Private _scheduler As JobSchedulerService

    ''' <summary>
    ''' SynctablesService
    ''' </summary>
    Private ReadOnly _syncTablesService As SyncTablesService

    ''' <summary>
    ''' Servicio de Peticiones a PostgreSQL
    ''' </summary>
    Private ReadOnly _PGService As PostgreService
#End Region

#Region " Constructor "

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        Try
            mLog = New Log
            _scheduler = New JobSchedulerService
            _syncTablesService = New SyncTablesService
            _PGService = New PostgreService
        Catch ex As Exception
            mLog.InsertApplicationLog("SyncTablesController", "New", "Error", ex.Message)
        End Try
    End Sub

#End Region

#Region "Clases"
    Public Class ResetRequest
        Property User As String
        Property PersonnelDivision As String
    End Class
#End Region

#Region " Métodos Públicos "

    ''' <summary>
    ''' Index
    ''' </summary>
    ''' <param name="p">Prioridad</param>
    ''' <returns>Respuesta de la Api</returns>
    <HttpGet>
    <Route("api/synctables")>
    Public Function Index(Optional p As String = "0") As IHttpActionResult
        Try
            Dim priority As Integer = If(Integer.TryParse(p, priority), priority, 0)
            If (priority.Equals(0)) Then
                Jobs()
            ElseIf (priority.Equals(1)) Then
                Prioridad1()
            ElseIf (priority.Equals(2)) Then
                Prioridad2()
            ElseIf (priority.Equals(3)) Then
                Prioridad3()
            ElseIf (priority.Equals(4)) Then
                Prioridad4()
            End If

            Return Ok()
        Catch ex As Exception
            NotificacionError(ex.Message)
            mLog.InsertApplicationLog("SyncTablesController", "Index", "Error", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Reinicia el contador de intentos manualmente
    ''' </summary>
    <HttpPost>
    <Route("api/resetCount")>
    Public Function ResetCount(<FromBody> request As ResetRequest) As IHttpActionResult
        Try
            Dim safeUsr As String = SanitizeRequestString(request.User)
            Dim safeDivPer As String = SanitizeRequestString(request.PersonnelDivision)

            mLog.InsertApplicationLog("ResetCount", "Reset Request", "Inicio", "Iniciando ejecución")
            _PGService.ActionTryCount(safeUsr, safeDivPer, "RESET")
            Return Ok()
        Catch ex As Exception
            NotificacionError(ex.Message)
            mLog.InsertApplicationLog("CountReset", "Peticion Api", "Error", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpGet>
    <Route("api/resetAll")>
    Public Function ResetAll() As IHttpActionResult
        Try
            mLog.InsertApplicationLog("Reset All", "Reset Request", "Inicio", "Iniciando ejecución")
            _PGService.ActionResetAll()
            Return Ok()
        Catch ex As Exception
            NotificacionError(ex.Message)
            mLog.InsertApplicationLog("ResetAll", "Peticion API", "Error", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function


#End Region

#Region " Métodos Privados "

    ''' <summary>
    ''' Jobs
    ''' </summary>
    Private Sub Jobs()
        Try
            mLog.InsertApplicationLog("SyncTablesController", "Jobs", "Inicio", "Iniciando ejecución")
            _scheduler.CheckAndExcecuteJobs()
            mLog.InsertApplicationLog("SyncTablesController", "Jobs", "Fin", "Finalizando ejecución")
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Envía notificación de error en una tabla
    ''' </summary>
    ''' <param name="mensajeError">Mensaje de Error.</param>
    Private Sub NotificacionError(mensajeError As String)
        Try
            Dim parametros As New Dictionary(Of String, String) From {
                {"@MensajeError", mensajeError}
            }

            Dim correo As New PlantillaCorreo()
            correo.Parametros = parametros
            correo.Enviar(2)
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Prioridad 1
    ''' </summary>
    Private Sub Prioridad1()
        Try
            mLog.InsertApplicationLog("SyncTablesController", "Prioridad1", "Inicio", "Iniciando ejecución")
            Dim SyncS As List(Of TableMapModel) = _syncTablesService.GetSyncTables(1)

            If SyncS IsNot Nothing AndAlso SyncS.Count > 0 Then
                Dim uniqueModels = SyncS _
                .Select(Function(x) SanitizeModelName(x.Model)) _
                .Where(Function(m) Not String.IsNullOrWhiteSpace(m) AndAlso IsValidModelName(m)) _
                .Distinct() _
                .ToList()

                For Each model As String In uniqueModels
                    Dim SpecialT = SyncS.Where(Function(x) SanitizeModelName(x.Model) = model).ToList()
                    Dim outdated = _syncTablesService.CheckUpdates(model, SpecialT, 1)

                    If outdated.Count >= 1 Then
                        _syncTablesService.SpecialDownloadAndUpdate(outdated, 1)
                    End If
                Next
            End If

            mLog.InsertApplicationLog("SyncTablesController", "Prioridad1", "Fin", "Finalizando ejecución")
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Prioridad 2
    ''' </summary>
    Private Sub Prioridad2()
        Try
            mLog.InsertApplicationLog("SyncTablesController", "Prioridad2", "Inicio", "Iniciando ejecución")
            Dim SyncP As List(Of TableMapModel) = _syncTablesService.GetSyncTables(2)

            If SyncP IsNot Nothing AndAlso SyncP.Count > 0 Then
                Dim uniqueModels = SyncP _
                .Select(Function(x) SanitizeModelName(x.Model)) _
                .Where(Function(m) Not String.IsNullOrWhiteSpace(m) AndAlso IsValidModelName(m)) _
                .Distinct() _
                .ToList()

                For Each model As String In uniqueModels
                    Dim tablesHigh = SyncP.Where(Function(x) SanitizeModelName(x.Model) = model).ToList()
                    Dim outdated = _syncTablesService.CheckUpdates(model, tablesHigh, 2)

                    If outdated.Count >= 1 Then
                        _syncTablesService.DownloadAndUpdate(outdated, 2)
                    End If
                Next
            End If

            mLog.InsertApplicationLog("SyncTablesController", "Prioridad2", "Fin", "Finalizando ejecución")
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Ejecuta Prioridad 3
    ''' </summary>
    Private Sub Prioridad3()
        Try
            mLog.InsertApplicationLog("SyncTablesController", "Prioridad3", "Inicio", "Iniciando ejecución")

            Dim SyncLow As List(Of TableMapModel) = _syncTablesService.GetSyncTables(3)

            If SyncLow IsNot Nothing AndAlso SyncLow.Count > 0 Then
                Dim uniqueModels = SyncLow _
                .Select(Function(x) SanitizeModelName(x.Model)) _
                .Where(Function(m) Not String.IsNullOrWhiteSpace(m) AndAlso IsValidModelName(m)) _
                .Distinct() _
                .ToList()

                For Each model As String In uniqueModels
                    Dim tablesLow = SyncLow.Where(Function(x) SanitizeModelName(x.Model) = model).ToList()
                    Dim outdated = _syncTablesService.CheckUpdates(model, tablesLow, 3)

                    If outdated.Count >= 1 Then
                        _syncTablesService.DownloadAndUpdate(outdated, 3)
                    End If
                Next
            End If

            mLog.InsertApplicationLog("SyncTablesController", "Prioridad3", "Fin", "Finalizando ejecución")
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Ejecuta Envio de Logs de Excepciones Por Mail
    ''' </summary>
    Private Sub Prioridad4()
        Try
            mLog.InsertApplicationLog("Envio de Logs por correo", "Prioridad4", "Inicio", "Iniciando ejecución")
            _scheduler.SendLogByMail()
        Catch ex As Exception
            mLog.InsertApplicationLog("Envio de Logs por correo", "Prioridad4", "Error", "Error en el envio de Logs por correo " & ex.ToString() & ".")
        End Try
    End Sub

    Private Function IsValidModelName(value As String) As Boolean
        Return System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-zA-Z0-9_]{1,100}$")
    End Function

    Private Function SanitizeModelName(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return String.Empty
        Return System.Text.RegularExpressions.Regex.Replace(value.Trim(), "[^a-zA-Z0-9_\-]", String.Empty)
    End Function

    Private Function SanitizeRequestString(Value As String) As String
        If String.IsNullOrWhiteSpace(Value) Then Return ""
        Return Regex.Replace(Value, "[^a-zA-Z0-9\s\-_@.]", "")
    End Function

#End Region

End Class
