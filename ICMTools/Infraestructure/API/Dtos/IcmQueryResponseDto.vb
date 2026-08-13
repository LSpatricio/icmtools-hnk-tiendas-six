Imports Newtonsoft.Json

Public Class IcmQueryResponseDto

    <JsonProperty("columnDefinitions")>
    Public Property ColumnDefinitions As List(Of IcmColumnDefinitionDto)

    <JsonProperty("data")>
    Public Property Data As List(Of List(Of Object))

End Class

Public Class IcmColumnDefinitionDto

    <JsonProperty("name")>
    Public Property Name As String

    <JsonProperty("type")>
    Public Property Type As String

    <JsonProperty("nullable")>
    Public Property Nullable As Boolean

End Class