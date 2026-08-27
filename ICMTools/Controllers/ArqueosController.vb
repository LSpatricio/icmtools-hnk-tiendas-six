Imports System.Threading.Tasks
Imports System.Web.Http
Imports System.Linq
Imports Serilog

Public Class ArqueosController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _arqueosService As ArqueosService

    Public Sub New()
        mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _arqueosService = New ArqueosService()
    End Sub

    Private ReadOnly sc As New SharedController()

    <HttpPost>
    <Route("api/arqueos/cargarinfo")>
    Public Async Function CargarInfoAsync(<FromBody> request As ValidateFileRequest) As Task(Of IHttpActionResult)
        Dim idCarga As Guid = Guid.NewGuid()
        Dim logger = Log _
            .ForContext("Pantalla", request.Screen) _
            .ForContext("Usuario", mUser.Email) _
            .ForContext("Periodo", request.Period) _
            .ForContext("Proceso", LoggerConfig.Proceso.CargarInformacion.ToString()) _
            .ForContext("IdCarga", idCarga)

        Try
            logger.Information("Inicio de validacion y carga de Arqueos")
            Dim cargaResponse = Await _arqueosService.ProcesarArqueos(request, idCarga, logger)

            If cargaResponse.Errores.Any() Then
                Dim filasErrores = String.Join("", cargaResponse.Errores.Select(
                    Function(errorValidacion) $"<tr><td>{errorValidacion.Problema}</td><td>{errorValidacion.Detalle}</td></tr>"))
                Return Ok(New With {.d = sc.TableBuilder(filasErrores, 1)})
            End If

            logger.Information("Validacion y carga de Arqueos finalizada")
            Return Ok(New With {.d = cargaResponse.Exitoso, .id = cargaResponse.IdCarga})
        Catch ex As Exception
            logger.Error(ex, "Error al validar o cargar la informacion de Arqueos")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/arqueos/enviarinformacion")>
    Public Async Function EnvioArqueos(<FromBody> request As SendInfoRequest) As Task(Of IHttpActionResult)
        Dim logger = Log _
            .ForContext("Pantalla", request.Screen) _
            .ForContext("Usuario", mUser.Email) _
            .ForContext("Periodo", request.Period) _
            .ForContext("Proceso", LoggerConfig.Proceso.EnviarInformacion.ToString()) _
            .ForContext("IdCarga", request.IdGui)

        Try
            logger.Information("Inicio de envio de Arqueos")
            Await _arqueosService.EnvioArqueos(request, logger)
            logger.Information("Envio de Arqueos finalizado")
            Return Ok(New With {.d = True})
        Catch ex As Exception
            logger.Error(ex, "Error al generar o enviar el archivo de Arqueos")
            Return InternalServerError(ex)
        End Try
    End Function

End Class
