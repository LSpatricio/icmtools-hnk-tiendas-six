Imports System.Threading.Tasks

Public Class CatalogoService

    Private ReadOnly _icmApiClient As IcmApiClient

    Public Sub New()
        _icmApiClient = New IcmApiClient()
    End Sub

    Public Async Function ObtenerRegiones(model As String) As Threading.Tasks.Task(Of List(Of RegionDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDRegion"", ""Description"" FROM ""CatRegionSix"" ORDER BY ""Description"" ASC",
            .Offset = 0,
            .Limit = 100
        }, model)

        Return IcmQueryMapper.MapResponse(Of RegionDto)(query)

    End Function

    Public Async Function ObtenerCatalogoRegiones(model As String) As Task(Of HashSet(Of String))

        Dim regiones = Await ObtenerRegiones(model)

        Return New HashSet(Of String)(
            regiones.Select(Function(r) r.Description),
            StringComparer.OrdinalIgnoreCase
        )

    End Function



End Class
