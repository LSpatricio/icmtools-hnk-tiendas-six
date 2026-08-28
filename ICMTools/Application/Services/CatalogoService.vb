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
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of RegionDto)(query)

    End Function


    Public Async Function ObtenerGZSix(model As String) As Threading.Tasks.Task(Of List(Of GZSixDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDGZ"", ""Description"" FROM ""CatGZSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of GZSixDto)(query)

    End Function

    Public Async Function ObtenerEstatusTienda(model As String) As Threading.Tasks.Task(Of List(Of EstatusTiendaDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDStoreStatus"", ""Description"" FROM ""CatStoreStatusSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of EstatusTiendaDto)(query)

    End Function


End Class
