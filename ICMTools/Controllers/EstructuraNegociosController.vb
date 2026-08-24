Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class EstructuraNegociosController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _estructuraNegociosServices As EstructuraNegociosServices


    ' Private mLog As Log

    ' Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _estructuraNegociosServices = New EstructuraNegociosServices()

        '     Me.mLog = New Log
    End Sub

    ' ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/estructuranegocios/cargarinfo")>
    Public Async Function CargarInfoAsync(<FromBody> request As ValidateFileRequest) As Task(Of IHttpActionResult)
        Try
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing

            Dim cargaResponse = Await _estructuraNegociosServices.ProcesarEstructuraNegocios(request)

            If cargaResponse.Errores.Any() Then
                For Each errores In cargaResponse.Errores
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

            End If

            Return Ok(New With {.d = cargaResponse.Exitoso, .id = cargaResponse.IdCarga})
        Catch ex As Exception
            'mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/estructuranegocios/enviarinformacion")>
    Public Async Function EnvioEstructuraNegocios(<FromBody> request As SendInfoRequest) As Task(Of IHttpActionResult)
        Try
            Thread.Sleep(1000)

            Await _estructuraNegociosServices.EnvioEstructuraNegocios(request)

            Return Ok(New With {.d = True})
        Catch ex As Exception
            'mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

End Class


