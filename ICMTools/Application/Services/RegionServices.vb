Public Class RegionServices

    Private ReadOnly _icmApiClient As IcmApiClient

    Public Sub New()
        _icmApiClient = New IcmApiClient()
    End Sub

    Public Async Function ObtenerRegiones(model As String) As Threading.Tasks.Task(Of List(Of Region))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDRegion"", ""Description"" FROM ""CatRegionSix"" ORDER BY ""Description"" ASC",
            .Offset = 0,
            .Limit = 100
        }, model)

        Return IcmQueryMapper.MapResponse(Of Region)(query)

    End Function

End Class
