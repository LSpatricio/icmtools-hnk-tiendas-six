Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Imports Serilog

Public Class EficienciaController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _eficienciaService As EficienciaService

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _configuration = New AppConfiguration()
        _eficienciaService = New EficienciaService()
    End Sub

    ' ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/eficiencia/cargarinfo")>
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


            logger.Information("Iniciando proceso de validaciones y carga de información para Eficiencia")

            Dim cargaResponse = Await _eficienciaService.ProcesarEficiencia(request, idCarga, logger)

            logger.Information("Fin proceso de validaciones y carga de información para Eficiencia")

            If cargaResponse.Errores.Any() Then

                logger.Warning(
                    "Se encontraron {CantidadErrores} errores de validación.",
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
            "Error al validar/cargar información del archivo."
        )
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/eficiencia/enviarinformacion")>
    Public Async Function EnvioEficiencia(<FromBody> request As SendInfoRequest) As Task(Of IHttpActionResult)
        Dim logger = Log _
                .ForContext("Pantalla", request.Screen) _
                .ForContext("Usuario", mUser.Email) _
                .ForContext("Periodo", request.Period) _
                .ForContext("Proceso", LoggerConfig.Proceso.EnviarInformacion.ToString()) _
                .ForContext("IdCarga", request.IdGui)
        Try
            Thread.Sleep(1000)
            logger.Information("Inicio proceso envio de información")
            Await _eficienciaService.EnvioEficiencia(request, logger)
            logger.Information("Fin proceso envio de información")

            Return Ok(New With {.d = True})
        Catch ex As Exception
            logger.Error(
            ex,
            "Error al enviar información."
        )
            Return InternalServerError(ex)
        End Try
    End Function

End Class


