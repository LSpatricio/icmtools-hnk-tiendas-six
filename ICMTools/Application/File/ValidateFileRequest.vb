Public Class ValidateFileRequestt

    Property FileClass As String
    Property Path As String

End Class

Public Class ValidateFileRequest
    Property FileType As String
    Property Extension As String
    Property columns As String()
    Property types As String()
    Property nulleable_columns As String()
    Property LogPage As String
    Property LogType As String
    Property LogBody As String
    Property AllowDuplicateEntries As Boolean
    Property FileClass As Type
    Property Path As String
    Property HeaderRow As Integer
End Class