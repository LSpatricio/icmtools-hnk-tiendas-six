Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading.Tasks
Imports System.Web.ApplicationServices
Imports DocumentFormat.OpenXml.Drawing
Imports Newtonsoft.Json

Public Class IcmApiClient

    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _httpClient As HttpClient

    Public Sub New()
        _configuration = New AppConfiguration()
        _httpClient = New HttpClient()
        _httpClient.DefaultRequestHeaders.Clear()

        _httpClient.DefaultRequestHeaders.Accept.Clear()
        _httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
    End Sub

    Public Async Function Query(
      payload As IcmQueryRequestDto,
      modelo As String
  ) As Task(Of IcmQueryResponseDto)

        Try

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12

            Dim requestUrl As String =
                $"{_configuration.UrlBase}/rpc/querytool"

            Dim request As HttpRequestMessage =
                New HttpRequestMessage(
                    HttpMethod.Post,
                    requestUrl) With {
                        .Content = New StringContent(
                            JsonConvert.SerializeObject(payload),
                            Encoding.UTF8,
                            "application/json")
                    }

            request.Headers.Add("Model", modelo)

            request.Headers.Authorization =
                New AuthenticationHeaderValue(
                    "Bearer",
                    _configuration.BearerToken)

            Using request

                Using response As HttpResponseMessage =
                    Await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead
                    ).ConfigureAwait(False)

                    response.EnsureSuccessStatusCode()

                    Using stream As Stream =
                        Await response.Content.ReadAsStreamAsync().
                        ConfigureAwait(False)

                        Using streamReader As New StreamReader(stream)

                            Using jsonReader As New JsonTextReader(streamReader)

                                Dim serializer As New JsonSerializer()

                                Return serializer.Deserialize(
                                    Of IcmQueryResponseDto
                                )(jsonReader)

                            End Using
                        End Using
                    End Using

                End Using
            End Using

        Catch ex As HttpRequestException
            Throw New ApplicationException(
                "No se pudo realizar la consulta a ICM.",
                ex)

        Catch ex As JsonException
            Throw New ApplicationException(
                "La respuesta de ICM no tiene el formato esperado.",
                ex)

        End Try

    End Function
End Class
