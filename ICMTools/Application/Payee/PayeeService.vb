Imports DocumentFormat.OpenXml.Drawing.Charts

Public Class PayeeService

    Private ReadOnly _icmApiClient As IcmApiClient

    Public Sub New()
        _icmApiClient = New IcmApiClient()
    End Sub

    Public Async Function ValidarPayeePorCorreoModelo(userEmail As String, model As String) As Threading.Tasks.Task(Of Boolean)

        Dim query As IcmQueryResponseDto = Await _icmApiClient.Query(New IcmQueryRequestDto With {
            .QueryString = $"SELECT COUNT(*) FROM ""Payee_"" WHERE UPPER(""Email_"") = UPPER('{userEmail}')",
            .Offset = 0,
            .Limit = 10000
        }, model)

        Dim total As Integer = Convert.ToInt32(query.Data(0)(0))

        Return total > 0
    End Function




End Class
