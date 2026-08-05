Public Interface IAppConfiguration

    ReadOnly Property Maintenance As Boolean

    ReadOnly Property UrlBase As String

    ReadOnly Property HomePage As String

    ReadOnly Property BearerToken As String

    Function ObtenerModelo(model As String) As String

End Interface