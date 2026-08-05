Imports System.Configuration

Public Class AppConfiguration
    Implements IAppConfiguration

    Public ReadOnly Property Maintenance As Boolean Implements IAppConfiguration.Maintenance

        Get
            Return Boolean.Parse(
                ConfigurationManager.AppSettings("Maintenance"))
        End Get

    End Property
    Public ReadOnly Property UrlBase As String Implements IAppConfiguration.UrlBase

        Get
            Return ConfigurationManager.AppSettings("BASE_URL")
        End Get

    End Property

    Public ReadOnly Property BearerToken As String Implements IAppConfiguration.BearerToken

        Get
            Return ConfigurationManager.AppSettings("BEARER_TOKEN")
        End Get

    End Property
    Public ReadOnly Property HomePage As String Implements IAppConfiguration.HomePage

        Get
            Return ConfigurationManager.AppSettings("HomePage")
        End Get

    End Property

    Public Function ObtenerModelo(model As String) As String Implements IAppConfiguration.ObtenerModelo

        Return ConfigurationManager.AppSettings(model)
    End Function

End Class