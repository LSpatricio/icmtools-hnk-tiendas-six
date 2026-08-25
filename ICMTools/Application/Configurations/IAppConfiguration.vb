Public Interface IAppConfiguration

    ReadOnly Property Maintenance As Boolean

    ReadOnly Property UrlBase As String

    ReadOnly Property HomePage As String

    ReadOnly Property BearerToken As String

    ReadOnly Property ConnectionString As String

    ReadOnly Property SftpHost As String

    ReadOnly Property SftpPort As Integer

    ReadOnly Property SftpUsername As String

    ReadOnly Property SftpPassword As String

    ReadOnly Property SftpRemotePath As String

End Interface
