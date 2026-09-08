Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Serilog

Public Class RenegociacionesController
    Inherits ApiController

    Private ReadOnly mUser As User
    Private ReadOnly _service As RenegociacionesService
    Private ReadOnly _sharedController As New SharedController

    Public Sub New()
        mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _service = New RenegociacionesService()
    End Sub

    <HttpPost>
    <Route("api/renegociaciones/cargarinfo")>
    Public Async Function CargarInfoAsync(<FromBody> request As ValidateFileRequest) As Task(Of IHttpActionResult)
        Dim idCarga = Guid.NewGuid()
        Dim logger = Log.ForContext("Pantalla", request.Screen).ForContext("Usuario", mUser.Email).ForContext("Periodo", request.Period).ForContext("Proceso", LoggerConfig.Proceso.CargarInformacion.ToString()).ForContext("IdCarga", idCarga)
        Try
            Thread.Sleep(1000)
            Dim response = Await _service.ProcesarRenegociaciones(request, idCarga, logger)
            If response.Errores.Any() Then
                Dim errorsList As String = Nothing
                For Each errorItem In response.Errores
                    errorsList += $"<tr><td>{errorItem.Problema}</td><td>" & String.Join(", ", errorItem.Detalle) & "</td></tr>"
                Next
                Return Ok(New With {.d = _sharedController.TableBuilder(errorsList, 1)})
            End If
            Return Ok(New With {.d = response.Exitoso, .id = response.IdCarga})
        Catch ex As Exception
            logger.Error(ex, "Error al validar/cargar renegociaciones")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/renegociaciones/enviarinformacion")>
    Public Async Function EnvioRenegociaciones(<FromBody> request As SendInfoRequest) As Task(Of IHttpActionResult)
        Dim logger = Log.ForContext("Pantalla", request.Screen).ForContext("Usuario", mUser.Email).ForContext("Periodo", request.Period).ForContext("Proceso", LoggerConfig.Proceso.EnviarInformacion.ToString()).ForContext("IdCarga", request.IdGui)
        Try
            Thread.Sleep(1000)
            Await _service.EnvioRenegociaciones(request, logger)
            Return Ok(New With {.d = True})
        Catch ex As Exception
            logger.Error(ex, "Error al enviar renegociaciones")
            Return InternalServerError(ex)
        End Try
    End Function
End Class
