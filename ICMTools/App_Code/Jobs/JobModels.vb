Public Class JobConfig
    Public Property ID As Integer
    Public Property JOB_NAME As String
    Public Property FUNCTION_NAME As String
    Public Property IS_ACTIVE As String
    Public Property Schedules As New List(Of JobSchedule)
End Class

Public Class JobSchedule
    Public Property ID As Integer
    Public Property JOB_ID As Integer
    Public Property CRON_EXPRESSION As String
    Public Property IS_ACTIVE As Boolean
End Class

Public Class LotsResponse
    Public Property O_Lot As Integer
    Public Property O_Status As String
    Public Property O_Date As DateTime
    Public Property O_Subject As String
    Public Property O_Body As String
    Public Property O_To As String
    Public Property O_Cc As String
End Class
