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

    Public Async Function ObtenerTipoMovimientoSix(model As String) As Threading.Tasks.Task(Of List(Of TipoMovimientoSixDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDTypeMovement"", ""Description"" FROM ""CatMovementTypeSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of TipoMovimientoSixDto)(query)

    End Function

    Public Async Function ObtenerTipoGarantia(model As String) As Threading.Tasks.Task(Of List(Of TipoGarantiaSixDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDTypeGuarantee"", ""Description"" FROM ""CatGuaranteeTypeSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of TipoGarantiaSixDto)(query)

    End Function

    Public Async Function ObtenerEstatusSKSix(model As String) As Threading.Tasks.Task(Of List(Of EstatusSKSixDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDStatusSK"", ""Description"" FROM ""CatStatusSKSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of EstatusSKSixDto)(query)

    End Function
    Public Async Function ObtenerCategoriaSix(model As String) As Threading.Tasks.Task(Of List(Of CategoriaSixDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDCategory"", ""Description"" FROM ""CatCategorySix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of CategoriaSixDto)(query)

    End Function
    Public Async Function ObtenerCeBeSix(model As String) As Threading.Tasks.Task(Of List(Of CeBeSIxDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDCeBe"", ""Description"" FROM ""CatCeBeSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of CeBeSIxDto)(query)

    End Function
    Public Async Function ObtenerProductSix(model As String) As Threading.Tasks.Task(Of List(Of ProductSixDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDProduct"", ""Description"" FROM ""CatProductSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of ProductSixDto)(query)

    End Function
    Public Async Function ObtenerTipoRenta(model As String) As Threading.Tasks.Task(Of List(Of TipoRentaDto))

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDRentType"", ""Description"" FROM ""CatRentTypeSix""",
            .Offset = 0,
            .Limit = 1000
        }, model)

        Return IcmQueryMapper.MapResponse(Of TipoRentaDto)(query)

    End Function

End Class
