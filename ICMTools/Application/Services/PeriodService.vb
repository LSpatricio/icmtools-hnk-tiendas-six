Public Class PeriodService

    Private ReadOnly _icmApiClient As IcmApiClient

    Public Sub New()
        _icmApiClient = New IcmApiClient()
    End Sub

    Public Async Function ObtenerPeriodoActual(model As String) As Threading.Tasks.Task(Of PeriodsDto)

        Dim year = Date.Now.Year.ToString()
        Dim mes As String = Today.ToString("MM")
        Dim fechaICM As String = $"{year}, MONTH {mes}"

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT ""IDPeriodString"", ""StartDate"", ""EndDate"" FROM ""CfgDateStringPeriods"" WHERE UPPER(""IDPeriodString"") = UPPER('{fechaICM}')",
            .Offset = 0,
            .Limit = 1
        }, model)

        Return IcmQueryMapper.MapResponse(Of PeriodsDto)(query).FirstOrDefault

    End Function



End Class
