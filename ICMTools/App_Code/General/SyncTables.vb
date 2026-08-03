Public Class SyncTablesResponse
    Public Property IDTable As Integer
    Public Property Table As String
    Public Property Priority As Integer
    Public Property LastUpdateDate As DateTime
    Public Property LastUpdateAuditID As Long
End Class

Public Class TableMapModel
    Public Property ICMTableName As String
    Public Property PostgreTableName As String
    Public Property StagingTableName As String
    Public Property LastUpdateAuditID As Long
    Public Property Model As String
    Public Property Schedule As String
    Public Property LastUpdateDate As DateTime
End Class

