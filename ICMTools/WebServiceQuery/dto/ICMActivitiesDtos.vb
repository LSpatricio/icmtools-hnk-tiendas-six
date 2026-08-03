Imports Newtonsoft.Json

Public Class RunActivityDto
    <JsonProperty("completedactivities")>
    Public Property CompletedActivities As String
    <JsonProperty("liveactivities")>
    Public Property LiveActivities As String
End Class

Public Class LiveActivitiesDto
    <JsonProperty("progressId")>
    Public Property ProgressId As Integer

    <JsonProperty("userId")>
    Public Property UserId As String

    <JsonProperty("type")>
    Public Property Type As String

    <JsonProperty("status")>
    Public Property Status As String

    <JsonProperty("time")>
    Public Property Time As DateTime

    <JsonProperty("apiServer")>
    Public Property ApiServer As String

    <JsonProperty("percent")>
    Public Property Percent As Integer

    <JsonProperty("description")>
    Public Property Description As String

    <JsonProperty("hasDescription")>
    Public Property HasDescription As Boolean

    <JsonProperty("expiresAt")>
    Public Property ExpiresAt As DateTime

    <JsonProperty("isCancellable")>
    Public Property IsCancellable As Boolean

    <JsonProperty("isInitialization")>
    Public Property IsInitialization As Boolean

    <JsonProperty("computationId")>
    Public Property ComputationId As Integer
End Class

Public Class CompletedActivitiesDto
    <JsonProperty("progressId")>
    Public Property ProgressId As Integer

    <JsonProperty("userId")>
    Public Property UserId As String

    <JsonProperty("type")>
    Public Property Type As String

    <JsonProperty("status")>
    Public Property Status As String

    <JsonProperty("message")>
    Public Property Message As String

    <JsonProperty("time")>
    Public Property Time As DateTime

    <JsonProperty("apiServer")>
    Public Property ApiServer As String
End Class


Public Enum StatusImportacionEnum
    Running
    Cancelled
    Failed
    Completed
    SinRespuesta
End Enum
