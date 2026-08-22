Imports System.Configuration

Public Class AppConfiguration
    Implements IAppConfiguration

    Public ReadOnly Property Maintenance As Boolean _
        Implements IAppConfiguration.Maintenance
        Get
            Return Boolean.Parse(
                ConfigurationManager.AppSettings("Maintenance")
            )
        End Get
    End Property

    Public ReadOnly Property UrlBase As String _
        Implements IAppConfiguration.UrlBase
        Get
            Return ConfigurationManager.AppSettings("BASE_URL")
        End Get
    End Property

    Public ReadOnly Property BearerToken As String _
        Implements IAppConfiguration.BearerToken
        Get
            Return ConfigurationManager.AppSettings("BEARER_TOKEN")
        End Get
    End Property

    Public ReadOnly Property HomePage As String _
        Implements IAppConfiguration.HomePage
        Get
            Return ConfigurationManager.AppSettings("HomePage")
        End Get
    End Property

    Public ReadOnly Property ConnectionString As String _
        Implements IAppConfiguration.ConnectionString
        Get
            Return ConfigurationManager.ConnectionStrings(
                "HNK_ICM_TOOLS_BD"
            ).ConnectionString
        End Get
    End Property

    Public ReadOnly Property SftpHost As String _
        Implements IAppConfiguration.SftpHost
        Get
            Return ConfigurationManager.AppSettings("SFTP_HOST")
        End Get
    End Property

    Public ReadOnly Property SftpPort As Integer _
        Implements IAppConfiguration.SftpPort
        Get
            Return Integer.Parse(
                ConfigurationManager.AppSettings("SFTP_PORT")
            )
        End Get
    End Property

    Public ReadOnly Property SftpUsername As String _
        Implements IAppConfiguration.SftpUsername
        Get
            Return ConfigurationManager.AppSettings("SFTP_USERNAME")
        End Get
    End Property

    Public ReadOnly Property SftpPassword As String _
        Implements IAppConfiguration.SftpPassword
        Get
            Return ConfigurationManager.AppSettings("SFTP_PASSWORD")
        End Get
    End Property

    Public ReadOnly Property SftpRemotePath As String _
        Implements IAppConfiguration.SftpRemotePath
        Get
            Return ConfigurationManager.AppSettings("SFTP_REMOTE_PATH")
        End Get
    End Property

End Class