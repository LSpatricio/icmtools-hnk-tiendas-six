Imports Newtonsoft.Json

Public Class IcmQueryRequestDto

    <JsonProperty("queryString")>
    Public Property QueryString As String

    <JsonProperty("offset")>
    Public Property Offset As Integer

    <JsonProperty("limit")>
    Public Property Limit As Integer

End Class