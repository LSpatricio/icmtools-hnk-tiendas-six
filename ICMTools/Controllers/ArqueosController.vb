Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports System.Linq
Imports Serilog

Public Class ArqueosController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _arqueosService As ArqueosService

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _arqueosService = New ArqueosService()
    End Sub

    ReadOnly sc As New SharedController

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
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing

            logger.Information("Iniciando proceso de validaciones y carga de informacion para Arqueos")

            Dim cargaResponse = Await _arqueosService.ProcesarArqueos(request, idCarga, logger)

            logger.Information("Fin proceso de validaciones y carga de informacion para Arqueos")

            If cargaResponse.Errores.Any() Then

                logger.Warning(
                    "Se encontraron {CantidadErrores} errores de validacion.",
                    cargaResponse.Errores.Count
                )

                For Each errores In cargaResponse.Errores
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

            End If

            Return Ok(New With {.d = cargaResponse.Exitoso, .id = cargaResponse.IdCarga})

        Catch ex As Exception
            logger.Error(
                ex,
                "Error al validar/cargar informacion del archivo de Arqueos."
            )
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
            Thread.Sleep(1000)

            logger.Information("Inicio proceso envio de informacion de Arqueos")

            Await _arqueosService.EnvioArqueos(request, logger)

            logger.Information("Fin proceso envio de informacion de Arqueos")

            Return Ok(New With {.d = True})

        Catch ex As Exception
            logger.Error(
                ex,
                "Error al enviar informacion de Arqueos."
            )
            Return InternalServerError(ex)
        End Try
    End Function

End Class
